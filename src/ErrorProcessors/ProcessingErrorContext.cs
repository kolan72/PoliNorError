using System;

namespace PoliNorError
{
	/// <summary>
	/// Provides context information for error processing.
	/// </summary>
	public class ProcessingErrorContext
	{
		internal ProcessingErrorContext(): this(PolicyAlias.NotSet) { }

		/// <summary>
		/// Initializes a new instance with the specified policy kind.
		/// </summary>
		public ProcessingErrorContext(PolicyAlias policyKind) => PolicyKind = policyKind;

		/// <summary>
		/// Gets the current retry count.
		/// </summary>
#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("CurrentRetryCount is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
		public int CurrentRetryCount { get; } = -1;

		/// <summary>
		/// Gets a value indicating whether the policy alias has been set.
		/// </summary>
		internal bool IsPolicyAliasSet => PolicyKind != PolicyAlias.NotSet;

		/// <summary>
		/// Gets or sets the policy alias.
		/// </summary>
		internal PolicyAlias PolicyKind { get; set; }

		/// <summary>
		/// Converts the context to a <see cref="ProcessingErrorInfo"/>.
		/// </summary>
		internal virtual ProcessingErrorInfo ToProcessingErrorInfo()
		{
			return new ProcessingErrorInfo(this);
		}
	}
}
