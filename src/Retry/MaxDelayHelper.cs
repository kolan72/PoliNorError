using System;

namespace PoliNorError
{
	internal static class MaxDelayHelper
	{
		/// <summary>
		/// Returns <paramref name="maxDelay"/> when <paramref name="ms"/> is at or above
		/// <paramref name="adaptedMaxDelayMs"/>; otherwise converts <paramref name="ms"/>
		/// to a <see cref="TimeSpan"/>.
		/// </summary>
		internal static TimeSpan LimitToMaxDelay(double ms, double adaptedMaxDelayMs, TimeSpan maxDelay)
		{
			return (ms >= adaptedMaxDelayMs) ? maxDelay : TimeSpan.FromMilliseconds(ms);
		}
	}
}
