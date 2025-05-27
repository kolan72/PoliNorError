using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

			if (_times.Length == 0)
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

		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan fourthTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime, fourthTime }, maxDelay, useJitter);

		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan thirdTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime, thirdTime }, maxDelay, useJitter);

		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan secondTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime, secondTime }, maxDelay, useJitter);

		public static TimeSeriesRetryDelay Create(TimeSpan firstTime, TimeSpan? maxDelay = null, bool useJitter = false) =>
				Create(new TimeSpan[] { firstTime }, maxDelay, useJitter);

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
