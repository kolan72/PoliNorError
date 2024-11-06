using System;

namespace PoliNorError
{
	public class ProcessingErrorContext
	{
		public ProcessingErrorContext(PolicyAlias policyKind) => PolicyKind = policyKind;

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("CurrentRetryCount is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
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
