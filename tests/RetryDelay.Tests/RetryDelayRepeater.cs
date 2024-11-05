using System;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class RetryDelayRepeater
	{
		private readonly RetryDelay _retryDelay;
		public RetryDelayRepeater(RetryDelay retryDelay)
		{
			_retryDelay = retryDelay;
		}

		public List<TimeSpan> Repeat(int attemptNumber, int numOfRepeats)
		{
			var times = new List<TimeSpan>();
			for (int i = 0; i < numOfRepeats; i++)
			{
				times.Add(_retryDelay.GetDelay(attemptNumber));
			}
			return times;
		}
	}
}
