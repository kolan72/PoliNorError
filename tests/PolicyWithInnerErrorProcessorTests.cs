using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	internal class PolicyWithInnerErrorProcessorTests
	{
		[Test]
		[TestCase(PolicyAlias.Simple, true, true)]
		[TestCase(PolicyAlias.Simple, true, false)]
		[TestCase(PolicyAlias.Simple, false, false)]
		[TestCase(PolicyAlias.Simple, false, true)]
		[TestCase(PolicyAlias.Retry, true, true)]
		[TestCase(PolicyAlias.Retry, true, false)]
		[TestCase(PolicyAlias.Retry, false, false)]
		[TestCase(PolicyAlias.Retry, false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(PolicyAlias policyAlias, bool sync, bool withCancellationType)
		{
			async Task shorthandHandlerFunc<T>(T pol) where T : IPolicyBase, IWithInnerErrorProcessor<T>
			{
				await PolicyWithInnerErrorProcessorForTest.Handle(pol, sync, withCancellationType);
			}
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					await shorthandHandlerFunc(new SimplePolicy());
					break;
				case PolicyAlias.Retry:
					await shorthandHandlerFunc(new RetryPolicy(1));
					break;
			}
		}
	}
}
