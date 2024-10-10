using System;

namespace PoliNorError
{
	internal static class RetryDelayConstants
	{
		// Upper-bound to prevent overflow beyond TimeSpan.MaxValue. Potential truncation during conversion from double to long
		// (as described at https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions)
		// is avoided by the arbitrary subtraction of 1,000.
		internal static readonly double MaxTimeSpanTicks = (double)TimeSpan.MaxValue.Ticks - 1_000;

		internal static readonly TimeSpan MaxTimeSpanFromTicks = TimeSpan.FromTicks((long)MaxTimeSpanTicks);

		internal static readonly double MaxTimeSpanMs = (TimeSpan.MaxValue - TimeSpan.FromMilliseconds(2)).TotalMilliseconds;

		internal const double JitterFactor = 0.5;

		internal const double ExponentialFactor = 2.0;

		internal const double SlopeFactor = 1.0;
	}
}
