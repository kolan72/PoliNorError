using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class RetryDelayTests
	{
		[Test]
		[TestCase(RetryDelayType.Constant, true)]
		[TestCase(RetryDelayType.Constant, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		public void Should_RetryDelay_Returns_Correct_Timespan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(0, 1, 2);

			switch (retryDelayType)
			{
				case RetryDelayType.Constant:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(2));
					break;
				case RetryDelayType.Linear:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(4));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(6));
					break;
				case RetryDelayType.Exponential:
					Assert.That(res[0].TotalSeconds, Is.EqualTo(2));
					Assert.That(res[1].TotalSeconds, Is.EqualTo(4));
					Assert.That(res[2].TotalSeconds, Is.EqualTo(8));
					break;
			}

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Constant:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Constant, TimeSpan.FromSeconds(2));
						else
							return ConstantRetryDelay.Create(TimeSpan.FromSeconds(2));
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2));
						else
							return LinearRetryDelay.Create(TimeSpan.FromSeconds(2));
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2));
						else
							return ExponentialRetryDelay.Create(TimeSpan.FromSeconds(2));
					default:
						throw new NotImplementedException();
				}
			}
		}

		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		public void Should_RetryDelay_NotExceed_MaxDelay(RetryDelayType retryDelayType, bool useBaseClass)
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
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
						else
							return LinearRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1));
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
						else
							return ExponentialRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1));
					default:
						throw new NotImplementedException();
				}
			}
		}

		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		public void Should_RetryDelay_Returns_MaxTimeSpan_When_Calculated_One_Exceed_MaxTimeSpan(RetryDelayType retryDelayType, bool useBaseClass)
		{
			var rd = GetRetryDelayByRetryDelayType();

			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(2);

			Assert.That(res[0], Is.EqualTo(TimeSpan.MaxValue));

			RetryDelay GetRetryDelayByRetryDelayType()
			{
				switch (retryDelayType)
				{
					case RetryDelayType.Linear:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Linear, TimeSpan.MaxValue);
						else
							return LinearRetryDelay.Create(TimeSpan.MaxValue);
					case RetryDelayType.Exponential:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.Exponential, TimeSpan.MaxValue);
						else
							return ExponentialRetryDelay.Create(TimeSpan.MaxValue);
					default:
						throw new NotImplementedException();
				}
			}
		}

		[Test]
		public void Should_LinearRetryDelayWithSlopeFactor_Returns_Correct_Timespan()
		{
			var rd = LinearRetryDelay.Create(TimeSpan.FromSeconds(2), 2.0);
			var rdch = new RetryDelayChecker(rd);
			var res = rdch.Attempt(0, 1, 2);
			Assert.That(res[0].TotalSeconds, Is.EqualTo(4));
			Assert.That(res[1].TotalSeconds, Is.EqualTo(8));
			Assert.That(res[2].TotalSeconds, Is.EqualTo(12));
		}

		[Test]
		[TestCase(RetryDelayType.Constant, 2)]
		[TestCase(RetryDelayType.Linear, 6)]
		[TestCase(RetryDelayType.Exponential, 8)]
		public void Should_Set_RetryDelay_When_Initialized_Through_Constructor_With_Options(RetryDelayType retryDelayType, int equalTo)
		{
			var baseDelay = TimeSpan.FromMilliseconds(2);
			RetryDelay rd;
			switch (retryDelayType)
			{
				case RetryDelayType.Constant:
					rd = new RetryDelay(new ConstantRetryDelayOptions() { BaseDelay = baseDelay });
					break;
				case RetryDelayType.Linear:
					rd = new RetryDelay(new LinearRetryDelayOptions() { BaseDelay = baseDelay });
					break;
				case RetryDelayType.Exponential:
					rd = new RetryDelay(new ExponentialRetryDelayOptions() { BaseDelay = baseDelay });
					break;
				default:
					throw new NotImplementedException();
			}

			var res = rd.GetDelay(2);
			Assert.That(res.TotalMilliseconds, Is.EqualTo(equalTo));
		}

		[Test]
		public void Should_Set_RetryDelay_When_Initialized_Through_DelayProvider()
		{
			TimeSpan delayValueProvider(int _) => TimeSpan.FromSeconds(1);

			var rd = new RetryDelay(delayValueProvider);
			Assert.That(rd.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[Test]
		public void Should_Throw_When_DelayValueProviderIsNull()
		{
			Func<int, TimeSpan> nullProvider = null;

			Assert.Throws<ArgumentNullException>(() => new RetryDelay(nullProvider));
		}

		[Test]
		public void Should_Return_Exact_Delay_When_Within_Bounds()
		{
			var options = new TimeSeriesRetryDelayOptions
			{
				Times = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) },
				MaxDelay = TimeSpan.FromSeconds(10)
			};

			var retryDelay = new TimeSeriesRetryDelay(options);

			Assert.That(retryDelay.GetDelay(0), Is.EqualTo(TimeSpan.FromSeconds(1)));
			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(TimeSpan.FromSeconds(2)));
		}

		[Test]
		public void Should_Return_Last_Delay_When_Attempt_Exceeds_Index()
		{
			var maxTime = TimeSpan.FromSeconds(3);
			var options = new TimeSeriesRetryDelayOptions
			{
				Times = new[] { TimeSpan.FromSeconds(1), maxTime},
				MaxDelay = TimeSpan.FromSeconds(10)
			};

			var retryDelay = new TimeSeriesRetryDelay(options);

			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(2), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(3), Is.EqualTo(maxTime));
		}

		[Test]
		public void Should_Use_BaseDelay_If_Times_Is_Empty()
		{
			var baseTime = TimeSpan.FromSeconds(2);
			var options = new TimeSeriesRetryDelayOptions
			{
				Times = Array.Empty<TimeSpan>(),
				BaseDelay = baseTime,
				MaxDelay = TimeSpan.FromSeconds(5)
			};

			var retryDelay = new TimeSeriesRetryDelay(options);

			Assert.That(retryDelay.GetDelay(0), Is.EqualTo(baseTime));
			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(baseTime));
		}

		[Test]
		public void Should_Clamp_Delay_To_MaxDelay()
		{
			var maxTime = TimeSpan.FromSeconds(10);
			var options = new TimeSeriesRetryDelayOptions
			{
				Times = new[] { TimeSpan.FromSeconds(12) },
				MaxDelay = maxTime
			};

			var retryDelay = new TimeSeriesRetryDelay(options);

			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(2), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(3), Is.EqualTo(maxTime));
		}
	}
}
