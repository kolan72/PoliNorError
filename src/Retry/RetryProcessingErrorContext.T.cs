namespace PoliNorError
{
	internal class RetryProcessingErrorContext<TParam> : ProcessingErrorContext<TParam>
	{
		public RetryProcessingErrorContext(int retryCount, TParam param) : base(PolicyAlias.Retry, param)
		{
			Param = param;
			RetryCount = retryCount;
		}

		public int RetryCount { get; set; }

		internal override ProcessingErrorInfo ToProcessingErrorInfo() => new RetryProcessingErrorInfo<TParam>(this);
	}
}
