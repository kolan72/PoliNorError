using System;

namespace PoliNorError
{
	public class ConstantRetryDelay : RetryDelay
    {
        private readonly TimeSpan _baseDelay;

        public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions) : this(retryDelayOptions.BaseDelay){}

        internal ConstantRetryDelay(TimeSpan baseDelay)
		{
            _baseDelay = baseDelay;
        }

        public override TimeSpan GetDelay(int attempt)
        {
            return _baseDelay;
        }
    }

    public class ConstantRetryDelayOptions : RetryDelayOptions
    {
        public override RetryDelayType DelayType => RetryDelayType.Constant;
    }
}
