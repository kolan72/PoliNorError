using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of extension methods to add error processor to policy processor.
	/// </summary>
	public static class ErrorProcessorRegistration
	{
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor, cancellationType));

		public static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor) where T : IPolicyProcessor
		{
			if (errorProcessor == null)
				throw new ArgumentNullException(nameof(errorProcessor));

			policyProcessor.AddErrorProcessor(errorProcessor);
			return policyProcessor;
		}
	}
}
