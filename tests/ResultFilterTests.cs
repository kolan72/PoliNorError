using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class ResultFilterSetTests
	{
		[Test]
		public void Should_InitializeWithEmptyExcludedFilters()
		{
			var filterSet = new ResultFilterSet<string>();

			Assert.That(filterSet.ExcludedErrorFilters, Is.Empty);
		}

		[Test]
		public void Should_AddExcludedFilter()
		{
			var filterSet = new ResultFilterSet<string>();
			Expression<Func<string, bool>> filter = s => s.Length > 5;

			filterSet.ExcludeFilter(filter);

			Assert.That(filterSet.ExcludedErrorFilters, Has.Count.EqualTo(1));
			Assert.That(filterSet.ExcludedErrorFilters, Contains.Item(filter));
		}

		[Test]
		public void Should_AppendExcludedFiltersFromOtherFilterSet()
		{
			var filterSet1 = new ResultFilterSet<string>();
			var filterSet2 = new ResultFilterSet<string>();

			Expression<Func<string, bool>> filter = s => s.Length > 5;

			filterSet2.ExcludeFilter(filter);

			filterSet1.AppendFilter(filterSet2);

			Assert.That(filterSet1.ExcludedErrorFilters, Contains.Item(filter));
		}

		[Test]
		public void Should_CompilePredicate_ReturnAlwaysTrue_WhenNoFilters()
		{
			var filterSet = new ResultFilterSet<string>();
			var predicate = filterSet.CompilePredicate();

			Assert.That(predicate("hello"), Is.True);
			Assert.That(predicate(""), Is.True);
			Assert.That(predicate("anything"), Is.True);
		}

		[Test]
		public void Should_CompilePredicate_WithExcludedFilters()
		{
			var filterSet = new ResultFilterSet<string>();
			filterSet.ExcludeFilter(s => s == "excluded");

			var predicate = filterSet.CompilePredicate();

			Assert.That(predicate("excluded"), Is.False);
			Assert.That(predicate("included"), Is.True);
		}

		[Test]
		public void Should_CompilePredicate_WithMultipleExcludedFilters()
		{
			var filterSet = new ResultFilterSet<string>();
			filterSet.ExcludeFilter(s => s == "excluded1");
			filterSet.ExcludeFilter(s => s == "excluded2");

			var predicate = filterSet.CompilePredicate();

			Assert.That(predicate("excluded1"), Is.False);
			Assert.That(predicate("excluded2"), Is.False);
			Assert.That(predicate("other"), Is.True);
		}

		[Test]
		public void Should_CompilePredicate_AfterAppendFilter_PreserveExistingFilters()
		{
			var originalSet = new ResultFilterSet<string>();
			var additionalSet = new ResultFilterSet<string>();

			Expression<Func<string, bool>> originalFilter = s => s == "a";
			Expression<Func<string, bool>> additionalFilter = s => s == "b";

			originalSet.ExcludeFilter(originalFilter);
			additionalSet.ExcludeFilter(additionalFilter);

			originalSet.AppendFilter(additionalSet);

			Assert.That(originalSet.ExcludedErrorFilters, Has.Count.EqualTo(2));
			Assert.That(originalSet.ExcludedErrorFilters, Contains.Item(originalFilter));
			Assert.That(originalSet.ExcludedErrorFilters, Contains.Item(additionalFilter));
		}

		[Test]
		public void Should_Work_With_NonStringType()
		{
			var filterSet = new ResultFilterSet<int>();
			filterSet.ExcludeFilter(i => i > 10);

			var predicate = filterSet.CompilePredicate();

			Assert.That(predicate(5), Is.True);
			Assert.That(predicate(15), Is.False);
			Assert.That(predicate(20), Is.False);
		}
	}

	[TestFixture]
	internal class ResultFilterTests
	{
		[Test]
		public void Should_HaveEmptyExcludedFilters_ByDefault()
		{
			var filter = new ResultFilter<string>();

			Assert.That(filter.ExcludedErrorFilters, Is.Empty);
		}

		[Test]
		public void Should_GetCanHandle_ReturnAlwaysTrue_WhenNoFilters()
		{
			var filter = new ResultFilter<string>();
			var canHandle = filter.GetCanHandle();

			Assert.That(canHandle("hello"), Is.True);
			Assert.That(canHandle(""), Is.True);
		}

		[Test]
		public void Should_ExcludeError_AddToExcludedFilters()
		{
			var filter = new ResultFilter<string>();
			filter.ExcludeError(s => s == "excluded");

			Assert.That(filter.ExcludedErrorFilters, Has.Count.EqualTo(1));
			Assert.That(filter.GetCanHandle()("excluded"), Is.False);
			Assert.That(filter.GetCanHandle()("included"), Is.True);
		}

		[Test]
		public void Should_ExcludeError_ReturnSelf()
		{
			var filter = new ResultFilter<string>();
			var result = filter.ExcludeError(s => s == "excluded");

			Assert.That(result, Is.SameAs(filter));
		}

		[Test]
		public void Should_AddExcludedErrorFilter_StoreFilter()
		{
			var filter = new ResultFilter<string>();
			Expression<Func<string, bool>> expression = s => s.Length > 3;

			filter.AddExcludedErrorFilter(expression);

			Assert.That(filter.ExcludedErrorFilters, Has.Count.EqualTo(1));
		}

		[Test]
		public void Should_AppendFilter_MergeExcludedFilters()
		{
			var filter = new ResultFilter<string>();
			filter.ExcludeError(s => s == "excluded1");

			var appendedFilter = new ResultFilter<string>();
			appendedFilter.ExcludeError(s => s == "excluded2");

			filter.AppendFilter(appendedFilter);

			Assert.That(filter.ExcludedErrorFilters, Has.Count.EqualTo(2));
			Assert.That(filter.GetCanHandle()("excluded1"), Is.False);
			Assert.That(filter.GetCanHandle()("excluded2"), Is.False);
			Assert.That(filter.GetCanHandle()("other"), Is.True);
		}

		[Test]
		public void Should_GetSlim_Match_GetCanHandle()
		{
			var filter = new ResultFilter<string>();
			filter.ExcludeError(s => s == "excluded");

			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("excluded"), Is.False);
			Assert.That(slim.CanHandle("included"), Is.True);
			Assert.That(slim.CanHandle("excluded"), Is.EqualTo(filter.GetCanHandle()("excluded")));
			Assert.That(slim.CanHandle("included"), Is.EqualTo(filter.GetCanHandle()("included")));
		}

		[Test]
		public void Should_GetSlim_Match_GetCanHandle_WhenNoFilters()
		{
			var filter = new ResultFilter<string>();
			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("anything"), Is.EqualTo(filter.GetCanHandle()("anything")));
		}

		[Test]
		public void Should_GetSlim_Match_GetCanHandle_AfterAppendFilter()
		{
			var filter = new ResultFilter<string>();
			filter.ExcludeError(s => s == "first");

			var appended = new ResultFilter<string>();
			appended.ExcludeError(s => s == "second");

			filter.AppendFilter(appended);
			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("first"), Is.False);
			Assert.That(slim.CanHandle("second"), Is.False);
			Assert.That(slim.CanHandle("other"), Is.True);
		}

	}

	[TestFixture]
	internal class ResultFilterSlimTests
	{
		[Test]
		public void Should_CanHandle_Match_FilterGetCanHandle_WhenNoFilters()
		{
			var filter = new ResultFilter<string>();
			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("hello"), Is.EqualTo(filter.GetCanHandle()("hello")));
		}

		[Test]
		public void Should_CanHandle_Match_FilterGetCanHandle_ForExcludedFilters()
		{
			var filter = new ResultFilter<string>();
			filter.ExcludeError(s => s == "filtered");

			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("filtered"), Is.EqualTo(false));
			Assert.That(slim.CanHandle("not_filtered"), Is.EqualTo(true));
			Assert.That(slim.CanHandle("filtered"), Is.EqualTo(filter.GetCanHandle()("filtered")));
		}

		[Test]
		public void Should_CanHandle_Match_FilterGetCanHandle_AfterAppendFilter()
		{
			var filter = new ResultFilter<string>();
			var appended = new ResultFilter<string>();
			appended.ExcludeError(s => s == "appended_excluded");

			filter.AppendFilter(appended);
			var slim = filter.GetSlim();

			Assert.That(slim.CanHandle("appended_excluded"), Is.EqualTo(false));
			Assert.That(slim.CanHandle("other"), Is.EqualTo(true));
			Assert.That(slim.CanHandle("appended_excluded"), Is.EqualTo(filter.GetCanHandle()("appended_excluded")));
		}
	}
}
