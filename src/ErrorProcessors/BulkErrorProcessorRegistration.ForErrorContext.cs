using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class BulkErrorProcessorRegistration
	{
		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessorOf<TErrorContext>(this BulkErrorProcessor policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor)
		{
			return policyProcessor.WithErrorContextProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithErrorContextProcessor<TErrorContext>(this BulkErrorProcessor policyProcessor, DefaultErrorProcessor<TErrorContext> errorProcessor)
		{
			return policyProcessor.WithErrorContextProcessor(errorProcessor, _addErrorProcessorAction);
		}
	}
}
