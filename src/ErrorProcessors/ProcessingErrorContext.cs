namespace PoliNorError
{
	public class ProcessingErrorContext
	{
		internal ProcessingErrorContext() : this(PolicyAlias.NotSet) { }

		public ProcessingErrorContext(PolicyAlias policyKind) => PolicyKind = policyKind;

		public int CurrentRetryCount { get; } = -1;

		internal bool IsPolicyAliasSet => PolicyKind != PolicyAlias.NotSet;

		internal PolicyAlias PolicyKind { get; set; }

		internal virtual ProcessingErrorInfo ToProcessingErrorInfo(PolicyAlias policyAlias)
		{
			if (policyAlias != PolicyAlias.NotSet)
				return new ProcessingErrorInfo(policyAlias, this);
			else
				return new ProcessingErrorInfo(this);
		}
	}
}
