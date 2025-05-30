using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class RetryDelayJitteredTests
	{
		[Test]
		[TestCase(RetryDelayType.Constant, true)]
		[TestCase(RetryDelayType.Constant, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.TimeSeries, true)]
		[TestCase(RetryDelayType.TimeSeries, false)]
		public void Should_RetryDelay_Returns_Jittered_Timespan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var repeater = new RetryDelayRepeater(GetJitteredRetryDelayByRetryDelayType());
			var res = repeater.Repeat(1, 10);
			RetryDelay GetJitteredRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Constant:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Constant, TimeSpan.FromSeconds(4), true);
						else
							return ConstantRetryDelay.Create(TimeSpan.FromSeconds(4), null, true);
					case RetryDelayType.Linear:
						if (useBaseClass)
						{
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2), true);
						}
						else
						{
							return LinearRetryDelay.Create(TimeSpan.FromSeconds(2), null, true);
						}
					case RetryDelayType.TimeSeries:
						if (useBaseClass)
						{
							return new RetryDelay(RetryDelayType.TimeSeries, TimeSpan.FromSeconds(4), true);
						}
						else
						{
							return TimeSeriesRetryDelay.Create(new List<TimeSpan>() {TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(6)}, null, true);
						}
					default:
						throw new NotImplementedException();
				}
			}
			Assert.That(res.Exists(t => Math.Abs(4 - t.TotalSeconds) > 0), Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ExponentialRetryDelay_Returns_Jittered_Timespan(bool useBaseClass)
		{
			var times = new List<TimeSpan>();
			for (int i = 0; i < 10; i++)
			{
				times.Add(GetRetryDelay().GetDelay(1));
			}

			Assert.That(times.Exists(t => Math.Abs(4 - t.TotalSeconds) > 0), Is.True);

			RetryDelay GetRetryDelay()
			{
				if (useBaseClass)
				{
					return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2), true);
				}
				else
				{
					return ExponentialRetryDelay.Create(TimeSpan.FromSeconds(2), useJitter: true);
				}
			}
		}

		[TestCase(RetryDelayType.Constant, true)]
		[TestCase(RetryDelayType.Constant, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.TimeSeries, true)]
		[TestCase(RetryDelayType.TimeSeries, false)]
		public void Should_RetryDelayJittered_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.LessThanOrEqualTo(TimeSpan.MaxValue));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Constant:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Constant, TimeSpan.MaxValue, true);
						else
							return ConstantRetryDelay.Create(TimeSpan.MaxValue, null, useJitter: true);
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.MaxValue, true);
						else
							return LinearRetryDelay.Create(TimeSpan.MaxValue, useJitter: true);
					case RetryDelayType.TimeSeries:
						if (useBaseClass)
						{
							return new RetryDelay(RetryDelayType.TimeSeries, TimeSpan.MaxValue, true);
						}
						else
						{
							return TimeSeriesRetryDelay.Create(new List<TimeSpan>() { TimeSpan.MaxValue, TimeSpan.MaxValue, TimeSpan.MaxValue}, null, true);
						}
					default:
						throw new NotImplementedException();
				}
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_ExponentialRetryDelay_Jittered_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(RetryDelayConstants.MaxTimeSpanFromTicks));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				if (useBaseClass)
					return new RetryDelay(RetryDelayType.Exponential, TimeSpan.MaxValue, true);
				else
					return ExponentialRetryDelay.Create(TimeSpan.MaxValue, useJitter: true);
			}
		}

		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.TimeSeries, true)]
		[TestCase(RetryDelayType.TimeSeries, false)]
		public void Should_RetryDelayJittered_NotExceed_MaxDelay(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(TimeSpan.FromSeconds(1)));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), true);
						else
							return LinearRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1), true);
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), true);
						else
							return ExponentialRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1), useJitter: true);
					case RetryDelayType.TimeSeries:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.TimeSeries, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1), true);
						else
							return TimeSeriesRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1), useJitter: true);
					default:
						throw new NotImplementedException();
				}
			}
		}

		[Test]
		public void Should_ConstantRetryDelayJittered_Throw_If_MaxDelay_LessThan_BaseDelay()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => new ConstantRetryDelay(new ConstantRetryDelayOptions() { BaseDelay = TimeSpan.FromSeconds(2), MaxDelay = TimeSpan.FromSeconds(1), UseJitter = true }));
		}
	}
}
