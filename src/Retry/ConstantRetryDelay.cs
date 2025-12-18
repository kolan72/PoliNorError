using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public sealed class ConstantRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions) : base(GetDelayValueProvider(retryDelayOptions))
		{
			if (retryDelayOptions.UseJitter && retryDelayOptions.MaxDelay < retryDelayOptions.BaseDelay)
			{
				throw new ArgumentOutOfRangeException(nameof(retryDelayOptions), "MaxDelay must be greater than or equal to BaseDelay.");
			}
#pragma warning disable CS0618 // Type or member is obsolete
			InnerDelay = this;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private static Func<int, TimeSpan> GetDelayValueProvider(ConstantRetryDelayOptions retryDelayOptions)
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

		/// <summary>
		/// Creates <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="baseDelay">Base delay value between retries.</param>
		/// <param name="maxDelay">Maximum delay value. Only used if <paramref name="useJitter"/> is true. If null, it will be set to <see cref="TimeSpan.MaxValue"/>.</param>
		/// <param name="useJitter">Whether jitter is used.</param>
		/// <returns></returns>
		public static ConstantRetryDelay Create(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) => new ConstantRetryDelay(baseDelay, maxDelay, useJitter);

		internal ConstantRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay, UseJitter = useJitter, MaxDelay = maxDelay ?? TimeSpan.MaxValue }){}

		private static Func<int, TimeSpan> GetDelayValue(ConstantRetryDelayOptions options) => (_) => options.BaseDelay;

		private static Func<int, TimeSpan> GetJitteredDelayValue(ConstantRetryDelayOptions options)
		{
			return (_) =>
			{
				var maxDelayDelimiter = new MaxDelayDelimiter(options);
				return maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(ApplyJitter(GetDelayValueInMs(options)));
			};
		}

		private static double GetDelayValueInMs(ConstantRetryDelayOptions options)
		{
			return options.BaseDelay.TotalMilliseconds;
		}
	}

	/// <summary>
	/// Represents options for the <see cref="ConstantRetryDelay"/>.
	/// </summary>
	public class ConstantRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Constant;
	}
}
