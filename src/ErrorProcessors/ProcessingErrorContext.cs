using System;

namespace PoliNorError
{
	public class ProcessingErrorContext
	{
		internal ProcessingErrorContext(): this(PolicyAlias.NotSet) { }

		public ProcessingErrorContext(PolicyAlias policyKind) => PolicyKind = policyKind;

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("CurrentRetryCount is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
		public int CurrentRetryCount { get; } = -1;

		internal bool IsPolicyAliasSet => PolicyKind != PolicyAlias.NotSet;

		internal PolicyAlias PolicyKind { get; set; }

		internal virtual ProcessingErrorInfo ToProcessingErrorInfo()
		{
			return new ProcessingErrorInfo(this);
		}
	}
}
