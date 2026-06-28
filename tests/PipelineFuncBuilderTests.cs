using NUnit.Framework;
using System;
using System.Collections.Generic;
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
			string func(int input) => input.ToString();

			// Act
			var result = PipelineFuncBuilder.StartWith((Func<int, string>)func);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<IPipelineFuncStepBuilder<int, int, string>>());
        }

#pragma warning disable RCS1194 // Implement exception constructors.
#pragma warning disable S3871 // Exception types should be "public"
		private class TestException : Exception
		{
			public TestException(string message) : base(message) { }
		}
#pragma warning restore S3871 // Exception types should be "public"
#pragma warning restore RCS1194 // Implement exception constructors.

		#region Build Tests

		[Test]
		public void Should_Build_ReturnFuncThatExecutesInitialFunction()
		{
			// Arrange
			int func(string s) => s.Length;
			var builder = PipelineFuncBuilder.StartWith((Func<string, int>)func);

			// Act
			var pipeline = builder.Build();
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		[Test]
		public void Should_Build_ReturnFuncThatCanBeCalledMultipleTimes()
		{
			// Arrange
			int func(int i) => i * 2;
			var builder = PipelineFuncBuilder.StartWith((Func<int, int>)func);
			var pipeline = builder.Build();

			// Act
			var result1 = pipeline(5, CancellationToken.None);
			var result2 = pipeline(10, CancellationToken.None);

			// Assert
			Assert.That(result1.Result, Is.EqualTo(10));
			Assert.That(result2.Result, Is.EqualTo(20));
		}

		[Test]
		public void Should_Build_ReturnFuncThatHandlesExceptions()
		{
			// Arrange
			int func(string _) => throw new TestException("test error");
			var builder = PipelineFuncBuilder.StartWith((Func<string, int>)func);

			// Act
			var pipeline = builder.Build();
			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
		}

		#endregion

		#region AddFunc Tests

		[Test]
		public void Should_AddFunc_ChainFunctions()
		{
			// Arrange
			int func1(string s) => s.Length;
			string func2(int i) => $"Length: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.AddFunc(func2)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Length: 5"));
		}

		[Test]
		public void Should_AddFunc_ChainMultipleFunctions()
		{
			// Arrange
			int func1(int i) => i + 10;
			int func2(int i) => i * 2;
			string func3(int i) => $"Result: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)func1)
				.AddFunc(func2)
				.AddFunc(func3)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Result: 30"));
		}

		[Test]
		public void Should_AddFunc_NotExecuteNextFunc_WhenPreviousFuncThrows()
		{
			// Arrange
			bool func2Called = false;
			int func1(int _) => throw new TestException("error in func1");
			string func2(int i)
			{
				func2Called = true;
				return i.ToString();
			}

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)func1)
				.AddFunc(func2)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(func2Called, Is.False);
		}

		[Test]
		public void Should_AddFunc_ExecuteInCorrectOrder()
		{
			// Arrange
			var executionOrder = new List<int>();
			int func1(int i) { executionOrder.Add(1); return i + 1; }
			int func2(int i) { executionOrder.Add(2); return i + 1; }
			int func3(int i) { executionOrder.Add(3); return i + 1; }

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)func1)
				.AddFunc(func2)
				.AddFunc(func3)
				.Build();

			pipeline(0, CancellationToken.None);

			// Assert
			Assert.That(executionOrder, Is.EqualTo(new[] { 1, 2, 3 }));
		}

		[Test]
		public void Should_AddFunc_TransformTypesThroughPipeline()
		{
			// Arrange
			int func1(string s) => s.Length;
			double func2(int i) => i * 1.5;
			bool func3(double d) => d > 5.0;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.AddFunc(func2)
				.AddFunc(func3)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.True);
		}

		#endregion

		#region OnError Tests - Sync Action

		[Test]
		public void Should_OnError_SyncAction_CaptureException()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("test error");

			int func1(string _) => throw expectedException;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((ex, _) => capturedException = ex)
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_OnError_SyncAction_CaptureProcessingErrorInfo()
		{
			// Arrange
			ProcessingErrorInfo<string> capturedInfo = null;

			int func1(string _) => throw new TestException("test error");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, pi) => capturedInfo = pi)
				.Build();

			var result = pipeline("test-input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedInfo, Is.Not.Null);
			Assert.That(capturedInfo.Param, Is.EqualTo("test-input"));
		}

		[Test]
		public void Should_OnError_SyncAction_NotExecute_WhenNoException()
		{
			// Arrange
			bool errorHandlerCalled = false;

			int func1(string s) => s.Length;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, __) => errorHandlerCalled = true)
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(errorHandlerCalled, Is.False);
		}

		[Test]
		public void Should_OnError_SyncAction_HandleExceptionInMiddleOfPipeline()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("error in func2");

			int func1(int i) => i + 10;
			int func2(int _) => throw expectedException;
			string func3(int i) => i.ToString();

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)func1)
				.AddFunc(func2)
				.OnError((ex, _) => capturedException = ex)
				.AddFunc(func3)
				.Build();

			var result = pipeline(5, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_OnError_SyncAction_ReturnFluentInterface()
		{
			// Arrange
			int func1(string s) => s.Length;

			// Act
			var builder = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, __) => { });

			// Assert
			Assert.That(builder, Is.InstanceOf<IPipelineFuncBuilder<string, int>>());
		}

		#endregion

		#region OnError Tests - Async Action

		[Test]
		public void Should_OnError_AsyncAction_CaptureException()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("async test error");

			int func1(string _) => throw expectedException;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError(async (ex, _) =>
				{
					await Task.Delay(1);
					capturedException = ex;
				})
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_OnError_AsyncAction_CaptureProcessingErrorInfo()
		{
			// Arrange
			ProcessingErrorInfo<string> capturedInfo = null;

			int func1(string _) => throw new TestException("async test error");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError(async (_, pi) =>
				{
					await Task.Delay(1);
					capturedInfo = pi;
				})
				.Build();

			var result = pipeline("async-input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedInfo, Is.Not.Null);
			Assert.That(capturedInfo.Param, Is.EqualTo("async-input"));
		}

		[Test]
		public void Should_OnError_AsyncAction_NotExecute_WhenNoException()
		{
			// Arrange
			bool errorHandlerCalled = false;

			int func1(string s) => s.Length;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError(async (_, __) =>
				{
					await Task.Delay(1);
					errorHandlerCalled = true;
				})
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(errorHandlerCalled, Is.False);
		}

		[Test]
		public void Should_OnError_AsyncAction_ReturnFluentInterface()
		{
			// Arrange
			int func1(string s) => s.Length;

			// Act
			var builder = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError(async (_, __) => await Task.Delay(1));

			// Assert
			Assert.That(builder, Is.InstanceOf<IPipelineFuncBuilder<string, int>>());
		}

		#endregion

		#region Complex Pipeline Tests

		[Test]
		public void Should_ComplexPipeline_ExecuteAllStepsSuccessfully()
		{
			// Arrange
			int func1(string s) => s.Length;
			int func2(int i) => i * 2;
			string func3(int i) => $"Result: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, __) => { /* error handler 1 */ })
				.AddFunc(func2)
				.OnError((_, __) => { /* error handler 2 */ })
				.AddFunc(func3)
				.OnError((_, __) => { /* error handler 3 */ })
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Result: 10"));
		}

		[Test]
		public void Should_ComplexPipeline_HandleErrorInFirstStep()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("error in step 1");

			int func1(string _) => throw expectedException;
			int func2(int i) => i * 2;
			string func3(int i) => $"Result: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((ex, _) => capturedException = ex)
				.AddFunc(func2)
				.OnError((_, __) => { })
				.AddFunc(func3)
				.OnError((_, __) => { })
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_ComplexPipeline_HandleErrorInMiddleStep()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("error in step 2");

			int func1(string s) => s.Length;
			int func2(int _) => throw expectedException;
			string func3(int i) => $"Result: {i}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, __) => { })
				.AddFunc(func2)
				.OnError((ex, _) => capturedException = ex)
				.AddFunc(func3)
				.OnError((_, __) => { })
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_ComplexPipeline_HandleErrorInLastStep()
		{
			// Arrange
			Exception capturedException = null;
			var expectedException = new TestException("error in step 3");

			int func1(string s) => s.Length;
			int func2(int i) => i * 2;
			string func3(int _) => throw expectedException;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, __) => { })
				.AddFunc(func2)
				.OnError((_, __) => { })
				.AddFunc(func3)
				.OnError((ex, _) => capturedException = ex)
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedException, Is.SameAs(expectedException));
		}

		[Test]
		public void Should_ComplexPipeline_MixSyncAndAsyncErrorHandlers()
		{
			// Arrange
			var exceptions = new List<Exception>();
			var expectedException = new TestException("test error");

			int func1(string _) => throw expectedException;
			int func2(int i) => i * 2;

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((ex, _) =>  exceptions.Add(ex))
				.AddFunc(func2)
				.OnError(async (ex, _) =>
				{
					await Task.Delay(1);
					exceptions.Add(ex);
				})
				.Build();

			var result = pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(exceptions.Count, Is.EqualTo(1));
			Assert.That(exceptions[0], Is.SameAs(expectedException));
		}

		#endregion

		#region ProcessingErrorInfo Tests

		[Test]
		public void Should_ProcessingErrorInfo_ContainCorrectInputParameter()
		{
			// Arrange
			string capturedParam = null;

			int func1(string _) => throw new TestException("error");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, pi) => capturedParam = pi.Param)
				.Build();

			pipeline("test-parameter", CancellationToken.None);

			// Assert
			Assert.That(capturedParam, Is.EqualTo("test-parameter"));
		}

		[Test]
		public void Should_ProcessingErrorInfo_ContainCorrectIntermediateParameter()
		{
			// Arrange
			int capturedParam = 0;

			int func1(string s) => s.Length;
			string func2(int _) => throw new TestException("error");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.AddFunc(func2)
				.OnError((_, pi) => capturedParam = pi.Param)
				.Build();

			pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(capturedParam, Is.EqualTo(5));
		}

		[Test]
		public void Should_ProcessingErrorInfo_HaveErrorContext()
		{
			// Arrange
			ProcessingErrorContext capturedContext = null;

			int func1(string _) => throw new TestException("error");

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.OnError((_, pi) => capturedContext = pi.CurrentContext)
				.Build();

			pipeline("input", CancellationToken.None);

			// Assert
			Assert.That(capturedContext, Is.Not.Null);
		}

		#endregion

		#region OnError(Action<ContextErrorProcessors<TIm>>) Tests

		[Test]
		public void Should_OnError_ContextErrorProcessorsConfigure_ReturnIPipelineFuncBuilder()
		{
			// Arrange
			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(x => x + 1));

			// Act
			var result = builder.ConfigureErrorProcessors((PipelineErrorProcessors<int> _) => { });

			// Assert
			Assert.That(result, Is.InstanceOf<IPipelineFuncBuilder<int, int>>());
		}

		[Test]
		public void Should_OnError_ContextErrorProcessorsConfigure_NotInvokeConfiguredProcessor_WhenNoException()
		{
			// Arrange
			int processorCallCount = 0;
			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(x => x * 3));

			var pipeline = builder
				.ConfigureErrorProcessors(cep => cep.Add((Exception _, ProcessingErrorInfo<int> __) => processorCallCount++))
				.Build();

			// Act
			var result = pipeline(4, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(12));
			Assert.That(processorCallCount, Is.EqualTo(0));
		}

		[Test]
		public void Should_OnError_ContextErrorProcessorsConfigure_InvokeAllConfiguredProcessors_WhenExceptionOccurs()
		{
			// Arrange
			int firstProcessorCount = 0;
			int secondProcessorCount = 0;

			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw new InvalidOperationException("fail")));
			var pipeline = builder
				.ConfigureErrorProcessors(cep =>
				{
					cep.Add((Exception _, ProcessingErrorInfo<int> __) => firstProcessorCount++);
					cep.Add((Exception _, ProcessingErrorInfo<int> __) => secondProcessorCount++);
				})
				.Build();

			// Act
			var result = pipeline(10, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(firstProcessorCount, Is.EqualTo(1));
			Assert.That(secondProcessorCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_OnError_ContextErrorProcessorsConfigure_PassTypedIntermediateParamToProcessor()
		{
			// Arrange
			int capturedParam = -1;
			var expected = new InvalidOperationException("mid-step-failure");

			int func1(string s) => s.Length;
			string func2(int _) => throw expected;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.AddFunc(func2)
				.ConfigureErrorProcessors(cep => cep.Add((ex, pi) =>
				{
					capturedParam = pi.Param;
					Assert.That(ex, Is.SameAs(expected));
				}))
				.Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedParam, Is.EqualTo(5));
		}

		#endregion

		#region ConfigureErrorProcessors(Action<ErrorProcessors>) Tests

		[Test]
		public void Should_ConfigureErrorProcessors_NotInvokeConfiguredProcessor_WhenNoException()
		{
			// Arrange
			int processorCallCount = 0;
			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(x => x * 2));
			var pipeline = builder
				.ConfigureErrorProcessors(ep => ep.Add(_ => processorCallCount++))
				.Build();

			// Act
			var result = pipeline(3, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(6));
			Assert.That(processorCallCount, Is.EqualTo(0));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_InvokeAllConfiguredProcessors_WhenExceptionOccurs()
		{
			// Arrange
			int firstProcessorCount = 0;
			int secondProcessorCount = 0;
			var expected = new InvalidOperationException("fail");

			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw expected));
			var pipeline = builder
				.ConfigureErrorProcessors(ep =>
				{
					ep.Add(ex =>
					{
						firstProcessorCount++;
						Assert.That(ex, Is.SameAs(expected));
					});
					ep.Add(ex =>
					{
						secondProcessorCount++;
						Assert.That(ex, Is.SameAs(expected));
					});
				})
				.Build();

			// Act
			var result = pipeline(10, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(firstProcessorCount, Is.EqualTo(1));
			Assert.That(secondProcessorCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddWithInfo_PassProcessingErrorInfoParam()
		{
			// Arrange
			ProcessingErrorInfo<int> capturedParam = null;
			var expected = new InvalidOperationException("fail-with-info");
			var builder = new PipelineFuncBuilder<int, int, int>(new PipelineDelegateHolder<int, int>(_ => throw expected));
			var pipeline = builder
				.ConfigureErrorProcessors(ep =>
					ep.Add((Exception ex, ProcessingErrorInfo<int> capturedPi) =>
					{
						capturedParam = capturedPi;
						Assert.That(ex, Is.SameAs(expected));
					}))
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedParam.Param, Is.EqualTo(42));
		}

		#endregion

		#region ConfigureErrorProcessors(PipelineErrorProcessors<TIm>.AddForInnerException) Tests

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithAction_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner failure");
			bool invoked = false;
			ArgumentException capturedEx = null;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(ex => { invoked = true; capturedEx = ex; })
				)
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.True);
			Assert.That(capturedEx, Is.SameAs(innerEx));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithAction_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(_ => invoked = true)
				)
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.False);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithAction_NotProcessWhenNoInnerException()
		{
			// Arrange
			bool invoked = false;
			var directEx = new ArgumentException("no inner");

			string func1(int _) => throw directEx;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(_ => invoked = true)
				)
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.False);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndCancellationType_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(_ => invoked = true, CancellationType.Precancelable)
				)
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.True);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndCancellationType_NotProcessWhenPrecancelled()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(_ => invoked = true, CancellationType.Precancelable)
				)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				// Act
				pipeline(42, cts.Token);
			}

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndCancellationToken_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			CancellationToken receivedToken = default;
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.ConfigureErrorProcessors(cep =>
						cep.AddForInnerException((ArgumentException _, CancellationToken token) => { invoked = true; receivedToken = token; })
					)
					.Build();

				// Act
				var result = pipeline(42, cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.True);
				Assert.That(invoked, Is.True);
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndCancellationToken_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.ConfigureErrorProcessors(cep =>
						cep.AddForInnerException((Action<ArgumentException, CancellationToken>)((_, __) => invoked = true))
					)
					.Build();

				// Act
				pipeline(42, cts.Token);

				// Assert
				Assert.That(invoked, Is.False);
			}
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFunc_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			ArgumentException capturedEx = null;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(async ex => { invoked = true; capturedEx = ex; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.True);
			Assert.That(capturedEx, Is.SameAs(innerEx));
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFunc_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(async _ => { invoked = true; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFunc_NotProcessWhenNoInnerException()
		{
			// Arrange
			bool invoked = false;
			var directEx = new ArgumentException("no inner");

			string func1(int _) => throw directEx;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(async _ => { invoked = true; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndCancellationToken_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			CancellationToken receivedToken = default;
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			using (var cts = new CancellationTokenSource())
			{
				const int param = 42;
				CancellationToken ctoken = cts.Token;

				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.ConfigureErrorProcessors(cep =>
						cep.AddForInnerException(async (
							ArgumentException _,
							CancellationToken token) => { invoked = true; receivedToken = token; await Task.CompletedTask; })
					)
					.Build();

				// Act
				await Task.Run(() => pipeline(param, ctoken));

				// Assert
				Assert.That(invoked, Is.True);
				Assert.That(receivedToken, Is.EqualTo(ctoken));
			}
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndCancellationToken_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException(async (ArgumentException _, CancellationToken __) => { invoked = true; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndCancellationType_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			ArgumentException capturedEx = null;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(async ex => { invoked = true; capturedEx = ex; await Task.CompletedTask; }, CancellationType.Precancelable)
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.True);
			Assert.That(capturedEx, Is.SameAs(innerEx));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndProcessingErrorInfo_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			ArgumentException capturedEx = null;
			int? capturedParam = null;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>((ex, info) => { invoked = true; capturedEx = ex; capturedParam = info.Param; })
				)
				.Build();

			// Act
			var result = pipeline(99, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.True);
			Assert.That(capturedEx, Is.SameAs(innerEx));
			Assert.That(capturedParam, Is.EqualTo(99));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndProcessingErrorInfo_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException((Action<ArgumentException, ProcessingErrorInfo<int> >)((_, __) => invoked = true))
				)
				.Build();

			// Act
			pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationToken_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			CancellationToken receivedToken = default;
			bool invoked = false;
			int capturedParam = -1;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.ConfigureErrorProcessors(cep =>
						cep.AddForInnerException((Action<ArgumentException, ProcessingErrorInfo<int>, CancellationToken>)((_, info, token) => { invoked = true; receivedToken = token; capturedParam = info.Param; }))
					)
					.Build();

				// Act
				var result = pipeline(17, cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.True);
				Assert.That(invoked, Is.True);
				Assert.That(capturedParam, Is.EqualTo(17));
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationType_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			int capturedParam = -1;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>((_, info) => { invoked = true; capturedParam = info.Param; }, CancellationType.Precancelable)
				)
				.Build();

			// Act
			var result = pipeline(55, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(invoked, Is.True);
			Assert.That(capturedParam, Is.EqualTo(55));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithActionAndProcessingErrorInfoAndCancellationType_NotProcessWhenPrecancelled()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException(
						(Action<ArgumentException, ProcessingErrorInfo<int>>)((_, __) => invoked = true),
						CancellationType.Precancelable)
				)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var result = pipeline(42, cts.Token);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsFailed, Is.True);
			}

			Assert.That(invoked, Is.False);
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndProcessingErrorInfo_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			ArgumentException capturedEx = null;
			int capturedParam = -1;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException<ArgumentException>(async (ex, info) => { invoked = true; capturedEx = ex; capturedParam = info.Param; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(77, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.True);
			Assert.That(capturedEx, Is.SameAs(innerEx));
			Assert.That(capturedParam, Is.EqualTo(77));
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndProcessingErrorInfo_NotProcessNonMatchingInnerException()
		{
			// Arrange
			var innerEx = new InvalidOperationException("inner");
			bool invoked = false;

			string func1(int _) => throw new Exception("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException(async (
						ArgumentException _,
						ProcessingErrorInfo<int> __) => { invoked = true; await Task.CompletedTask; })
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(42, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.False);
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationToken_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			CancellationToken receivedToken = default;
			bool invoked = false;
			int capturedParam = -1;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, string>)func1)
					.ConfigureErrorProcessors(cep =>
						cep.AddForInnerException(async (
							ArgumentException _,
							ProcessingErrorInfo<int> info,
							CancellationToken token) => { invoked = true; capturedParam = info.Param; receivedToken = token; await Task.CompletedTask; })
					)
					.Build();

				// Act
				await Task.Run(() => pipeline(33, cts.Token));

				// Assert
				Assert.That(invoked, Is.True);
				Assert.That(capturedParam, Is.EqualTo(33));
				Assert.That(receivedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public async Task Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationType_ProcessMatchingInnerException()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;
			int capturedParam = -1;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException(
						async (
							ArgumentException _,
							ProcessingErrorInfo<int> info) => { invoked = true; capturedParam = info.Param; await Task.CompletedTask; },
						CancellationType.Precancelable)
				)
				.Build();

			// Act
			await Task.Run(() => pipeline(88, CancellationToken.None));

			// Assert
			Assert.That(invoked, Is.True);
			Assert.That(capturedParam, Is.EqualTo(88));
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_WithFuncAndProcessingErrorInfoAndCancellationType_NotProcessWhenPrecancelled()
		{
			// Arrange
			var innerEx = new ArgumentException("inner");
			bool invoked = false;

			string func1(int _) => throw new InvalidOperationException("outer", innerEx);

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.ConfigureErrorProcessors(cep =>
					cep.AddForInnerException(async (ArgumentException _, ProcessingErrorInfo<int> __) => { invoked = true; await Task.CompletedTask; }, CancellationType.Precancelable)
				)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var result = pipeline(42, cts.Token);
				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsFailed, Is.True);
			}

			Assert.That(invoked, Is.False);
		}

		[Test]
		public void Should_ConfigureErrorProcessors_AddForInnerException_ReturnFluentBuilder()
		{
			// Arrange
			var builder = new PipelineFuncBuilder<int, int, string>(new PipelineDelegateHolder<int, string>(x => x.ToString()));

			// Act
			var result = builder.ConfigureErrorProcessors(cep => cep.AddForInnerException<ArgumentException>(_ => { }));

			// Assert
			Assert.That(result, Is.InstanceOf<IPipelineFuncBuilder<int, string>>());
		}

		#endregion

		[Test]
		public void Should_Pipeline_HandleNullInput()
		{
			// Arrange
			int func1(string s) => s?.Length ?? 0;
			var pipeline = PipelineFuncBuilder.StartWith((Func<string, int>)func1).Build();

			var result = pipeline(null, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(0));
		}

		[Test]
		public void Should_Pipeline_HandleDefaultValueTypes()
		{
			// Arrange
			int func1(int i) => i + 1;
			var pipeline = PipelineFuncBuilder.StartWith((Func<int, int>)func1).Build();

			var result = pipeline(0, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(1));
		}

		[Test]
		public void Should_Pipeline_WorkWithComplexTypes()
		{
			// Arrange
			int func1(List<int> list) => list.Count;
			string func2(int count) => $"Count: {count}";

			// Act
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<List<int>, int>)func1)
				.AddFunc(func2)
				.Build();

			var result = pipeline(new List<int> { 1, 2, 3 }, CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo("Count: 3"));
		}

		#region CancellationToken Tests

		[Test]
		public void Should_Pipeline_AcceptCancellationToken()
		{
			// Arrange
			int func1(string s) => s.Length;
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				// Act
				var result = pipeline("hello", cts.Token);

				// Assert
				Assert.That(result.IsFailed, Is.False);
				Assert.That(result.Result, Is.EqualTo(5));
			}
		}

		[Test]
		public void Should_Pipeline_WorkWithNoneCancellationToken()
		{
			// Arrange
			int func1(string s) => s.Length;
			var pipeline = PipelineFuncBuilder.StartWith((Func<string, int>)func1).Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(5));
		}

		#endregion

		#region Result Tests

		[Test]
		public void Should_PipelineResult_HaveIsFalseWhenSuccessful()
		{
			// Arrange
			int func1(string s) => s.Length;
			var pipeline = PipelineFuncBuilder.StartWith((Func<string, int>)func1).Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.False);
		}

		[Test]
		public void Should_PipelineResult_HaveIsFailedTrueWhenException()
		{
			// Arrange
			int func1(string _) => throw new TestException("error");
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
		}

		[Test]
		public void Should_PipelineResult_ContainCorrectResultValue()
		{
			// Arrange
			string func1(int i) => $"Value: {i}";
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, string>)func1)
				.Build();

			// Act
			var result = pipeline(42, CancellationToken.None);

			// Assert
			Assert.That(result.Result, Is.EqualTo("Value: 42"));
		}

		[Test]
		public void Should_PipelineResult_ReturnDefaultWhenFailed()
		{
			// Arrange
			int func1(string _) => throw new TestException("error");
			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)func1)
				.Build();

			// Act
			var result = pipeline("hello", CancellationToken.None);

			// Assert
			Assert.That(result.IsFailed, Is.True);
			Assert.That(result.Result, Is.EqualTo(default(int)));
		}

		#endregion

		[Test]
		public void Should_OnError_WithCancellationToken_InvokeProcessor_WhenStepThrows()
		{
			var expected = new InvalidOperationException("fail");
			Exception capturedException = null;
			ProcessingErrorInfo<int> capturedInfo = null;
			CancellationToken capturedToken = default;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)(_ => throw expected))
				.OnError((ex, info, token) =>
				{
					capturedException = ex;
					capturedInfo = info;
					capturedToken = token;
				})
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				var result = pipeline(7, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(capturedException, Is.SameAs(expected));
				Assert.That(capturedInfo, Is.Not.Null);
				Assert.That(capturedInfo.Param, Is.EqualTo(7));
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_OnError_WithCancellationToken_NotInvokeProcessor_WhenNoException()
		{
			var wasCalled = false;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)(x => x + 1))
				.OnError((Exception _, ProcessingErrorInfo<int> __, CancellationToken ___) => wasCalled = true)
				.Build();

			var result = pipeline(3, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(wasCalled, Is.False);
		}

		[Test]
		public void Should_OnError_WithCancellationToken_ReceiveCanceledToken()
		{
			var expected = new InvalidOperationException("boom");
			CancellationToken capturedToken = default;
			var tokenWasCanceled = false;

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, int>)(_ => throw expected))
					.OnError((Exception _, ProcessingErrorInfo<int> __, CancellationToken token) =>
					{
						cts.Cancel();
						capturedToken = token;
						tokenWasCanceled = token.IsCancellationRequested;
					})
					.Build();

				var result = pipeline(1, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
				Assert.That(tokenWasCanceled, Is.True);
			}
		}

		[Test]
		public void Should_OnError_WithCancellationToken_WorkForIntermediateStepError()
		{
			var expected = new InvalidOperationException("middle-step-failure");
			int capturedIntermediate = -1;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)(s => s.Length))
				.AddFunc((Func<int, string>)(_ => throw expected))
				.OnError((ex, info, _) =>
				{
					Assert.That(ex, Is.SameAs(expected));
					capturedIntermediate = info.Param;
				})
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedIntermediate, Is.EqualTo(5));
		}

		[Test]
		public void Should_OnError_WithCancellationToken_ReturnFluentBuilder()
		{
			var builder = PipelineFuncBuilder
				.StartWith((Func<int, int>)(x => x))
				.OnError((Exception _, ProcessingErrorInfo<int> __, CancellationToken ___) => { });

			Assert.That(builder, Is.InstanceOf<IPipelineFuncBuilder<int, int>>());
		}

		[Test]
		public void Should_OnError_AsyncWithCancellationToken_InvokeProcessor_WhenStepThrows()
		{
			var expected = new InvalidOperationException("async-fail");
			Exception capturedException = null;
			ProcessingErrorInfo<int> capturedInfo = null;
			CancellationToken capturedToken = default;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)(_ => throw expected))
				.OnError(async (Exception ex, ProcessingErrorInfo<int> info, CancellationToken token) =>
				{
					await Task.Delay(1);
					capturedException = ex;
					capturedInfo = info;
					capturedToken = token;
				})
				.Build();

			using (var cts = new CancellationTokenSource())
			{
				var result = pipeline(7, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(capturedException, Is.SameAs(expected));
				Assert.That(capturedInfo, Is.Not.Null);
				Assert.That(capturedInfo.Param, Is.EqualTo(7));
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public void Should_OnError_AsyncWithCancellationToken_NotInvokeProcessor_WhenNoException()
		{
			var wasCalled = false;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<int, int>)(x => x + 1))
				.OnError(async (Exception _, ProcessingErrorInfo<int> __, CancellationToken ___) =>
				{
					await Task.Delay(1);
					wasCalled = true;
				})
				.Build();

			var result = pipeline(3, CancellationToken.None);

			Assert.That(result.IsFailed, Is.False);
			Assert.That(result.Result, Is.EqualTo(4));
			Assert.That(wasCalled, Is.False);
		}

		[Test]
		public void Should_OnError_AsyncWithCancellationToken_ReceiveCanceledToken()
		{
			var expected = new InvalidOperationException("async-boom");
			CancellationToken capturedToken = default;
			var tokenWasCanceled = false;

			using (var cts = new CancellationTokenSource())
			{
				var pipeline = PipelineFuncBuilder
					.StartWith((Func<int, int>)(_ => throw expected))
					.OnError(async (Exception _, ProcessingErrorInfo<int> __, CancellationToken token) =>
					{
						//HACK
						await Task.Delay(1);

						cts.Cancel();

						capturedToken = token;
						tokenWasCanceled = token.IsCancellationRequested;
					})
					.Build();

				var result = pipeline(1, cts.Token);

				Assert.That(result.IsFailed, Is.True);
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
				Assert.That(tokenWasCanceled, Is.True);
			}
		}

		[Test]
		public void Should_OnError_AsyncWithCancellationToken_WorkForIntermediateStepError()
		{
			var expected = new InvalidOperationException("async-middle-step-failure");
			int capturedIntermediate = -1;

			var pipeline = PipelineFuncBuilder
				.StartWith((Func<string, int>)(s => s.Length))
				.AddFunc((Func<int, string>)(_ => throw expected))
				.OnError(async (Exception ex, ProcessingErrorInfo<int> info, CancellationToken _) =>
				{
					await Task.Delay(1);
					Assert.That(ex, Is.SameAs(expected));
					capturedIntermediate = info.Param;
				})
				.Build();

			var result = pipeline("hello", CancellationToken.None);

			Assert.That(result.IsFailed, Is.True);
			Assert.That(capturedIntermediate, Is.EqualTo(5));
		}

		[Test]
		public void Should_OnError_AsyncWithCancellationToken_ReturnFluentBuilder()
		{
			var builder = PipelineFuncBuilder
				.StartWith((Func<int, int>)(x => x))
				.OnError((Exception _, ProcessingErrorInfo<int> __, CancellationToken ___) => Task.CompletedTask);

			Assert.That(builder, Is.InstanceOf<IPipelineFuncBuilder<int, int>>());
		}
	}
}
