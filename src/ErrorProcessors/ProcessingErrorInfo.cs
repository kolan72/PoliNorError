using System;

namespace PoliNorError
{
	public class ProcessingErrorInfo
	{
		protected ProcessingErrorInfo(){}

		internal ProcessingErrorInfo(ProcessingErrorContext currentContext) : this(currentContext.PolicyKind, currentContext){}

		public ProcessingErrorInfo(PolicyAlias policyKind,  ProcessingErrorContext currentContext = null)
		{
			PolicyKind = policyKind;

			CurrentContext = currentContext;

			HasContext = CurrentContext != null;
		}

		public ProcessingErrorContext CurrentContext { get; }

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("CurrentRetryCount is obsolete. Use RetryProcessingErrorInfo.RetryCount instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
		public int CurrentRetryCount { get; protected set; } = -1;

		public static ProcessingErrorInfo FromRetry(int retryAttempt)
		{
			return new RetryProcessingErrorInfo(retryAttempt);
		}

		public PolicyAlias PolicyKind { get; protected set; }

		public bool HasContext { get; protected set; }
	}

	public static class ProcessingErrorInfoExtensions
	{
		public static int GetRetryCount(this ProcessingErrorInfo processingErrorInfo)
		{
			return (processingErrorInfo as IRetryExecutionInfo)?.RetryCount ?? 0;
		}

		public static int GetAttemptCount(this ProcessingErrorInfo processingErrorInfo) => processingErrorInfo.GetRetryCount() + 1;
	}
}
