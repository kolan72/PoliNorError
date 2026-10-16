using System;
using System.Runtime.CompilerServices;

namespace PoliNorError
{
	internal static class MaxDelayHelper
	{
		/// <summary>
		/// Returns the effective maximum delay in milliseconds, clamped to avoid overflow
		/// when converting from <see cref="TimeSpan"/> to <see langword="double"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static double GetAdaptedMaxDelayMs(TimeSpan maxDelay)
		{
			return maxDelay.TotalMilliseconds > RetryDelayConstants.MaxTimeSpanMs
				? RetryDelayConstants.MaxTimeSpanMs
				: maxDelay.TotalMilliseconds;
		}

		/// <summary>
		/// Returns <paramref name="maxDelay"/> when <paramref name="ms"/> is at or above
		/// <paramref name="adaptedMaxDelayMs"/>; otherwise converts <paramref name="ms"/>
		/// to a <see cref="TimeSpan"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static TimeSpan LimitToMaxDelay(double ms, double adaptedMaxDelayMs, TimeSpan maxDelay)
		{
			return (ms >= adaptedMaxDelayMs) ? maxDelay : TimeSpan.FromMilliseconds(ms);
		}
	}
}
