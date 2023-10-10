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
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) =>((IPolicyProcessor)pr).AddErrorProcessor(erPr);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);
	}
}
