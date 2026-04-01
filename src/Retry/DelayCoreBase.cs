using System;

namespace PoliNorError
{
	internal abstract class DelayCoreBase
	{
		protected DelayCoreBase(RetryDelayOptions delayOptions)
		{
			if (delayOptions.UseJitter)
			{
				DelayProvider = GetJitteredDelay;
			}
			else
			{
				DelayProvider = GetBaseDelay;
			}
		}

		public Func<int, TimeSpan> DelayProvider { get; }

		protected abstract TimeSpan GetBaseDelay(int attempt);

		protected abstract TimeSpan GetJitteredDelay(int attempt);
	}
}
