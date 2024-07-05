using System;

namespace PoliNorError
{
	public class RetryDelay
	{
        private readonly RetryDelay _innerDelay;

		protected RetryDelay(){}

        public RetryDelay(RetryDelayOptions retryDelayOptions)
		{
            switch (retryDelayOptions.DelayType)
            {
                case RetryDelayType.Constant:
                    _innerDelay = new ConstantRetryDelay((ConstantRetryDelayOptions)retryDelayOptions);
                    break;
                case RetryDelayType.Linear:
                    _innerDelay = new LinearRetryDelay((LinearRetryDelayOptions)retryDelayOptions);
                    break;
                case RetryDelayType.Exponential:
                    _innerDelay = new ExponentialRetryDelay((ExponentialRetryDelayOptions)retryDelayOptions);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public virtual TimeSpan GetDelay(int attempt)
        {
            return _innerDelay.GetDelay(attempt);
        }
	}

    public abstract class RetryDelayOptions
    {
        public abstract RetryDelayType DelayType { get;}
        public TimeSpan BaseDelay { get; set; }
    }

    public enum RetryDelayType
    {
        /// <summary>
        /// The constant delay type.
        /// </summary>
        /// <remarks>
        /// Constant delay for each attempt.
        /// </remarks>
        Constant,

        /// <summary>
        /// The linear delay type.
        /// </summary>
        /// <remarks>
        /// Generates delays in an linear manner.
        /// </remarks>
        Linear,

        /// <summary>
        /// The exponential backoff type.
        /// /// </summary>
        Exponential
    }
}
