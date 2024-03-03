namespace PoliNorError
{
	public class ProcessingErrorContext
	{
		internal ProcessingErrorContext() : this(PolicyAlias.NotSet) { }

		public ProcessingErrorContext(PolicyAlias policyKind) => PolicyKind = policyKind;

		public int CurrentRetryCount { get; private set; } = -1;

		internal bool IsPolicyAliasSet => PolicyKind != PolicyAlias.NotSet;

		internal PolicyAlias PolicyKind { get; set; }

		public static ProcessingErrorContext FromRetry(int currentRetryCount)
		{
			return new ProcessingErrorContext() { CurrentRetryCount = currentRetryCount };
		}

		internal virtual ProcessingErrorInfo ToProcessingErrorInfo(PolicyAlias policyAlias)
		{
			if (policyAlias != PolicyAlias.NotSet)
				return new ProcessingErrorInfo(policyAlias, this);
			else
				return new ProcessingErrorInfo(this);
		}
	}
}
