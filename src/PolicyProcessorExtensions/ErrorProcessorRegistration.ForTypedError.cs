using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class ErrorProcessorRegistration
	{
		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		internal static T WithTypedErrorProcessor<T, TException>(this T policyProcessor, DefaultTypedErrorProcessor<TException> errorProcessor) where T : IPolicyProcessor where TException : Exception
			=> policyProcessor.WithTypedErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
