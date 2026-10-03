using NUnit.Framework;
using System;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class PolicyCollectionIWithErrorFilterTests
	{
		private static PolicyCollection CreateCollectionWithTwoRetryPolicies()
		{
			return PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.WithPolicy(new RetryPolicy(1));
		}

		private static void AssertIncludedFiltersForAllPolicies(PolicyCollection collection, int expectedCount)
		{
			Assert.That(collection.Count(), Is.EqualTo(2));
			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(expectedCount));
			}
		}

		private static void AssertExcludedFiltersForAllPolicies(PolicyCollection collection, int expectedCount)
		{
			Assert.That(collection.Count(), Is.EqualTo(2));
			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(expectedCount));
			}
		}

		[Test]
		public void Should_IncludeError_Generic_Add_Included_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.IncludeError<ArgumentNullException>();

			AssertIncludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_IncludeError_Generic_With_Predicate_Add_Included_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.IncludeError<ArgumentNullException>(e => e.Message == "Test");

			AssertIncludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_IncludeError_Expression_Add_Included_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.IncludeError(ex => ex.Message == "Test");

			AssertIncludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_ExcludeError_Generic_Add_Excluded_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.ExcludeError<ArgumentNullException>();

			AssertExcludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_ExcludeError_Generic_With_Predicate_Add_Excluded_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.ExcludeError<ArgumentNullException>(e => e.Message == "Test");

			AssertExcludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_ExcludeError_Expression_Add_Excluded_Filter_To_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.ExcludeError(ex => ex.Message == "Test");

			AssertExcludedFiltersForAllPolicies(collection, 1);
		}

		[Test]
		public void Should_IncludeError_Generic_Return_Same_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			var result = collection.IncludeError<ArgumentNullException>();

			Assert.That(result, Is.SameAs(collection));
		}

		[Test]
		public void Should_IncludeError_Expression_Return_Same_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			var result = collection.IncludeError(ex => ex.Message == "Test");

			Assert.That(result, Is.SameAs(collection));
		}

		[Test]
		public void Should_ExcludeError_Generic_Return_Same_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			var result = collection.ExcludeError<ArgumentNullException>();

			Assert.That(result, Is.SameAs(collection));
		}

		[Test]
		public void Should_ExcludeError_Expression_Return_Same_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			var result = collection.ExcludeError(ex => ex.Message == "Test");

			Assert.That(result, Is.SameAs(collection));
		}

		[Test]
		public void Should_IncludeError_Generic_Add_Filter_Only_To_Included_Filters_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.IncludeError<ArgumentException>();

			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.IncludedErrorFilters, Is.Not.Empty);
				Assert.That(policy.PolicyProcessor.ErrorFilter.ExcludedErrorFilters, Is.Empty);
			}
		}

		[Test]
		public void Should_ExcludeError_Generic_Add_Filter_Only_To_Excluded_Filters_Collection()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection.ExcludeError<ArgumentException>();

			foreach (var policy in collection)
			{
				Assert.That(policy.PolicyProcessor.ErrorFilter.ExcludedErrorFilters, Is.Not.Empty);
				Assert.That(policy.PolicyProcessor.ErrorFilter.IncludedErrorFilters, Is.Empty);
			}
		}

		[Test]
		public void Should_IncludeError_Accumulate_Multiple_Filters_For_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection
				.IncludeError<ArgumentNullException>()
				.IncludeError(ex => ex.Message == "Test");

			AssertIncludedFiltersForAllPolicies(collection, 2);
		}

		[Test]
		public void Should_ExcludeError_Accumulate_Multiple_Filters_For_All_Policies()
		{
			var collection = CreateCollectionWithTwoRetryPolicies();

			collection
				.ExcludeError<ArgumentNullException>()
				.ExcludeError(ex => ex.Message == "Test");

			AssertExcludedFiltersForAllPolicies(collection, 2);
		}

		[Test]
		public void Should_IncludeError_Generic_Handle_Matching_Exception_As_Filter_Satisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.IncludeError<ArgumentNullException>();

			var result = collection.Handle(() => throw new ArgumentNullException("Test"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.False);
		}

		[Test]
		public void Should_IncludeError_Generic_Handle_Non_Matching_Exception_As_Filter_Unsatisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.IncludeError<ArgumentNullException>();

			var result = collection.Handle(() => throw new InvalidOperationException("Test"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
		}

		[Test]
		public void Should_IncludeError_Expression_Handle_Matching_Exception_As_Filter_Satisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.IncludeError(ex => ex.Message == "Test");

			var result = collection.Handle(() => throw new Exception("Test"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.False);
		}

		[Test]
		public void Should_IncludeError_Expression_Handle_Non_Matching_Exception_As_Filter_Unsatisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.IncludeError(ex => ex.Message == "Test");

			var result = collection.Handle(() => throw new Exception("Other"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
		}

		[Test]
		public void Should_ExcludeError_Generic_Handle_Excluded_Exception_As_Filter_Unsatisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.ExcludeError<ArgumentNullException>();

#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var result = collection.Handle(() => throw new ArgumentNullException("Test"));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
		}

		[Test]
		public void Should_ExcludeError_Generic_Handle_Not_Excluded_Exception_As_Filter_Satisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.ExcludeError<ArgumentNullException>();

			var result = collection.Handle(() => throw new InvalidOperationException("Test"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.False);
		}

		[Test]
		public void Should_ExcludeError_Expression_Handle_Excluded_Exception_As_Filter_Unsatisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.ExcludeError(ex => ex.Message == "Test");

			var result = collection.Handle(() => throw new Exception("Test"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.True);
		}

		[Test]
		public void Should_ExcludeError_Expression_Handle_Not_Excluded_Exception_As_Filter_Satisfied()
		{
			var collection = PolicyCollection.Create()
				.WithPolicy(new RetryPolicy(1))
				.ExcludeError(ex => ex.Message == "Test");

			var result = collection.Handle(() => throw new Exception("Other"));

			Assert.That(result.ErrorFilterUnsatisfied, Is.False);
		}

		[Test]
		public void Should_IncludeError_Not_Throw_For_Empty_Collection()
		{
			var collection = PolicyCollection.Create();

			Assert.That(() => collection.IncludeError<ArgumentException>(), Throws.Nothing);
			Assert.That(() => collection.IncludeError(ex => true), Throws.Nothing);
		}

		[Test]
		public void Should_ExcludeError_Not_Throw_For_Empty_Collection()
		{
			var collection = PolicyCollection.Create();

			Assert.That(() => collection.ExcludeError<ArgumentException>(), Throws.Nothing);
			Assert.That(() => collection.ExcludeError(ex => true), Throws.Nothing);
		}
	}
}
