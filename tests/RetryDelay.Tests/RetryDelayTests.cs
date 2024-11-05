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
	}
}
