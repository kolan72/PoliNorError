using System;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class RetryDelayChecker
	{
		private readonly RetryDelay _retryDelay;
		public RetryDelayChecker(RetryDelay retryDelay)
		{
			_retryDelay = retryDelay;
		}

		public List<TimeSpan> Attempt(params int[] attemptNumbers)
		{
			var times = new List<TimeSpan>();
			foreach (var an in attemptNumbers)
			{
				times.Add(_retryDelay.GetDelay(an));
			}
			return times;
		}
	}
}
