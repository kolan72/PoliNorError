namespace PoliNorError
{
	internal class RetryProcessingErrorContext<TParam> : RetryProcessingErrorContext
	{
		public RetryProcessingErrorContext(int retryCount, TParam param) : base(retryCount)
		{
			Param = param;
		}

		public TParam Param { get; set; }

		internal override ProcessingErrorInfo ToProcessingErrorInfo() => new RetryProcessingErrorInfo<TParam>(this);
	}
}
