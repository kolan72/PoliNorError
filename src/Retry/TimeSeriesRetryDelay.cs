using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	/// <summary>
	/// Represents a sealed class for handling time series–based retry delay values.
	/// </summary>
	public sealed class TimeSeriesRetryDelay : RetryDelay
	{
		/// <summary>
		/// Initializes a new instance of <see cref="TimeSeriesRetryDelay"/>.
		/// </summary>
		/// <param name="timeSeriesOptions"><see cref="TimeSeriesRetryDelayOptions"/></param>
		public TimeSeriesRetryDelay(TimeSeriesRetryDelayOptions timeSeriesOptions) : base(new TimeSeriesDelayCore(timeSeriesOptions)) {}

		/// <summary>
		/// Initializes a new instance of the <see cref="TimeSeriesRetryDelay"/> class with the specified base delay, optional maximum delay, and jitter setting.
		/// </summary>
		/// <param name="baseDelay">The base delay time span.</param>
		/// <param name="maxDelay">The optional maximum delay time span. If null, <see cref="TimeSpan.MaxValue"/> will be used.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		internal TimeSeriesRetryDelay(TimeSpan baseDelay, TimeSpan? maxDelay = null, bool useJitter = false)
		: this(new TimeSeriesRetryDelayOptions() { BaseDelay = baseDelay, MaxDelay = maxDelay ?? TimeSpan.MaxValue, UseJitter = useJitter, Times = new[] { baseDelay } })
		{
		}

		/// <summary>
		/// Creates a new <see cref="TimeSeriesRetryDelay"/> instance with four specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="thirdTime">The third delay time.</param>
		/// <param name="fourthTime">The fourth delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="TimeSeriesRetryDelay"/> instance.</returns>
		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan fourthTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime, fourthTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="TimeSeriesRetryDelay"/> instance with three specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="thirdTime">The third delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="TimeSeriesRetryDelay"/> instance.</returns>
		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="TimeSeriesRetryDelay"/> instance with two specified delay times.
		/// </summary>
		/// <param name="firstTime">The first delay time.</param>
		/// <param name="secondTime">The second delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="TimeSeriesRetryDelay"/> instance.</returns>
		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="TimeSeriesRetryDelay"/> instance with a single specified delay time.
		/// </summary>
		/// <param name="firstTime">The delay time.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="TimeSeriesRetryDelay"/> instance.</returns>
		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime }, maxDelay, useJitter);

		/// <summary>
		/// Creates a new <see cref="TimeSeriesRetryDelay"/> instance with a collection of specified delay times.
		/// </summary>
		/// <param name="times">The collection of delay times.</param>
		/// <param name="maxDelay">The optional maximum delay time span.</param>
		/// <param name="useJitter">Whether to apply jitter to the delay values.</param>
		/// <returns>A new <see cref="TimeSeriesRetryDelay"/> instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="times"/> is null.</exception>
		public static TimeSeriesRetryDelay Create(IEnumerable<TimeSpan> times, TimeSpan? maxDelay = null, bool useJitter = false)
		{
			if (times is null)
			{
				throw new ArgumentNullException(nameof(times));
			}
			return new TimeSeriesRetryDelay(new TimeSeriesRetryDelayOptions()
			{
				BaseDelay = TimeSpan.Zero,
				MaxDelay = maxDelay ?? TimeSpan.MaxValue,
				UseJitter = useJitter,
				Times = times.ToArray()
			});
		}
	}

	internal class TimeSeriesDelayCore : DelayCoreBase
	{
		private readonly MaxDelayDelimiter _maxDelayDelimiter;
		private readonly TimeSpan[] _times;
		private readonly int _maxIndex;

		public TimeSeriesDelayCore(TimeSeriesRetryDelayOptions delayOptions) : base(delayOptions)
		{
			_maxDelayDelimiter = new MaxDelayDelimiter(delayOptions);
			if (delayOptions.Times?.Length == 0)
			{
				_times = new[] { delayOptions.BaseDelay > delayOptions.MaxDelay ? delayOptions.MaxDelay : delayOptions.BaseDelay };
			}
			else
			{
				_times = delayOptions.Times;
			}
			_maxIndex = _times.Length - 1;
		}

		protected override TimeSpan GetBaseDelay(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(GetDelayInner(attempt).TotalMilliseconds);
		}

		protected override TimeSpan GetJitteredDelay(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(StandardJitter.AddJitter(GetDelayInner(attempt).TotalMilliseconds));
		}

		private TimeSpan GetDelayInner(int attempt)
		{
			return _times[(uint)attempt <= (uint)_maxIndex ? attempt : _maxIndex];
		}
	}
}
