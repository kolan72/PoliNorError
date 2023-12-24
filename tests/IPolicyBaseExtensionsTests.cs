using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace PoliNorError.Tests
{
	internal class IPolicyBaseExtensionsTests
	{
		[Test]
		public void Should_WithErrorProcessors_Add_ErrorProcessors()
		{
			var pol = new RetryPolicy(1)
						.WithErrorProcessor(new BasicErrorProcessor())
						.WithErrorProcessor(new BasicErrorProcessor())
						.WithErrorProcessor(new BasicErrorProcessor());
			ClassicAssert.AreEqual(3, ((DefaultRetryProcessor)pol.PolicyProcessor).Count());
		}

		[Test]
		public void Should_WithErrorProcessors_Add_DefaultErrorProcessorsByAction()
		{
			void act(Exception _, CancellationToken __) => Expression.Empty();
			var pol = new RetryPolicy(1).WithErrorProcessorOf(act);
			ClassicAssert.AreEqual(typeof(BasicErrorProcessor), pol.PolicyProcessor.FirstOrDefault()?.GetType());
		}
	}
}
