namespace PoliNorError
{
	/// <summary>
	/// Provides typed information about an error during policy processing.
	/// </summary>
	/// <typeparam name="TParam">The type of the parameter.</typeparam>
	public class ProcessingErrorInfo<TParam> : ProcessingErrorInfo
	{
		internal ProcessingErrorInfo(ProcessingErrorContext<TParam> currentContext) : this(currentContext.PolicyKind, currentContext) { }

		/// <summary>
		/// Initializes a new instance with the specified policy kind and context.
		/// </summary>
		/// <param name="policyKind">The policy kind.</param>
		/// <param name="currentContext">The error processing context.</param>
		public ProcessingErrorInfo(PolicyAlias policyKind, ProcessingErrorContext<TParam> currentContext = null) : base(policyKind, currentContext)
		{
			if (currentContext != null)
			{
				Param = currentContext.Param;
			}
		}

		/// <summary>
		/// Gets the parameter.
		/// </summary>
		public TParam Param { get; private set; }
	}
}
