namespace PoliNorError
{
	public class RetryProcessingErrorInfo<TParam> : ProcessingErrorInfo<TParam>
	{
		internal RetryProcessingErrorInfo(RetryProcessingErrorContext<TParam> currentContext) : base(PolicyAlias.Retry, currentContext)
		{
		}

		public int RetryCount => ((RetryProcessingErrorContext<TParam>)CurrentContext).RetryCount;
	}
}
