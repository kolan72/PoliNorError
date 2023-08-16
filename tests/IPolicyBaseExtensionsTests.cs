using NUnit.Framework;
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
						.WithErrorProcessor(new DefaultErrorProcessor())
						.WithErrorProcessor(new DefaultErrorProcessor())
						.WithErrorProcessor(new DefaultErrorProcessor());
			Assert.AreEqual(3, ((DefaultRetryProcessor)pol.PolicyProcessor).Count());
		}

		[Test]
		public void Should_WithErrorProcessors_Add_DefaultErrorProcessorsByAction()
		{
			void act(Exception _, CancellationToken __) => Expression.Empty();
			var pol = new RetryPolicy(1).WithErrorProcessorOf(act);
			Assert.AreEqual(typeof(DefaultErrorProcessorV2), pol.PolicyProcessor.FirstOrDefault()?.GetType());
		}
	}
}
