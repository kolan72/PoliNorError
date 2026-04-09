using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class SyncTypedErrorProcessorTests
	{
#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3871 // Exception types should be "public"
		private class TestException : Exception
		{
            public TestException(string message) : base(message) { }

            public string TestProperty { get; set; }
        }

        private class TestSubException : TestException
        {
            public TestSubException(string msg) : base(msg) { }
        }
#pragma warning restore S3871 // Exception types should be "public"
#pragma warning restore RCS1194 // Implement exception constructors.

        private class TestTypedErrorProcessor : TypedErrorProcessor<TestException>
        {
            public int ExecuteCallCount { get; private set; }
            public TestException LastException { get; private set; }
            public ProcessingErrorInfo LastProcessingErrorInfo { get; private set; }
            public CancellationToken LastCancellationToken { get; private set; }

            public override void Execute(TestException error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken token = default)
            {
                ExecuteCallCount++;
                LastException = error;
                LastProcessingErrorInfo = catchBlockProcessErrorInfo;
                LastCancellationToken = token;
                error.TestProperty = nameof(error.TestProperty);
            }
        }

        [Test]
        public void Should_CallExecute_WhenProcessIsCalled()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            var result = processor.Process(exception);

            // Assert
            Assert.That(processor.ExecuteCallCount, Is.EqualTo(1));
            Assert.That(processor.LastException, Is.SameAs(exception));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
            Assert.That(result, Is.SameAs(exception));
        }

        [Test]
        public void Should_Not_CallExecute_WhenProcessIsCalled()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestSubException("test error");

            // Act
           processor.Process(exception);

            // Assert
            Assert.That(processor.ExecuteCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Should_ReturnSameException_WhenProcessIsCalled()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            var result = processor.Process(exception);

            // Assert
            Assert.That(result, Is.SameAs(exception));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public void Should_PassProcessingErrorInfo_WhenProvided()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");
            var errorInfo = new ProcessingErrorInfo(new ProcessingErrorContext());

            // Act
            processor.Process(exception, errorInfo);

            // Assert
            Assert.That(processor.LastProcessingErrorInfo, Is.SameAs(errorInfo));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public void Should_PassCancellationToken_WhenProvided()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");
			using (var cts = new CancellationTokenSource())
			{
				// Act
				processor.Process(exception, null, cts.Token);

				// Assert
				Assert.That(processor.LastCancellationToken, Is.EqualTo(cts.Token));
			}
		}

        [Test]
        public void Should_PassDefaultCancellationToken_WhenNotProvided()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            processor.Process(exception);

            // Assert
            Assert.That(processor.LastCancellationToken, Is.EqualTo(default(CancellationToken)));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public async Task Should_CallExecute_WhenProcessAsyncIsCalled()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            var result = await processor.ProcessAsync(exception);

            // Assert
            Assert.That(processor.ExecuteCallCount, Is.EqualTo(1));
            Assert.That(processor.LastException, Is.SameAs(exception));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
            Assert.That(result, Is.SameAs(exception));
        }

        [Test]
        public async Task Should_ReturnSameException_WhenProcessAsyncIsCalled()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            var result = await processor.ProcessAsync(exception);

            // Assert
            Assert.That(result, Is.SameAs(exception));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public async Task Should_PassProcessingErrorInfo_WhenProvidedToProcessAsync()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");
            var errorInfo = new ProcessingErrorInfo(new ProcessingErrorContext());

            // Act
            await processor.ProcessAsync(exception, errorInfo);

            // Assert
            Assert.That(processor.LastProcessingErrorInfo, Is.SameAs(errorInfo));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public async Task Should_PassCancellationToken_WhenProvidedToProcessAsync()
		{
			// Arrange
			var processor = new TestTypedErrorProcessor();
			var exception = new TestException("test error");
			using (var cts = new CancellationTokenSource())
			{
				// Act
				await processor.ProcessAsync(exception, null, false, cts.Token);

				// Assert
				Assert.That(processor.LastCancellationToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
        public async Task Should_CompleteSuccessfully_WhenProcessAsyncReturnsTask()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            var task = processor.ProcessAsync(exception);

            // Assert
            Assert.That(task, Is.Not.Null);
            Assert.That(task.IsCompleted || await Task.WhenAny(task, Task.Delay(100)) == task, Is.True);
            Assert.That(task.Result, Is.SameAs(exception));
        }

        [Test]
        public void Should_HandleMultipleProcessCalls_Independently()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception1 = new TestException("error 1");
            var exception2 = new TestException("error 2");

            // Act
            processor.Process(exception1);
            processor.Process(exception2);

            // Assert
            Assert.That(processor.ExecuteCallCount, Is.EqualTo(2));
            Assert.That(processor.LastException, Is.SameAs(exception2));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public async Task Should_HandleMultipleProcessAsyncCalls_Independently()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception1 = new TestException("error 1");
            var exception2 = new TestException("error 2");

            // Act
            await processor.ProcessAsync(exception1);
            await processor.ProcessAsync(exception2);

            // Assert
            Assert.That(processor.ExecuteCallCount, Is.EqualTo(2));
            Assert.That(processor.LastException, Is.SameAs(exception2));
            Assert.That(processor.LastException.TestProperty, Is.EqualTo(nameof(TestException.TestProperty)));
        }

        [Test]
        public void Should_ProcessNullProcessingErrorInfo_WhenNotProvided()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act
            processor.Process(exception, null);

            // Assert
            Assert.That(processor.LastProcessingErrorInfo, Is.Null);
        }

        [Test]
        public void Should_AllowConfigAwaitParameter_InAsyncProcess()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();
            var exception = new TestException("test error");

            // Act & Assert - should not throw
            Assert.DoesNotThrowAsync(async () => await processor.ProcessAsync(exception, null, true));
            Assert.DoesNotThrowAsync(async () => await processor.ProcessAsync(exception, null, false));
        }

        [Test]
        public void Should_ImplementIErrorProcessor_Interface()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();

            // Assert
            Assert.That(processor, Is.InstanceOf<IErrorProcessor>());
        }

        [Test]
        public void Should_InheritFromTypedErrorProcessorBase()
        {
            // Arrange
            var processor = new TestTypedErrorProcessor();

            // Assert
            Assert.That(processor, Is.InstanceOf<TypedErrorProcessor<TestException>>());
        }
    }
}
