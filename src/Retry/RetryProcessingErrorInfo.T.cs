namespace PoliNorError
{
	public class RetryProcessingErrorInfo<TParam> : RetryProcessingErrorInfo
	{
		internal RetryProcessingErrorInfo(RetryProcessingErrorContext<TParam> currentContext) : base(currentContext.RetryCount)
		{
			Param = currentContext.Param;
		}

		public TParam Param { get; private set; }
	}
}
