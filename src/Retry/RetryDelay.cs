using System;

namespace PoliNorError
{
	/// <summary>
	/// The base class to get the delay value before the next attempt.
	/// </summary>
	public class RetryDelay
	{
		private readonly RetryDelay _innerDelay;

		protected RetryDelay(){}

		/// <summary>
		/// Initializes a new instance of <see cref="RetryDelay"/>.
		/// </summary>
		/// <param name="delayType">The type of delay.</param>
		/// <param name="baseDelay">Base delay value between retries.</param>
		public RetryDelay(RetryDelayType delayType, TimeSpan baseDelay)
		{
			switch (delayType)
			{
				case RetryDelayType.Constant:
					_innerDelay = new ConstantRetryDelay(baseDelay);
					break;
				case RetryDelayType.Linear:
					_innerDelay = new LinearRetryDelay(baseDelay);
					break;
				case RetryDelayType.Exponential:
					_innerDelay = new ExponentialRetryDelay(baseDelay);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets the delay value from the current attempt.
		/// </summary>
		/// <param name="attempt">The current attempt.</param>
		/// <returns></returns>
		public virtual TimeSpan GetDelay(int attempt)
		{
			return _innerDelay.GetDelay(attempt);
		}
	}

	/// <summary>
	/// Represents options for subclasses of <see cref="RetryDelay"/>.
	/// </summary>
	public abstract class RetryDelayOptions
	{
		/// <summary>
		/// The type of delay.
		/// </summary>
		public abstract RetryDelayType DelayType { get;}

		/// <summary>
		/// Base delay value between retries.
		/// </summary>
		public TimeSpan BaseDelay { get; set; }
	}

	/// <summary>
	/// Represents the type of delay.
	/// </summary>
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
