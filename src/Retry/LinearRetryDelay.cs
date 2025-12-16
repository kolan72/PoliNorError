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
		public LinearRetryDelay(LinearRetryDelayOptions retryDelayOptions): base(GetDelayValueProvider(retryDelayOptions))
		{
#pragma warning disable CS0618 // Type or member is obsolete
			InnerDelay = this;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		private static Func<int, TimeSpan> GetDelayValueProvider(LinearRetryDelayOptions retryDelayOptions)
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

		private static Func<int, TimeSpan> GetDelayValue(LinearRetryDelayOptions options)
		{
			return (attempt) =>
			{
				var maxDelayDelimiter = new MaxDelayDelimiter(options);
				return maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(GetDelayValueInMs(attempt, options));
			};
		}

		private static Func<int, TimeSpan> GetJitteredDelayValue(LinearRetryDelayOptions options)
		{
			return (attempt) =>
			{
				var maxDelayDelimiter = new MaxDelayDelimiter(options);
				return maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(ApplyJitter(GetDelayValueInMs(attempt, options)));
			};
		}

		private static double GetDelayValueInMs(int attempt, LinearRetryDelayOptions options)
		{
			return (attempt + 1) * options.SlopeFactor * options.BaseDelay.TotalMilliseconds;
		}
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
	}
}
