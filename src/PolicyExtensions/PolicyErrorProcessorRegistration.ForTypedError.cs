using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyErrorProcessorRegistration
	{
		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Action<TException, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(actionProcessor);
			return policy;
		}

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(actionProcessor);
			return policy;
		}

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(actionProcessor, cancellationType);
			return policy;
		}

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(funcProcessor);
			return policy;
		}

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(funcProcessor, cancellationType);
			return policy;
		}

		internal static T WithTypedErrorProcessorOf<T, TException>(this T policy, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessorOf(funcProcessor);
			return policy;
		}

		internal static T WithTypedErrorProcessor<T, TException>(this T policy, DefaultTypedErrorProcessor<TException> errorProcessor) where T : IPolicyBase where TException : Exception
		{
			policy.PolicyProcessor.WithTypedErrorProcessor(errorProcessor);
			return policy;
		}
	}
}
