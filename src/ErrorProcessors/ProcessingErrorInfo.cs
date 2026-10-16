using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides information about an error during policy processing.
	/// </summary>
	public class ProcessingErrorInfo
	{
		protected ProcessingErrorInfo(){}

		internal ProcessingErrorInfo(ProcessingErrorContext currentContext) : this(currentContext.PolicyKind, currentContext){}

		/// <summary>
		/// Initializes a new instance with the specified policy kind and context.
		/// </summary>
		public ProcessingErrorInfo(PolicyAlias policyKind,  ProcessingErrorContext currentContext = null)
		{
			PolicyKind = policyKind;

			CurrentContext = currentContext;

			HasContext = CurrentContext != null;
		}

		/// <summary>
		/// Gets the current error processing context.
		/// </summary>
		public ProcessingErrorContext CurrentContext { get; }

		/// <summary>
		/// Gets the current retry count.
		/// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("CurrentRetryCount is obsolete. Use RetryProcessingErrorInfo.RetryCount instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
		public int CurrentRetryCount { get; protected set; } = -1;

		/// <summary>
		/// Creates a <see cref="ProcessingErrorInfo"/> for a retry policy.
		/// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This method is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
		public static ProcessingErrorInfo FromRetry(int retryAttempt)
		{
			return new RetryProcessingErrorInfo(retryAttempt);
		}

		/// <summary>
		/// Gets the policy alias.
		/// </summary>
		public PolicyAlias PolicyKind { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether a context is present.
		/// </summary>
		public bool HasContext { get; protected set; }
	}

	/// <summary>
	/// Provides extension methods for <see cref="ProcessingErrorInfo"/>.
	/// </summary>
	public static class ProcessingErrorInfoExtensions
	{
		/// <summary>
		/// Gets the retry count from the error info.
		/// </summary>
		public static int GetRetryCount(this ProcessingErrorInfo processingErrorInfo)
		{
			return (processingErrorInfo as IRetryExecutionInfo)?.RetryCount ?? 0;
		}

		/// <summary>
		/// Gets the attempt count (retry count + 1).
		/// </summary>
		public static int GetAttemptCount(this ProcessingErrorInfo processingErrorInfo) => processingErrorInfo.GetRetryCount() + 1;
	}
}
