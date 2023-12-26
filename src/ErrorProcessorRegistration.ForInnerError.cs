using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class ErrorProcessorRegistration
	{
		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException> actionProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, CancellationToken> actionProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}
	}
}
