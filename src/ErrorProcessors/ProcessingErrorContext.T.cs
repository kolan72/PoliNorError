namespace PoliNorError
{
	public class ProcessingErrorContext<TParam> : ProcessingErrorContext
	{
		public ProcessingErrorContext(PolicyAlias policyKind, TParam param) : base(policyKind)
		{
			Param = param;
		}
		public TParam Param { get; set; }

		internal override ProcessingErrorInfo ToProcessingErrorInfo()
		{
			return new ProcessingErrorInfo<TParam>(this);
		}
	}
}
