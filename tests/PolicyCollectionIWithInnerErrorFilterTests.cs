using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionIWithInnerErrorFilterTests
	{
		private static PolicyCollection CreateCollectionWithTwoRetryPolicies()
		{
			return PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.WithPolicy(new RetryPolicy(1));
		}

		private static void AssertIncludedFiltersForAllPolicies(PolicyCollection collection, int expectedCount)
		{
			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(expectedCount));
			}
		}

		private static void AssertExcludedFiltersForAllPolicies(PolicyCollection collection, int expectedCount)
		{
			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(expectedCount));
			}
		}

		[Test]
		public void Should_IWithInnerErrorFilter_IncludeInnerError_Add_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			((IWithInnerErrorFilter<PolicyCollection>)collection).IncludeInnerError<TestInnerException>();

			AssertIncludedFiltersForAllPolicies(collection, 1);
			AssertExcludedFiltersForAllPolicies(collection, 0);
		}

		[Test]
		public void Should_IWithInnerErrorFilter_ExcludeInnerError_Add_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			((IWithInnerErrorFilter<PolicyCollection>)collection).ExcludeInnerError<TestInnerException>();

			AssertExcludedFiltersForAllPolicies(collection, 1);
			AssertIncludedFiltersForAllPolicies(collection, 0);
		}

		[Test]
		public async Task Should_IWithInnerErrorFilter_IncludeInnerError_Handle_Matching_Inner_Exception_As_Filter_Satisfied_For_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			((IWithInnerErrorFilter<PolicyCollection>)collection).IncludeInnerError<TestInnerException>();

			var policyDelegateCollection = collection.ToPolicyDelegateCollection(ActionWithInner);
			var handleRes = await policyDelegateCollection.HandleAllAsync();

			foreach (var result in handleRes.PolicyDelegateResults.Select(phr => phr.Result))
			{
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public async Task Should_IWithInnerErrorFilter_ExcludeInnerError_Handle_Excluded_Inner_Exception_As_Filter_Unsatisfied_For_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			((IWithInnerErrorFilter<PolicyCollection>)collection).ExcludeInnerError<TestInnerException>();

			var policyDelegateCollection = collection.ToPolicyDelegateCollection(ActionWithInner);
			var handleRes = await policyDelegateCollection.HandleAllAsync();

			foreach (var result in handleRes.PolicyDelegateResults.Select(phr => phr.Result))
			{
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}
	}
}
