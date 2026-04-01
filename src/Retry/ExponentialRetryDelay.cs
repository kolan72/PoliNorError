using System;
using static PoliNorError.ExponentialRetryDelay;

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
		public ExponentialRetryDelay(ExponentialRetryDelayOptions retryDelayOptions) : base(new ExponentialDelayCore(retryDelayOptions))
		{
#pragma warning disable CS0618 // Type or member is obsolete
			InnerDelay = this;
#pragma warning restore CS0618 // Type or member is obsolete
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

		public static implicit operator ExponentialRetryDelay(ExponentialRetryDelayOptions options) => new ExponentialRetryDelay(options);
	}

	internal class ExponentialDelayCore : DelayCoreBase
	{
		private readonly ExponentialRetryDelayOptions _delayOptions;

		private readonly DecorrelatedJitter _jitter;
		private readonly MaxDelayDelimiter _maxDelayDelimiter;

		public ExponentialDelayCore(ExponentialRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_delayOptions = delayOptions;

			if (delayOptions.UseJitter)
			{
				_jitter = new DecorrelatedJitter(delayOptions.BaseDelay, delayOptions.ExponentialFactor, delayOptions.MaxDelay);
			}
			else
			{
				_maxDelayDelimiter = new MaxDelayDelimiter(delayOptions);
			}
		}

		protected override TimeSpan GetBaseDelay(int attempt)
			=> _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(Math.Pow(_delayOptions.ExponentialFactor, attempt) * _delayOptions.BaseDelay.TotalMilliseconds);

		protected override TimeSpan GetJitteredDelay(int attempt)
			=> _jitter.DecorrelatedJitterBackoffV2(attempt);
	}
}
