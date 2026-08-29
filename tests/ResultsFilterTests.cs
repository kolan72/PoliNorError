using NUnit.Framework;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class ResultsFilterTests
	{
		[Test]
		public void Should_GetIsSuccessful_ReturnAlwaysTrue_ForUnregisteredType()
		{
			var filter = new ResultsFilter();

			var isSuccessful = filter.GetIsSuccessful<string>();

			Assert.That(isSuccessful("anything"), Is.True);
		}

		[Test]
		public void Should_ExcludeResult_StoreFilter_ForSpecificType()
		{
			var filter = new ResultsFilter();

			filter.ExcludeResult<string>(s => s == "excluded");

			var isSuccessful = filter.GetIsSuccessful<string>();

			Assert.That(isSuccessful("excluded"), Is.False);
			Assert.That(isSuccessful("included"), Is.True);
		}

		[Test]
		public void Should_ExcludeResult_ReturnSelf()
		{
			var filter = new ResultsFilter();

			var result = filter.ExcludeResult<string>(s => s == "excluded");

			Assert.That(result, Is.SameAs(filter));
		}

		[Test]
		public void Should_IsolateFilters_AcrossDifferentTypes()
		{
			var filter = new ResultsFilter();

			filter.ExcludeResult<string>(s => s == "excluded");
			filter.ExcludeResult<int>(i => i > 10);

			Assert.That(filter.GetIsSuccessful<string>()("excluded"), Is.False);
			Assert.That(filter.GetIsSuccessful<string>()("included"), Is.True);

			Assert.That(filter.GetIsSuccessful<int>()(15), Is.False);
			Assert.That(filter.GetIsSuccessful<int>()(5), Is.True);
		}

		[Test]
		public void Should_NotLeakFilters_BetweenTypes()
		{
			var filter = new ResultsFilter();

			filter.ExcludeResult<int>(i => i > 10);

			// string type has no registered filter, so always true.
			Assert.That(filter.GetIsSuccessful<string>()("15"), Is.True);
		}

		[Test]
		public void Should_AppendFilter_MergeMatchesOfOtherFilter()
		{
			var filter = new ResultsFilter();
			filter.ExcludeResult<string>(s => s == "first");

			var appended = new ResultsFilter();
			appended.ExcludeResult<string>(s => s == "second");
			appended.ExcludeResult<int>(i => i > 100);

			filter.AppendFilter(appended);

			Assert.That(filter.GetIsSuccessful<string>()("first"), Is.False);
			Assert.That(filter.GetIsSuccessful<string>()("second"), Is.False);
			Assert.That(filter.GetIsSuccessful<string>()("other"), Is.True);
			Assert.That(filter.GetIsSuccessful<int>()(150), Is.False);
			Assert.That(filter.GetIsSuccessful<int>()(50), Is.True);
		}

		[Test]
		public void Should_GetSlim_IsSuccessful_Match_GetIsSuccessful()
		{
			var filter = new ResultsFilter();
			filter.ExcludeResult<string>(s => s == "excluded");
			filter.ExcludeResult<int>(i => i > 10);

			var slim = filter.GetSlim();

			Assert.That(slim.IsSuccessful("excluded"), Is.False);
			Assert.That(slim.IsSuccessful("included"), Is.True);
			Assert.That(slim.IsSuccessful(15), Is.False);
			Assert.That(slim.IsSuccessful(5), Is.True);
		}

		[Test]
		public void Should_GetSlim_IsSuccessful_ReturnTrue_ForUnregisteredType()
		{
			var filter = new ResultsFilter();
			var slim = filter.GetSlim();

			Assert.That(slim.IsSuccessful("anything"), Is.True);
		}
	}
}