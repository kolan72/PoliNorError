using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class EnumerablePolicyDelegateErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) => ((IEnumerable<PolicyDelegateBase>)pr).Select(pd => pd.Policy).LastOrDefault()?.PolicyProcessor.AddErrorProcessor(erPr);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
				=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor
						=> policyProcessor.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
