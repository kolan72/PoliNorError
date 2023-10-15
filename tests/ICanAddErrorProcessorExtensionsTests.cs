using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class ICanAddErrorProcessorExtensionsTests
	{
		[Test]
		[TestCase(TestType.PolicyProc)]
		[TestCase(TestType.BulkErrorProc)]
		public void Should_WithErrorProcessorOf_AddErrorProcessors(TestType testType)
		{
			int errorProcessorsCount = 1;
			IErrorProcessorRegistration v = null;

			if (testType == TestType.PolicyProc)
			{
				v = new PolicyProcessorErrorProcessorRegistration();
			}
			else
			{
				v = new BulkErrorProcessorErrorProcessorRegistration();
			}

			v.WithErrorProcessorOf((Exception _, CancellationToken __) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __, CancellationToken ___) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf((Exception _) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1));
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, CancellationToken __) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1));
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __, CancellationToken ___) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1));
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), (Exception _, ProcessingErrorInfo __) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _, ProcessingErrorInfo __) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1));
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), (_) => Expression.Empty(), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(errorProcessorsCount++, v.Count);

			v.WithErrorProcessorOf(async (Exception _) => await Task.Delay(1), CancellationType.Precancelable);
			Assert.AreEqual(errorProcessorsCount, v.Count);
		}

		public enum TestType
		{
			PolicyProc,
			BulkErrorProc
		}

		public class BulkErrorProcessorErrorProcessorRegistration : IErrorProcessorRegistration
		{
			private readonly BulkErrorProcessor _processor = new BulkErrorProcessor(PolicyAlias.Simple);
			public int Count => _processor.Count();

			public void WithErrorProcessor(IErrorProcessor errorProcessor) => _processor.WithErrorProcessor(new DefaultErrorProcessor());

			public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
			}
		}

		public class PolicyProcessorErrorProcessorRegistration : IErrorProcessorRegistration
		{
			private readonly SimplePolicyProcessor _processor = new SimplePolicyProcessor();
			public int Count => _processor.Count();

			public void WithErrorProcessor(IErrorProcessor errorProcessor) => _processor.WithErrorProcessor(errorProcessor);

			public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(actionProcessor);
			}

			public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
			}

			public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
			{
				_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
			}
		}

		public interface IErrorProcessorRegistration
		{
			void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor);
			void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor);
			void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor);
			void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Action<Exception> actionProcessor);
			void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor);
			void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor);
			void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, Task> funcProcessor);
			void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor);
			void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType);
			void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType);
			void WithErrorProcessor(IErrorProcessor errorProcessor);
			int Count { get; }
		}
	}
}
