using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class EnumerablePolicyExtensionsTests
	{
		private static List<IPolicyBase> CreatePoliciesWithTwoRetryPolicies()
		{
			return new List<IPolicyBase> { new RetryPolicy(1), new RetryPolicy(1) };
		}

		private static void AssertIncludedFiltersForAllPolicies(IEnumerable<IPolicyBase> policies, int expectedIncludedCount)
		{
			foreach (var policy in policies)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(expectedIncludedCount));
			}
		}

		private static void AssertExcludedFiltersForAllPolicies(IEnumerable<IPolicyBase> policies, int expectedExcludedCount)
		{
			foreach (var policy in policies)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(expectedExcludedCount));
			}
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Add_Included_Filter_To_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>();

			AssertIncludedFiltersForAllPolicies(policies, 1);
			AssertExcludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_With_Predicate_Add_Included_Filter_To_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			AssertIncludedFiltersForAllPolicies(policies, 1);
			AssertExcludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Accumulate_Multiple_Filters_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>();
			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			AssertIncludedFiltersForAllPolicies(policies, 2);
			AssertExcludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Handle_Matching_Inner_Exception_As_Filter_Satisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>();

			foreach (var policy in policies)
			{
				var result = policy.Handle(ActionWithInner);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Handle_Inner_Exception_Without_Inner_Error_As_Filter_Unsatisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>();

			foreach (var policy in policies)
			{
				var result = policy.Handle(Action);
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_With_Predicate_Handle_Satisfied_Inner_Exception_As_Filter_Satisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			foreach (var policy in policies)
			{
				var result = policy.Handle(((Action<string>)ActionWithInnerWithMsg).Apply("Test"));
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_With_Predicate_Handle_Unsatisfied_Inner_Exception_As_Filter_Unsatisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			foreach (var policy in policies)
			{
				var result = policy.Handle(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"));
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Not_Throw_For_Empty_Collection()
		{
			var policies = new List<IPolicyBase>();

			Assert.That(() => policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(), Throws.Nothing);
			Assert.That(() => policies.AddIncludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test"), Throws.Nothing);
		}

		[Test]
		public void Should_AddIncludedInnerErrorFilterForAll_Work_For_Single_Policy_Collection()
		{
			var policies = new List<IPolicyBase> { new SimplePolicy() };

			policies.AddIncludedInnerErrorFilterForAll<TestInnerException>();

			AssertIncludedFiltersForAllPolicies(policies, 1);
			Assert.That(policies.Single().Handle(ActionWithInner).ErrorFilterUnsatisfied, Is.False);
			Assert.That(policies.Single().Handle(Action).ErrorFilterUnsatisfied, Is.True);
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Add_Excluded_Filter_To_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>();

			AssertExcludedFiltersForAllPolicies(policies, 1);
			AssertIncludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_With_Predicate_Add_Excluded_Filter_To_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			AssertExcludedFiltersForAllPolicies(policies, 1);
			AssertIncludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Accumulate_Multiple_Filters_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>();
			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			AssertExcludedFiltersForAllPolicies(policies, 2);
			AssertIncludedFiltersForAllPolicies(policies, 0);
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Handle_Excluded_Inner_Exception_As_Filter_Unsatisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>();

			foreach (var policy in policies)
			{
				var result = policy.Handle(ActionWithInner);
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Handle_Exception_Without_Inner_Error_As_Filter_Satisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>();

			foreach (var policy in policies)
			{
				var result = policy.Handle(Action);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_With_Predicate_Handle_Satisfied_Inner_Exception_As_Filter_Unsatisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			foreach (var policy in policies)
			{
				var result = policy.Handle(((Action<string>)ActionWithInnerWithMsg).Apply("Test"));
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_With_Predicate_Handle_Unsatisfied_Inner_Exception_As_Filter_Satisfied_For_All_Policies()
		{
			var policies = CreatePoliciesWithTwoRetryPolicies();

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test");

			foreach (var policy in policies)
			{
				var result = policy.Handle(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"));
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Not_Throw_For_Empty_Collection()
		{
			var policies = new List<IPolicyBase>();

			Assert.That(() => policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(), Throws.Nothing);
			Assert.That(() => policies.AddExcludedInnerErrorFilterForAll<TestInnerException>(ex => ex.Message == "Test"), Throws.Nothing);
		}

		[Test]
		public void Should_AddExcludedInnerErrorFilterForAll_Work_For_Single_Policy_Collection()
		{
			var policies = new List<IPolicyBase> { new SimplePolicy() };

			policies.AddExcludedInnerErrorFilterForAll<TestInnerException>();

			AssertExcludedFiltersForAllPolicies(policies, 1);
			Assert.That(policies.Single().Handle(ActionWithInner).ErrorFilterUnsatisfied, Is.True);
			Assert.That(policies.Single().Handle(Action).ErrorFilterUnsatisfied, Is.False);
		}
	}
}
