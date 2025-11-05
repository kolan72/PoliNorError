using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class SyncErrorProcessorTContextTests
	{
		[Test]
		[TestCase(ContextCall.None)]
		[TestCase(ContextCall.CorrectType)]
		[TestCase(ContextCall.IncorrectType)]
		public void Should_Process_For_KnownProcessor_SyncMethod(ContextCall contextCall)
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);
			var processor = new SimplePolicyProcessor()
						.WithErrorProcessor(errProcessor);

			PolicyResult result = null;

			if (contextCall == ContextCall.CorrectType)
			{
				const int correctContext = 5;
				result = processor.Execute(() => throw exception, correctContext);
				Assert.That(logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(logger.Context, Is.EqualTo(correctContext));
			}
			else
			{
				switch (contextCall)
				{
					case ContextCall.IncorrectType:
						const string wrongContext = "5";
						result = processor.Execute(() => throw exception, wrongContext);
						break;
					case ContextCall.None:
						result = processor.Execute(() => throw exception);
						break;
				}
				Assert.That(logger.LastLoggedException, Is.Null);
				Assert.That(logger.Context, Is.Default);
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(ContextCall.None)]
		[TestCase(ContextCall.CorrectType)]
		[TestCase(ContextCall.IncorrectType)]
		public async Task Should_ProcessAsync_For_KnownProcessor_AyncMethod(ContextCall contextCall)
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);
			var processor = new SimplePolicyProcessor()
						.WithErrorProcessor(errProcessor);

			PolicyResult result = null;

			if (contextCall == ContextCall.CorrectType)
			{
				const int correctContext = 5;
				result = await processor.ExecuteAsync((_) => throw exception, correctContext);
				Assert.That(logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(logger.Context, Is.EqualTo(correctContext));
			}
			else
			{
				switch (contextCall)
				{
					case ContextCall.IncorrectType:
						const string wrongContext = "5";
						result = await processor.ExecuteAsync((_) => throw exception, wrongContext);
						break;
					case ContextCall.None:
						result = await processor.ExecuteAsync((_) => throw exception);
						break;
				}
				Assert.That(logger.LastLoggedException, Is.Null);
				Assert.That(logger.Context, Is.Default);
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(ContextCall.None)]
		[TestCase(ContextCall.CorrectType)]
		[TestCase(ContextCall.IncorrectType)]
		public void Should_Process_For_KnownPolicy_SyncMethod(ContextCall contextCall)
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);
			var policy = new SimplePolicy()
						.WithErrorProcessor(errProcessor);

			PolicyResult result = null;

			if (contextCall == ContextCall.CorrectType)
			{
				const int correctContext = 5;
				result = policy.Handle(() => throw exception, correctContext);
				Assert.That(logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(logger.Context, Is.EqualTo(correctContext));
			}
			else
			{
				switch (contextCall)
				{
					case ContextCall.IncorrectType:
						const string wrongContext = "5";
						result = policy.Handle(() => throw exception, wrongContext);
						break;
					case ContextCall.None:
						result = policy.Handle(() => throw exception);
						break;
				}
				Assert.That(logger.LastLoggedException, Is.Null);
				Assert.That(logger.Context, Is.Default);
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(ContextCall.None)]
		[TestCase(ContextCall.CorrectType)]
		[TestCase(ContextCall.IncorrectType)]
		public async Task Should_ProcessAsync_For_KnownPolicy_AsyncMethod(ContextCall contextCall)
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);
			var policy = new SimplePolicy()
						.WithErrorProcessor(errProcessor);

			PolicyResult result = null;

			if (contextCall == ContextCall.CorrectType)
			{
				const int correctContext = 5;
				result = await policy.HandleAsync((_) => throw exception, correctContext, default);
				Assert.That(logger.LastLoggedException, Is.SameAs(exception));
				Assert.That(logger.Context, Is.EqualTo(correctContext));
			}
			else
			{
				switch (contextCall)
				{
					case ContextCall.IncorrectType:
						const string wrongContext = "5";
						result = await policy.HandleAsync((_) => throw exception, wrongContext, default);
						break;
					case ContextCall.None:
						result = await policy.HandleAsync((_) => throw exception);
						break;
				}
				Assert.That(logger.LastLoggedException, Is.Null);
				Assert.That(logger.Context, Is.Default);
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		public void Should_Process_By_BulkErrorProcessor()
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);

			var bp = new BulkErrorProcessor().WithErrorProcessor(errProcessor);
			var _ = bp.Process(exception, new ProcessingErrorContext<int>(PolicyAlias.NotSet, 5));
			Assert.That(logger.LastLoggedException, Is.SameAs(exception));
			Assert.That(logger.Context, Is.EqualTo(5));
		}

		[Test]
		public async Task Should_ProcessAsync_By_BulkErrorProcessor()
		{
			var exception = new InvalidOperationException("Test error");

			var logger = new TestLoggerWithContext();
			var errProcessor = new LogErrorProcessorWithContext(logger);

			var bp = new BulkErrorProcessor().WithErrorProcessor(errProcessor);
			var _ = await bp.ProcessAsync(exception, new ProcessingErrorContext<int>(PolicyAlias.NotSet, 5));
			Assert.That(logger.LastLoggedException, Is.SameAs(exception));
			Assert.That(logger.Context, Is.EqualTo(5));
		}
	}

	public interface ILoggerWithContext<in TContext>
	{
		void LogError(Exception exception, TContext context);
	}

	public class TestLoggerWithContext : ILoggerWithContext<int>
	{
		public Exception LastLoggedException { get; private set; }

		public int Context { get; private set; }

		public void LogError(Exception exception, int context)
		{
			LastLoggedException = exception;
			Context = context;
		}
	}

	public class LogErrorProcessorWithContext : ErrorProcessor<int>
	{
		private readonly ILoggerWithContext<int> _logger;

		public LogErrorProcessorWithContext(ILoggerWithContext<int> logger)
		{
			_logger = logger;
		}

		public override void Execute(Exception error, ProcessingErrorInfo<int> catchBlockProcessErrorInfo = null, CancellationToken token = default)
		{
			if (token.IsCancellationRequested)
			{
				IsCanceled = true;
			}
			_logger.LogError(error, catchBlockProcessErrorInfo?.Param ?? 0);
		}

		public bool IsCanceled { get; private set; }
	}

	public enum ContextCall
	{
		None,
		CorrectType,
		IncorrectType
	}
}
