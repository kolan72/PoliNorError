using System;

namespace PoliNorError
{
	/// <summary>
	/// The base class to get the delay value before the next attempt.
	/// </summary>
	public class RetryDelay
	{
		/// <summary>
		/// Gets or sets the delay value provider.
		/// Replaces <see cref="InnerDelay"/> and <see cref="InnerDelayValueProvider"/>, which are now obsolete.
		/// </summary>
		protected Func<int, TimeSpan> DelayValueProvider { get; set; }

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This property is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected RetryDelay InnerDelay { get; set; }

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This property is obsolete. Use DelayValueProvider property instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected Func<int, TimeSpan> InnerDelayValueProvider { get; set; }

		protected RetryDelay()
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="RetryDelay"/>.
		/// </summary>
		/// <param name="delayType">The type of delay.</param>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay between retries.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		public RetryDelay(RetryDelayType delayType, TimeSpan baseDelay, TimeSpan maxDelay, bool useJitter = false)
		{
			DelayValueProvider = GetRetryDelayProvider(delayType, baseDelay, maxDelay, useJitter);
		}

		///<inheritdoc cref = "RetryDelay(RetryDelayType, TimeSpan, TimeSpan, Boolean)"/>
		public RetryDelay(RetryDelayType delayType, TimeSpan baseDelay, bool useJitter = false)
		{
			DelayValueProvider = GetRetryDelayProvider(delayType, baseDelay, null, useJitter);
		}

		/// <summary>
		/// Initializes a new instance of <see cref="RetryDelay"/>.
		/// </summary>
		/// <param name="options"><see cref="RetryDelayOptions"/></param>
		public RetryDelay(RetryDelayOptions options)
		{
			switch (options)
			{
				case ConstantRetryDelayOptions c:
					DelayValueProvider = (new ConstantRetryDelay(c)).DelayValueProvider;
					break;
				case LinearRetryDelayOptions l:
					DelayValueProvider = (new LinearRetryDelay(l)).DelayValueProvider;
					break;
				case ExponentialRetryDelayOptions e:
					DelayValueProvider = (new ExponentialRetryDelay(e)).DelayValueProvider;
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
			return DelayValueProvider(attempt);
		}

		protected static double ApplyJitter(double delayInMs)
		{
			var offset = (delayInMs * RetryDelayConstants.JitterFactor) / 2;
			var randomDelay = (delayInMs * RetryDelayConstants.JitterFactor * StaticRandom.RandDouble()) - offset;
			return delayInMs + randomDelay;
		}

		private Func<int, TimeSpan> GetRetryDelayProvider(RetryDelayType delayType, TimeSpan baseDelay, TimeSpan? maxDelay, bool useJitter)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			switch (delayType)
			{
				case RetryDelayType.Constant:
					InnerDelay = new ConstantRetryDelay(baseDelay, maxDelay, useJitter);
					break;
				case RetryDelayType.Linear:
					InnerDelay = new LinearRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);
					break;
				case RetryDelayType.Exponential:
					InnerDelay = new ExponentialRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);
					break;
				default:
					throw new NotImplementedException();
			}
			return InnerDelay.DelayValueProvider;
#pragma warning restore CS0618 // Type or member is obsolete
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

		/// <summary>
		/// Indicates whether jitter is used. The default value is <see langword="false"/>.
		/// </summary>
		public bool UseJitter { get; set; }

		/// <summary>
		/// Maximum delay between retries.
		/// </summary>
		public TimeSpan MaxDelay { get; set; } = TimeSpan.MaxValue;

		internal double GetAdaptedMaxDelayMs()
		{
			return MaxDelay.TotalMilliseconds > RetryDelayConstants.MaxTimeSpanMs
				? RetryDelayConstants.MaxTimeSpanMs
				: MaxDelay.TotalMilliseconds;
		}
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

	internal class MaxDelayDelimiter
	{
		private readonly double _adaptedMaxDelayMs;

		private readonly TimeSpan _maxDelay;

		public MaxDelayDelimiter(RetryDelayOptions options)
		{
			_adaptedMaxDelayMs = options.GetAdaptedMaxDelayMs();
			_maxDelay = options.MaxDelay;
		}

		public TimeSpan GetDelayLimitedToMaxDelayIfNeed(double ms)
		{
			return (ms >= _adaptedMaxDelayMs) ? _maxDelay : TimeSpan.FromMilliseconds(ms);
		}
	}
}
