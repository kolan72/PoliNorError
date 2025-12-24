using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class ErrorProcessorRegistration
	{
		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		internal static T WithErrorContextProcessor<T, TErrorContext>(this T policyProcessor, DefaultErrorProcessor<TErrorContext> errorProcessor) where T : IPolicyProcessor
		{
			return policyProcessor.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
		}
	}
}
