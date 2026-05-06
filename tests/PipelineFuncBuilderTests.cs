using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
    [TestFixture]
    public class PipelineFuncBuilderTests
    {
        [Test]
        public void Should_Build_ReturnSuccessfulResult_FromInitialDelegate()
        {
            var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(x => x + 5));

            var pipeline = builder.Build();
            var result = pipeline(7, CancellationToken.None);

            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(12));
        }

        [Test]
        public void Should_AddFunc_ComposePipelineAndTransformOutput()
        {
            var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(x => x + 2));

            var pipeline = builder
                .AddFunc(x => x * 3)
                .Build();

            var result = pipeline(4, CancellationToken.None);

            Assert.That(result.IsFailed, Is.False);
            Assert.That(result.Result, Is.EqualTo(18));
        }

        [Test]
        public void Should_AddFunc_NotExecuteNextFunc_WhenPreviousStepFails()
        {
            var expected = new InvalidOperationException("boom");
            var nextCalled = false;
            var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw expected));

            var pipeline = builder
                .AddFunc(x =>
                {
                    nextCalled = true;
                    return x * 2;
                })
                .Build();

            var result = pipeline(10, CancellationToken.None);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(nextCalled, Is.False);
        }

        [Test]
        public void Should_OnError_ActionProcessor_HandleThrownException()
        {
            var expected = new InvalidOperationException("sync-failure");
            Exception receivedException = null;
            ProcessingErrorInfo<int> receivedInfo = null;

            var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw expected));
            var pipeline = builder
                .OnError((ex, info) =>
                {
                    receivedException = ex;
                    receivedInfo = info;
                })
                .Build();

            var result = pipeline(1, CancellationToken.None);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(receivedException, Is.SameAs(expected));
            Assert.That(receivedInfo, Is.Not.Null);
            Assert.That(receivedInfo.Param, Is.EqualTo(1));
        }

        [Test]
        public void Should_OnError_AsyncProcessor_HandleThrownException()
        {
            var expected = new InvalidOperationException("async-failure");
            Exception receivedException = null;
            ProcessingErrorInfo<int> receivedInfo = null;

            var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw expected));
            var pipeline = builder
                .OnError((ex, info) =>
                {
                    receivedException = ex;
                    receivedInfo = info;
                    return Task.CompletedTask;
                })
                .Build();

            var result = pipeline(2, CancellationToken.None);

            Assert.That(result.IsFailed, Is.True);
            Assert.That(receivedException, Is.SameAs(expected));
            Assert.That(receivedInfo, Is.Not.Null);
            Assert.That(receivedInfo.Param, Is.EqualTo(2));
        }

        [Test]
        public void Should_Create_PipelineFuncBuilder_When_Calling_StartWith_With_Valid_Func()
        {
            // Arrange
            Func<int, string> func = input => input.ToString();

            // Act
            var result = PipelineFuncBuilder.StartWith(func);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IPipelineWithHandlersBuilder<int, int, string>>());
        }
    }
}
