using NUnit.Framework;
using System.Linq;
using static PoliNorError.Tests.ExceptionFilterTests;

namespace PoliNorError.Tests
{
	internal class ICanAddErrorFilterTests
	{
		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_FilterErrors_WhenErrorFilterIsAdded_AndNoFiltersExist(bool excludeFilterWork, bool useSelector)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			IRetryProcessor retryProcessor;
			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				retryProcessor = new DefaultRetryProcessor().AddErrorFilter(appendedFilter);
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				retryProcessor = new DefaultRetryProcessor().AddErrorFilter(appendedFilterSelector);
			}

			Assert.That(retryProcessor.ErrorFilter.ExcludedErrorFilters.Count, Is.EqualTo(1));
			Assert.That(retryProcessor.ErrorFilter.IncludedErrorFilters.Count, Is.EqualTo(1));

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsEmpty();

			Assert.That(retryProcessor.Retry(() => throw errorToHandle, 1).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, true, true)]
		[TestCase(false, false, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, true, false)]
		[TestCase(false, false, false)]
		public void Should_FilterErrors_WhenErrorFilterIsAdded_AndFiltersExist(bool excludeFilterWork, bool useSelector, bool checkOriginExceptFiler)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			var retryProcessor = new DefaultRetryProcessor()
									.AddErrorFilter(errProvider.GetCatchBlockFilterFromIncludeAndExclude());

			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				retryProcessor.AddErrorFilter(appendedFilter);
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				retryProcessor.AddErrorFilter(appendedFilterSelector);
			}

			Assert.That(retryProcessor.ErrorFilter.ExcludedErrorFilters.Count, Is.EqualTo(2));
			Assert.That(retryProcessor.ErrorFilter.IncludedErrorFilters.Count, Is.EqualTo(2));

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsNotEmpty(checkOriginExceptFiler);

			Assert.That(retryProcessor.Retry(() => throw errorToHandle, 1).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}
	}
}
