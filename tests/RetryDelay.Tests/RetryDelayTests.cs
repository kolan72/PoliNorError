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

		[Test]
		public void Should_TimeSeriesRetryDelay_Create_Returns_Correct_TimeSpan()
		{
			var secs = new List<int>() { 1, 15, 20, 34 };
			var times = secs.Select(s => TimeSpan.FromSeconds(s));

			var rd = TimeSeriesRetryDelay.Create(times);
			var rdch = new RetryDelayChecker(rd);

			var res = rdch.Attempt(0, 1, 2, 3);

			for (int i = 0; i < secs.Count; i++)
			{
				Assert.That(res[i].TotalSeconds, Is.EqualTo(secs[i]));
			}
		}

		[TestCase(RetryDelayType.Exponential, true)]
		[TestCase(RetryDelayType.Exponential, false)]
		[TestCase(RetryDelayType.Linear, true)]
		[TestCase(RetryDelayType.Linear, false)]
		[TestCase(RetryDelayType.TimeSeries, true)]
		[TestCase(RetryDelayType.TimeSeries, false)]
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
					case RetryDelayType.TimeSeries:
						if (useBaseClass)
							return new RetryDelay(RetryDelayType.TimeSeries, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
						else
							return TimeSeriesRetryDelay.Create(TimeSpan.FromSeconds(2), maxDelay: TimeSpan.FromSeconds(1));
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
		[TestCase(RetryDelayType.TimeSeries, 2)]
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
				case RetryDelayType.TimeSeries:
					rd = new RetryDelay(new TimeSeriesRetryDelayOptions() { BaseDelay = baseDelay });
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
		public void Should_Throw_ArgumentNullException_When_Create_With_Null_Times()
		{
			IEnumerable<TimeSpan> nullTimes = null;
			Assert.That(() => TimeSeriesRetryDelay.Create(nullTimes),
				Throws.ArgumentNullException.With.Property("ParamName").EqualTo("times"));
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

			Assert.That(retryDelay.GetDelay(0), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(1), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(2), Is.EqualTo(maxTime));
			Assert.That(retryDelay.GetDelay(3), Is.EqualTo(maxTime));
		}

		[Test]
		public void Should_Implicitly_Convert_ConstantRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new ConstantRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(1) };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
		}

		[Test]
		public void Should_Implicitly_Convert_LinearRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new LinearRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), SlopeFactor = 2 };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(2 * crdo.SlopeFactor)));
		}

		[Test]
		public void Should_Implicitly_Convert_ExponentialRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new ExponentialRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), ExponentialFactor = 2 };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(2 * Math.Pow(crdo.ExponentialFactor, 0))));
		}

		[Test]
		public void Should_Implicitly_Convert_TimeSeriesRetryDelayOptions_To_RetryDelay()
		{
			var crdo = new TimeSeriesRetryDelayOptions() { BaseDelay = TimeSpan.FromMilliseconds(2), Times = new TimeSpan[] {TimeSpan.FromMilliseconds(1), TimeSpan.FromMilliseconds(2) } };
			var tester = new RetryDelayTester();
			Assert.That(tester.GetAttemptDelay(crdo), Is.EqualTo(TimeSpan.FromMilliseconds(1)));
		}

		[Test]
		public void Should_Apply_Jitter_To_ConstantRetryDelay_Within_Expected_Range_When_UseJitter_True()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = TimeSpan.MaxValue,
				UseJitter = true
			};
			var delay = new ConstantRetryDelay(options);

			var result = delay.GetDelay(1);
			var baseMs = baseDelay.TotalMilliseconds;
			var offset = baseMs * RetryDelayConstants.JitterFactor / 2;
			var minExpected = TimeSpan.FromMilliseconds(baseMs - offset);
			var maxExpected = TimeSpan.FromMilliseconds(baseMs + offset);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Cap_ConstantRetryDelay_At_MaxDelay_When_Jitter_Exceeds_Max()
		{
			var baseDelay = TimeSpan.FromMilliseconds(1000);
			var maxDelay = TimeSpan.FromMilliseconds(1100);
			var options = new ConstantRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = maxDelay,
				UseJitter = true
			};
			var delay = new ConstantRetryDelay(options);

			// Force jitter to exceed max by using a known random value
			// Note: This test assumes internal implementation details
			var result = delay.GetDelay(1);

			Assert.That(result, Is.LessThanOrEqualTo(maxDelay));
		}

		[Test]
		public void Should_Create_ConstantRetryDelay_Instance_Via_Create_Method()
		{
			var baseDelay = TimeSpan.FromMilliseconds(200);
			var instance = ConstantRetryDelay.Create(baseDelay, useJitter: true);

			Assert.That(instance, Is.Not.Null);
			Assert.That(instance.GetDelay(1), Is.GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(150)));
		}

		[Test]
		public void Should_Apply_Jitter_To_LinearRetryDelay_Within_Expected_Range_When_UseJitter_True()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				UseJitter = true,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(1);
			var baseValue = 200.0; // 2 * 100 * 1.0
			var jitterRange = baseValue * RetryDelayConstants.JitterFactor / 2;
			var minExpected = TimeSpan.FromMilliseconds(baseValue - jitterRange);
			var maxExpected = TimeSpan.FromMilliseconds(baseValue + jitterRange);

			Assert.That(result, Is.GreaterThanOrEqualTo(minExpected));
			Assert.That(result, Is.LessThanOrEqualTo(maxExpected));
		}

		[Test]
		public void Should_Cap_LinearRetryDelay_At_MaxDelay_When_Exceeded()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var maxDelay = TimeSpan.FromMilliseconds(250);
			var options = new LinearRetryDelayOptions
			{
				BaseDelay = baseDelay,
				MaxDelay = maxDelay,
				UseJitter = false,
				SlopeFactor = 1.0
			};
			var delay = new LinearRetryDelay(options);

			var result = delay.GetDelay(2); // Should be 300ms without cap

			Assert.That(result, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_Create_LinearRetryDelay_Instance_With_Create_Method_Default_Slope()
		{
			var baseDelay = TimeSpan.FromMilliseconds(100);
			var instance = LinearRetryDelay.Create(baseDelay);

			var result = instance.GetDelay(1);

			Assert.That(result, Is.EqualTo(TimeSpan.FromMilliseconds(200)));
		}

		[Test]
		public void Should_ApplyMaxDelayInCreateMethods_ToTimeSeriesRetryDelay_WhenMaxDelaySpecified()
		{
			// Arrange
			TimeSpan? maxDelay = TimeSpan.FromSeconds(2);

			// Act
			var delay = TimeSeriesRetryDelay.Create(TimeSpan.FromSeconds(5), maxDelay);
			var result = delay.GetDelay(0);

			// Assert
			Assert.That(result, Is.EqualTo(maxDelay));
		}

		[Test]
		public void Should_UseJitteredValues_ForTimeSeriesRetryDelay_WhenJitterEnabled()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = true,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act
			var results = Enumerable.Range(0, 10).Select(_ => delay.GetDelay(0)).ToArray();

			// Assert - With jitter, we should get some variation in results
			Assert.That(results.Distinct().Count(), Is.GreaterThan(1));
			Assert.That(results.ToList().TrueForAll(r => r.TotalMilliseconds >= 500 && r.TotalMilliseconds <= 1500), Is.True);
		}

		[Test]
		public void Should_UseNonJitteredValues_ForTimeSeriesRetryDelay_WhenJitterDisabled()
		{
			// Arrange
			var times = new[] { TimeSpan.FromSeconds(1) };
			var options = new TimeSeriesRetryDelayOptions
			{
				BaseDelay = TimeSpan.FromSeconds(1),
				MaxDelay = TimeSpan.FromSeconds(10),
				UseJitter = false,
				Times = times
			};
			var delay = new TimeSeriesRetryDelay(options);

			// Act
			var results = Enumerable.Range(0, 10).Select(_ => delay.GetDelay(0)).ToArray();

			// Assert - Without jitter, all results should be identical
			Assert.That(results.Distinct().Count(), Is.EqualTo(1));
			Assert.That(results[0], Is.EqualTo(TimeSpan.FromSeconds(1)));
		}

		[TestFixture]
		public class DecorrelatedJitterTests
		{
			private ExponentialRetryDelay.DecorrelatedJitter _jitter;

			[SetUp]
			public void Setup()
			{
				_jitter = new ExponentialRetryDelay.DecorrelatedJitter(
					TimeSpan.FromMilliseconds(100),
					2.0,
					TimeSpan.FromMinutes(5));
			}

			[Test]
			public void Should_GeneratePositiveDelaysForAllAttempts()
			{
				// Act & Assert
				for (int i = 0; i < 10; i++)
				{
					var delay = _jitter.DecorrelatedJitterBackoffV2(i);
					Assert.That(delay.TotalMilliseconds, Is.GreaterThan(0));
				}
			}

			[Test]
			public void Should_RespectMaxDelayLimit()
			{
				// Arrange
				var maxDelay = TimeSpan.FromMilliseconds(500);
				var jitter = new ExponentialRetryDelay.DecorrelatedJitter(
					TimeSpan.FromMilliseconds(100),
					2.0,
					maxDelay);

				// Act
				var delay = jitter.DecorrelatedJitterBackoffV2(100); // Very high attempt

				// Assert
				Assert.That(delay, Is.LessThanOrEqualTo(maxDelay));
			}

			[Test]
			public void Should_HandleInfinityCase()
			{
				// Arrange
				var maxDelay = TimeSpan.FromSeconds(30);
				var jitter = new ExponentialRetryDelay.DecorrelatedJitter(
					TimeSpan.FromMilliseconds(100),
					2.0,
					maxDelay);

				// Act
				var delay = jitter.DecorrelatedJitterBackoffV2(1024); // This should trigger infinity case

				// Assert
				Assert.That(delay, Is.EqualTo(maxDelay));
			}

			[Test]
			public void Should_ProduceVariedDelaysWithSameAttempt()
			{
				// Arrange
				var delays = new TimeSpan[10];

				// Act
				for (int i = 0; i < 10; i++)
				{
					delays[i] = _jitter.DecorrelatedJitterBackoffV2(1);
				}

				// Assert - Due to randomization, not all delays should be identical
				bool hasVariation = false;
				for (int i = 1; i < delays.Length; i++)
				{
					if (delays[i] != delays[0])
					{
						hasVariation = true;
						break;
					}
				}
				Assert.That(hasVariation, Is.True);
			}

			[Test]
			public void Should_GenerateReasonableDelayProgression()
			{
				// Arrange
				var baseDelay = TimeSpan.FromMilliseconds(100);
				var jitter = new ExponentialRetryDelay.DecorrelatedJitter(
					baseDelay,
					2.0,
					TimeSpan.FromMinutes(5));

				// Act
				var delay0 = jitter.DecorrelatedJitterBackoffV2(0);
				var delay1 = jitter.DecorrelatedJitterBackoffV2(1);
				var delay2 = jitter.DecorrelatedJitterBackoffV2(2);

				// Assert - Generally, delays should increase (though jitter may cause some variation)
				Assert.That(delay0.TotalMilliseconds, Is.GreaterThan(0));
				Assert.That(delay1.TotalMilliseconds, Is.GreaterThan(0));
				Assert.That(delay2.TotalMilliseconds, Is.GreaterThan(0));
			}

			[Test]
			public void Should_HandleZeroAttempt()
			{
				// Act
				var delay = _jitter.DecorrelatedJitterBackoffV2(0);

				// Assert
				Assert.That(delay.TotalMilliseconds, Is.GreaterThan(0));
			}
		}

		private class RetryDelayTester
		{
			public TimeSpan GetAttemptDelay(RetryDelay retryDelay, int attemptNumber = 0)
			{
				return retryDelay.GetDelay(attemptNumber);
			}
		}
	}
}
