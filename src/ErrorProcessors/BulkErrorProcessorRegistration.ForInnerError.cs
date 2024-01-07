using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class BulkErrorProcessorRegistration
	{
		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);
		}

		public static BulkErrorProcessor WithInnerErrorProcessorOf<TException>(this BulkErrorProcessor policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return policyProcessor.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
		}
	}
}
