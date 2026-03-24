namespace PoliNorError
{
	internal static class StandardJitter
	{
		internal static double AddJitter(double delayInMs)
		{
			var offset = (delayInMs * RetryDelayConstants.JitterFactor) / 2;
			var randomDelay = (delayInMs * RetryDelayConstants.JitterFactor * StaticRandom.RandDouble()) - offset;
			return delayInMs + randomDelay;
		}
	}
}
