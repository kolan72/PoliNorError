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
		private readonly TimeSpan[] _times;

		private readonly TimeSpan _lastElement;
		private readonly MaxDelayDelimiter _maxDelayDelimiter;

		/// <summary>
		/// Initializes a new instance of <see cref="TimeSeriesRetryDelay"/>.
		/// </summary>
		/// <param name="timeSeriesOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public TimeSeriesRetryDelay(TimeSeriesRetryDelayOptions timeSeriesOptions)
		{
			_times = timeSeriesOptions.Times;

			if (_times?.Length == 0)
			{
				_times = new[] { timeSeriesOptions.BaseDelay > timeSeriesOptions.MaxDelay ? timeSeriesOptions.MaxDelay : timeSeriesOptions.BaseDelay };
			}

			_lastElement = _times.LastOrDefault();

			if (timeSeriesOptions.UseJitter)
			{
				DelayValueProvider = GetJitteredDelayValue;
			}
			else
			{
				DelayValueProvider = GetDelayValue;
			}
			_maxDelayDelimiter = new MaxDelayDelimiter(timeSeriesOptions);
		}

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

		private TimeSpan GetDelayValue(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(GetDelayInner(attempt).TotalMilliseconds);
		}

		private TimeSpan GetJitteredDelayValue(int attempt)
		{
			return _maxDelayDelimiter.GetDelayLimitedToMaxDelayIfNeed(ApplyJitter(GetDelayInner(attempt).TotalMilliseconds));
		}

		private TimeSpan GetDelayInner(int attempt)
		{
			if (attempt > _times.Length - 1)
			{
				return _lastElement;
			}
			return _times[attempt];
		}
	}
}
