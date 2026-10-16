using System.Runtime.CompilerServices;

namespace PoliNorError
{
	internal static class StandardJitter
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static double AddJitter(double delayInMs)
		{
			var offset = (delayInMs * RetryDelayConstants.JitterFactor) / 2;
			var randomDelay = (delayInMs * RetryDelayConstants.JitterFactor * StaticRandom.RandDouble()) - offset;
			return delayInMs + randomDelay;
		}
	}
}
