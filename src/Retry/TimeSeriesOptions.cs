using System;

namespace PoliNorError
{
	/// <summary>
	/// Represents options for the time series retry delay type.
	/// </summary>
	public class TimeSeriesOptions : RetryDelayOptions
    {
        /// <summary>
        /// Gets or sets the sequence of time intervals to use between attempts.
        /// </summary>
        public TimeSpan[] Times { get; set; }

        /// <inheritdoc/>
        public override RetryDelayType DelayType => RetryDelayType.TimeSeries;
    }
}
