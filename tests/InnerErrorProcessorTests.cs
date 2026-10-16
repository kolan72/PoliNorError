using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class InnerErrorProcessorTests
	{
		private class TestInnerErrorProcessor : InnerErrorProcessor<ArgumentException>
		{
			public bool ExecuteCalled { get; private set; }
			public ArgumentException CapturedError { get; private set; }
			public ProcessingErrorInfo CapturedInfo { get; private set; }
			public CancellationToken CapturedToken { get; private set; }

			public override void Execute(ArgumentException error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken token = default)
			{
				ExecuteCalled = true;
				CapturedError = error;
				CapturedInfo = catchBlockProcessErrorInfo;
				CapturedToken = token;
			}
		}

		[Test]
		public void Should_ExecuteMethod_WhenInnerExceptionMatchesType()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.True);
			Assert.That(processor.CapturedError, Is.SameAs(innerException));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_NotExecuteMethod_WhenInnerExceptionDoesNotMatchType()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.False);
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_NotExecuteMethod_WhenNoInnerException()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var exception = new InvalidOperationException("Error without inner exception");

			// Act
			var result = processor.Process(exception, null, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.False);
			Assert.That(result, Is.SameAs(exception));
		}

		[Test]
		public void Should_PassProcessingErrorInfo_WhenProvided()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var processingInfo = new ProcessingErrorInfo(new ProcessingErrorContext(PolicyAlias.NotSet));

			// Act
			processor.Process(outerException, processingInfo, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.True);
			Assert.That(processor.CapturedInfo, Is.Not.Null);
			Assert.That(processor.CapturedInfo.CurrentContext.PolicyKind, Is.EqualTo(PolicyAlias.NotSet));
		}

		[Test]
		public void Should_PassCancellationToken_WhenProvided()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			using (var cts = new CancellationTokenSource())
			{
				// Act
				processor.Process(outerException, null, cts.Token);

				// Assert
				Assert.That(processor.ExecuteCalled, Is.True);
				Assert.That(processor.CapturedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public async System.Threading.Tasks.Task Should_ExecuteMethodAsync_WhenInnerExceptionMatchesType()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			// Act
			var result = await processor.ProcessAsync(outerException, null, false, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.True);
			Assert.That(processor.CapturedError, Is.SameAs(innerException));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public async System.Threading.Tasks.Task Should_NotExecuteMethodAsync_WhenInnerExceptionDoesNotMatchType()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = await processor.ProcessAsync(outerException, null, false, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.False);
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_WorkWithDifferentInnerExceptionTypes()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Argument error");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processor.ExecuteCalled, Is.True);
			Assert.That(processor.CapturedError, Is.InstanceOf<ArgumentException>());
			Assert.That(processor.CapturedError.Message, Is.EqualTo("Argument error"));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_ReturnOriginalException_AfterProcessing()
		{
			// Arrange
			var processor = new TestInnerErrorProcessor();
			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(result, Is.SameAs(outerException));
			Assert.That(result.InnerException, Is.SameAs(innerException));
		}
	}
}
