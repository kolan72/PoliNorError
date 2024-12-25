namespace PoliNorError
{
	public class ProcessingErrorInfo<TParam> : ProcessingErrorInfo
	{
		internal ProcessingErrorInfo(ProcessingErrorContext<TParam> currentContext) : this(currentContext.PolicyKind, currentContext) { }

		public ProcessingErrorInfo(PolicyAlias policyKind, ProcessingErrorContext<TParam> currentContext = null) : base(policyKind, currentContext)
		{
			if (currentContext != null)
			{
				Param = currentContext.Param;
			}
		}
		public TParam Param { get; private set; }
	}
}
