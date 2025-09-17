using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SyncErrorProcessorTests
	{
		private TestLogger _logger;
		private LogErrorProcessor _processor;

		[SetUp]
		public void Setup()
		{
			_logger = new TestLogger();
			_processor = new LogErrorProcessor(_logger);
		}

		[Test]
		public void Should_ProcessException_When_ExecuteIsCalledWithException()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");

			// Act
			_processor.Execute(exception);

			// Assert
			Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
		}

		[Test]
		public void Should_ProcessException_When_ExecuteIsCalledWithExceptionAndProcessingErrorInfo()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Retry);

			// Act
			_processor.Execute(exception, errorInfo);

			// Assert
			Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
		}

		[Test]
		public void Should_ProcessException_When_ProcessIsCalled()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");

			// Act
			var result = _processor.Process(exception);

			// Assert
			Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
			Assert.That(result, Is.SameAs(exception));
		}

		[Test]
		public void Should_ProcessException_When_ProcessAsyncIsCalled()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");

			// Act
			var resultTask = _processor.ProcessAsync(exception);
			var result = resultTask.GetAwaiter().GetResult();

			// Assert
			Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
			Assert.That(result, Is.SameAs(exception));
		}

		[Test]
		public void Should_ProcessException_When_ProcessAsyncIsCalledWithProcessingErrorInfo()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");
			var errorInfo = new ProcessingErrorInfo(PolicyAlias.Retry);

			// Act
			var resultTask = _processor.ProcessAsync(exception, errorInfo);
			var result = resultTask.GetAwaiter().GetResult();

			// Assert
			Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
			Assert.That(result, Is.SameAs(exception));
		}

		[Test]
		public void Should_SetIsCanceledToTrue_When_ProcessIsCalledWithCanceledToken()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				// Act
				var result = _processor.Process(exception, null, cts.Token);

				// Assert
				Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(result, Is.SameAs(exception));
				Assert.That(_processor.IsCanceled, Is.True);
			}
		}

		[Test]
		public async Task Should_SetIsCanceledToTrue_When_ProcessAsyncIsCalledWithCanceledToken()
		{
			// Arrange
			var exception = new InvalidOperationException("Test error");
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();

				// Act
				var result = await _processor.ProcessAsync(exception, null, false, cts.Token);

				// Assert
				Assert.That(_logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(result, Is.SameAs(exception));
				Assert.That(_processor.IsCanceled, Is.True);
			}
		}

		[Test]
		public void Should_Process_By_BulkErrorProcessor()
		{
			var exception = new InvalidOperationException("Test error");
			var bp = new BulkErrorProcessor().WithErrorProcessor(_processor);
			var result = bp.Process(exception);
			Assert.That(result.HandlingError, Is.SameAs(exception));
		}

		[Test]
		public async Task Should_ProcessAsync_By_BulkErrorProcessor()
		{
			var exception = new InvalidOperationException("Test error");
			var bp = new BulkErrorProcessor().WithErrorProcessor(_processor);
			var result = await bp.ProcessAsync(exception);
			Assert.That(result.HandlingError, Is.SameAs(exception));
		}
	}

	public interface ILogger
	{
		void LogError(Exception exception);
	}
	public class LogErrorProcessor : ErrorProcessor
	{
		private readonly ILogger _logger;

		public LogErrorProcessor(ILogger logger)
		{
			_logger = logger;
		}

		public override void Execute(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
			{
				IsCanceled = true;
			}
			_logger.LogError(error);
		}

		public bool IsCanceled { get; private set; }
	}

	public class TestLogger : ILogger
	{
		public Exception LastLoggedException { get; private set; }

		public void LogError(Exception exception)
		{
			LastLoggedException = exception;
		}
	}
}
