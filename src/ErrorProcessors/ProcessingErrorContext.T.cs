namespace PoliNorError
{
	/// <summary>
	/// Provides typed context information for error processing.
	/// </summary>
	/// <typeparam name="TParam">The type of the parameter.</typeparam>
	public class ProcessingErrorContext<TParam> : ProcessingErrorContext
	{
		/// <summary>
		/// Initializes a new instance with the specified policy kind and parameter.
		/// </summary>
		/// <param name="policyKind">The policy kind.</param>
		/// <param name="param">The parameter.</param>
		public ProcessingErrorContext(PolicyAlias policyKind, TParam param) : base(policyKind)
		{
			Param = param;
		}

		/// <summary>
		/// Gets or sets the parameter.
		/// </summary>
		public TParam Param { get; set; }

		/// <summary>
		/// Converts the context to a typed <see cref="ProcessingErrorInfo"/>.
		/// </summary>
		internal override ProcessingErrorInfo ToProcessingErrorInfo()
		{
			return new ProcessingErrorInfo<TParam>(this);
		}
	}
}
