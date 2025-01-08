using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class PolicyErrorProcessorRegistration
	{
		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(actionProcessor);
			return policy;
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(actionProcessor, cancellationType);
			return policy;
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(actionProcessor);
			return policy;
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(funcProcessor);
			return policy;
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(funcProcessor, cancellationType);
			return policy;
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policy, Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessorOf(funcProcessor);
			return policy;
		}

		internal static T WithErrorContextProcessor<T, TErrorContext>(this T policy, DefaultErrorProcessor<TErrorContext> errorProcessor) where T : IPolicyBase
		{
			policy.PolicyProcessor.WithErrorContextProcessor(errorProcessor);
			return policy;
		}
	}
}
