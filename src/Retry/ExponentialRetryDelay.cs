using System;

namespace PoliNorError
{
	/// <summary>
	///  Class to get the delay value calculated exponentially.
	/// </summary>
	public sealed partial class ExponentialRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ExponentialRetryDelayOptions"/></param>
		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions) : base(GetDelayValueProvider(retryDelayOptions))
		{
#pragma warning disable CS0618 // Type or member is obsolete
			InnerDelay = this;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private static Func<int, TimeSpan> GetDelayValueProvider(ExponentialRetryDelayOptions retryDelayOptions)
		{
			if (retryDelayOptions.UseJitter)
			{
				return GetJitteredDelayValue(retryDelayOptions);
			}
			else
			{
				return GetDelayValue(retryDelayOptions);
			}
		}

		private static Func<int, TimeSpan> GetJitteredDelayValue(ExponentialRetryDelayOptions options)
		{
			return (attempt) =>
			{
				var dj = new DecorrelatedJitter(options.BaseDelay, options.ExponentialFactor, options.MaxDelay);
				return dj.DecorrelatedJitterBackoffV2(attempt);
			};
		}

		private static Func<int, TimeSpan> GetDelayValue(ExponentialRetryDelayOptions options)
		{
			return (attempt) =>
			{
				var maxDelayDelimiter = new MaxDelayDelimiter(options);
				return maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(Math.Pow(options.ExponentialFactor, attempt) * options.BaseDelay.TotalMilliseconds);
			};
		}

		/// <summary>
		/// Creates <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">>Maximum delay value. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ExponentialRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new ExponentialRetryDelay(baseDelay, maxDelay: maxDelay, useJitter: useJitter);

		/// <summary>
		/// Creates <see cref="ExponentialRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="exponentialFactor">Exponential factor to use.</param>
		/// <param name="maxDelay">>Maximum delay value.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ExponentialRetryDelay Create(TimeSpan baseDelay, double exponentialFactor, TimeSpan? maxDelay = null, bool useJitter = false) => new ExponentialRetryDelay(baseDelay, exponentialFactor, maxDelay, useJitter);

		internal ExponentialRetryDelay(TimeSpan baseDelay, double exponentialFactor = RetryDelayConstants.ExponentialFactor, TimeSpan? maxDelay = null, bool useJitter = false) :
			this(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay, ExponentialFactor = exponentialFactor, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue}) {}
	}

	/// <summary>
	///  Represents options for the <see cref="ExponentialRetryDelay"/>.
	/// </summary>
	public class ExponentialRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Exponential;

		/// <summary>
		/// Exponential factor to use.
		/// </summary>
		public double ExponentialFactor { get; set; } = RetryDelayConstants.ExponentialFactor;
	}
}
