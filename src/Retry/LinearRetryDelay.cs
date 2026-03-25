using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the delay value calculated linearly.
	/// </summary>
	public sealed class LinearRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="LinearRetryDelayOptions"/></param>
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions): base(new LinearDelayCore(retryDelayOptions).GetDelay)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			InnerDelay = this;
#pragma warning restore CS0618 // Type or member is obsolete
		}
		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static LinearRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new LinearRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);

		/// <summary>
		///  Creates <see cref="LinearRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="slopeFactor">Slope factor to use.</param>
		/// <param name="maxDelay">Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static LinearRetryDelay Create(TimeSpan baseDelay, double slopeFactor, TimeSpan? maxDelay = null, bool useJitter = false) => new LinearRetryDelay(baseDelay, slopeFactor, maxDelay, useJitter);

		internal LinearRetryDelay(TimeSpan baseDelay, double slopeFactor = RetryDelayConstants.SlopeFactor, TimeSpan? maxDelay = null, bool useJitter = false) :
			 this(new LinearRetryDelayOptions() { BaseDelay = baseDelay, SlopeFactor = slopeFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue } ) {}
	}

	/// <summary>
	/// Represents options for the <see cref="LinearRetryDelay"/>.
	/// </summary>
	public class LinearRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Linear;

		/// <summary>
		/// Slope factor to use.
		/// </summary>
		public double SlopeFactor { get; set; } = RetryDelayConstants.SlopeFactor;

		public static implicit operator LinearRetryDelay(LinearRetryDelayOptions options) => new LinearRetryDelay(options);
	}

	internal class LinearDelayCore
	{
		private readonly LinearRetryDelayOptions _delayOptions;
		private readonly MaxDelayDelimiter _maxDelayDelimiter;

		private readonly Func<int, TimeSpan> _getDelay;

		public LinearDelayCore(LinearRetryDelayOptions delayOptions)
		{
			_delayOptions = delayOptions;
			_maxDelayDelimiter = new MaxDelayDelimiter(delayOptions);

			if (delayOptions.UseJitter)
			{
				_getDelay = GetJitteredDelay;
			}
			else
			{
				_getDelay = GetBaseDelay;
			}
		}

		public TimeSpan GetDelay(int attempt) => _getDelay(attempt);

		private TimeSpan GetBaseDelay(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(GetDelayValueInMs(attempt, _delayOptions));
		}

		private TimeSpan GetJitteredDelay(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(StandardJitter.AddJitter(GetDelayValueInMs(attempt, _delayOptions)));
		}

		private static double GetDelayValueInMs(int attempt, LinearRetryDelayOptions options)
		{
			return (attempt + 1) * options.SlopeFactor * options.BaseDelay.TotalMilliseconds;
		}
	}
}
