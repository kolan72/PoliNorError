using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyErrorProcessorRegistration
	{
		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException> actionProcessor) where T : IPolicyBase where TException : Exception
		{
		 	errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException, CancellationToken> actionProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException> actionProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, CancellationToken, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType);
			return errorPolicyBase;
		}

		internal static T WithInnerErrorProcessorOf<T, TException>(this T errorPolicyBase, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			errorPolicyBase.PolicyProcessor.WithInnerErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}
	}
}
