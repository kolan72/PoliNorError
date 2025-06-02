using System;
using System.Threading;

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
	}
}
