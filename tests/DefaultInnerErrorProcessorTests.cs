using NUnit.Framework;
using System;
using System.Threading;

namespace PoliNorError.Tests
{
	[TestFixture]
	public class DefaultInnerErrorProcessorTests
	{
		[Test]
		public void Should_ProcessException_WhenInnerExceptionMatchesType()
		{
			// Arrange
			bool processorCalled = false;
			ArgumentException capturedException = null;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>((ex, _) =>
			{
				processorCalled = true;
				capturedException = ex;
			});

			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.True);
			Assert.That(capturedException, Is.SameAs(innerException));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_NotProcessException_WhenInnerExceptionDoesNotMatchType()
		{
			// Arrange
			bool processorCalled = false;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>((_, __) => processorCalled = true);

			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.False);
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_NotProcessException_WhenNoInnerException()
		{
			// Arrange
			bool processorCalled = false;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>((_, __) => processorCalled = true);

			var exception = new InvalidOperationException("Error without inner exception");

			// Act
			var result = processor.Process(exception, null, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.False);
			Assert.That(result, Is.SameAs(exception));
		}

		[Test]
		public void Should_ProcessException_WhenInnerExceptionIsExactType()
		{
			// Arrange
			bool processorCalled = false;
			NullReferenceException capturedException = null;

			var processor = new DefaultInnerErrorProcessor<NullReferenceException>((ex, _) =>
			{
				processorCalled = true;
				capturedException = ex;
			});

			var innerException = new NullReferenceException("Null reference");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = processor.Process(outerException, null, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.True);
			Assert.That(processorCalled, Is.True);
			Assert.That(capturedException, Is.SameAs(innerException));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public void Should_ReceiveProcessingErrorInfo_WhenProvided()
		{
			// Arrange
			ProcessingErrorInfo capturedInfo = null;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>((_, pi) => capturedInfo = pi);

			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);
			var processingInfo = new ProcessingErrorInfo(new ProcessingErrorContext(PolicyAlias.NotSet));

			// Act
			processor.Process(outerException, processingInfo, CancellationToken.None);

			// Assert
			Assert.That(capturedInfo, Is.Not.Null);
			Assert.That(capturedInfo.CurrentContext.PolicyKind, Is.EqualTo(PolicyAlias.NotSet));
		}

		[Test]
		public void Should_WorkWithCancellationToken_WhenProvided()
		{
			// Arrange
			CancellationToken capturedToken = default;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>((_, __, ct) => capturedToken = ct);

			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			using (var cts = new CancellationTokenSource())
			{
				// Act
				processor.Process(outerException, null, cts.Token);

				// Assert
				Assert.That(capturedToken, Is.EqualTo(cts.Token));
			}
		}

		[Test]
		public async System.Threading.Tasks.Task Should_ProcessExceptionAsync_WhenInnerExceptionMatchesType()
		{
			// Arrange
			bool processorCalled = false;
			ArgumentException capturedException = null;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>(async (ex, _) =>
			{
				await System.Threading.Tasks.Task.Delay(1);
				processorCalled = true;
				capturedException = ex;
			});

			var innerException = new ArgumentException("Inner error");
			var outerException = new InvalidOperationException("Outer error", innerException);

			// Act
			var result = await processor.ProcessAsync(outerException, null, false, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.True);
			Assert.That(capturedException, Is.SameAs(innerException));
			Assert.That(result, Is.SameAs(outerException));
		}

		[Test]
		public async System.Threading.Tasks.Task Should_NotProcessExceptionAsync_WhenInnerExceptionDoesNotMatchType()
		{
			// Arrange
			bool processorCalled = false;

			var processor = new DefaultInnerErrorProcessor<ArgumentException>(async (_, __) =>
			{
				await System.Threading.Tasks.Task.Delay(1);
				processorCalled = true;
			});

			var innerException = new InvalidOperationException("Inner error");
			var outerException = new Exception("Outer error", innerException);

			// Act
			var result = await processor.ProcessAsync(outerException, null, false, CancellationToken.None);

			// Assert
			Assert.That(processorCalled, Is.False);
			Assert.That(result, Is.SameAs(outerException));
		}
	}
}
