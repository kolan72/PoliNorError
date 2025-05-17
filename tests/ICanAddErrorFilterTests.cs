using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
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
		public void Should_DefaultRetryProcessor_FilterErrors_WhenErrorFilterIsAdded_AndNoFiltersExist(bool excludeFilterWork, bool useSelector)
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
		public void Should_DefaultRetryProcessor_FilterErrors_WhenErrorFilterIsAdded_AndFiltersExist(bool excludeFilterWork, bool useSelector, bool checkOriginExceptFiler)
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

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, false, true)]
		[TestCase(FallbackTypeForTests.BaseClass, false, false)]
		public void Should_FallbackPolicy_FilterErrors_WhenErrorFilterIsAdded_AndNoFiltersExist(FallbackTypeForTests policyAlias, bool excludeFilterWork, bool useSelector)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			FallbackPolicyBase fb = null;
			switch (policyAlias)
			{
				case FallbackTypeForTests.BaseClass:
					{
						fb = new FallbackPolicy()
								.WithAsyncFallbackFunc(async (_) => await Task.Delay(1))
								.WithFallbackAction((_) => { });
						break;
					}
				default:
					throw new NotImplementedException();
			}
			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				switch (policyAlias)
				{
					case FallbackTypeForTests.BaseClass:
					{
						fb = fb.AddErrorFilter(appendedFilter);
						break;
					}
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				switch (policyAlias)
				{
					case FallbackTypeForTests.BaseClass:
					{
						fb = fb.AddErrorFilter(appendedFilterSelector);
						break;
					}
				}
			}
			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsEmpty();

			Assert.That(fb.Handle(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}

		[Test]
		[TestCase(PolicyAlias.Retry, true, true)]
		[TestCase(PolicyAlias.Retry, true, false)]
		[TestCase(PolicyAlias.Retry, false, true)]
		[TestCase(PolicyAlias.Retry, false, false)]
		[TestCase(PolicyAlias.Simple, true, true)]
		[TestCase(PolicyAlias.Simple, true, false)]
		[TestCase(PolicyAlias.Simple, false, true)]
		[TestCase(PolicyAlias.Simple, false, false)]
		public void Should_FilterErrors_WhenErrorFilterIsAdded_AndNoFiltersExist(PolicyAlias policyAlias, bool excludeFilterWork, bool useSelector)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			IPolicyBase retryPolicy;
			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						retryPolicy = new RetryPolicy(1).AddErrorFilter(appendedFilter);
						break;
					case PolicyAlias.Simple:
						retryPolicy = new SimplePolicy().AddErrorFilter(appendedFilter);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						retryPolicy = new RetryPolicy(1).AddErrorFilter(appendedFilterSelector);
						break;
					case PolicyAlias.Simple:
						retryPolicy = new SimplePolicy().AddErrorFilter(appendedFilterSelector);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsEmpty();

			Assert.That(retryPolicy.Handle(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}

		[Test]
		[TestCase(FallbackTypeForTests.BaseClass, true, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false, true)]
		[TestCase(FallbackTypeForTests.BaseClass, false, true, true)]
		[TestCase(FallbackTypeForTests.BaseClass, false, false, true)]
		[TestCase(FallbackTypeForTests.BaseClass, true, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, true, false, false)]
		[TestCase(FallbackTypeForTests.BaseClass, false, true, false)]
		[TestCase(FallbackTypeForTests.BaseClass, false, false, false)]
		public void Should_FallbackPolicy_FilterErrors_WhenErrorFilterIsAdded_AndFiltersExist(FallbackTypeForTests policyAlias, bool excludeFilterWork, bool useSelector, bool checkOriginExceptFiler)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			FallbackPolicyBase retryPolicy;
			switch (policyAlias)
			{
				case FallbackTypeForTests.BaseClass:
					retryPolicy = new FallbackPolicy()
								.WithAsyncFallbackFunc(async (_) => await Task.Delay(1))
								.WithFallbackAction((_) => { })
								.AddErrorFilter(errProvider.GetCatchBlockFilterFromIncludeAndExclude());
					break;
				default:
					throw new NotImplementedException();
			}

			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				switch (policyAlias)
				{
					case FallbackTypeForTests.BaseClass:
						retryPolicy.AddErrorFilter(appendedFilter);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				switch (policyAlias)
				{
					case FallbackTypeForTests.BaseClass:
						retryPolicy.AddErrorFilter(appendedFilterSelector);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsNotEmpty(checkOriginExceptFiler);
			Assert.That(retryPolicy.Handle(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}

		[Test]
		[TestCase(PolicyAlias.Retry, true, true, true)]
		[TestCase(PolicyAlias.Retry, true, false, true)]
		[TestCase(PolicyAlias.Retry, false, true, true)]
		[TestCase(PolicyAlias.Retry, false, false, true)]
		[TestCase(PolicyAlias.Retry, true, true, false)]
		[TestCase(PolicyAlias.Retry, true, false, false)]
		[TestCase(PolicyAlias.Retry, false, true, false)]
		[TestCase(PolicyAlias.Retry, false, false, false)]
		[TestCase(PolicyAlias.Simple, true, true, true)]
		[TestCase(PolicyAlias.Simple, true, false, true)]
		[TestCase(PolicyAlias.Simple, false, true, true)]
		[TestCase(PolicyAlias.Simple, false, false, true)]
		[TestCase(PolicyAlias.Simple, true, true, false)]
		[TestCase(PolicyAlias.Simple, true, false, false)]
		[TestCase(PolicyAlias.Simple, false, true, false)]
		[TestCase(PolicyAlias.Simple, false, false, false)]
		public void Should_FilterErrors_WhenErrorFilterIsAdded_AndFiltersExist(PolicyAlias policyAlias, bool excludeFilterWork, bool useSelector, bool checkOriginExceptFiler)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			IPolicyBase retryPolicy;
			switch (policyAlias)
			{
				case PolicyAlias.Retry:
					retryPolicy = new RetryPolicy(1)
									.AddErrorFilter(errProvider.GetCatchBlockFilterFromIncludeAndExclude());
					break;
				case PolicyAlias.Simple:
					retryPolicy = new SimplePolicy()
									.AddErrorFilter(errProvider.GetCatchBlockFilterFromIncludeAndExclude());
					break;
				default:
					throw new NotImplementedException();
			}

			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				switch(policyAlias)
				{
					case PolicyAlias.Retry:
						((RetryPolicy)retryPolicy).AddErrorFilter(appendedFilter);
						break;
					case PolicyAlias.Simple:
						((SimplePolicy)retryPolicy).AddErrorFilter(appendedFilter);
						break;
					default:
						throw new NotImplementedException();
				}
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				switch (policyAlias)
				{
					case PolicyAlias.Retry:
						((RetryPolicy)retryPolicy).AddErrorFilter(appendedFilterSelector);
						break;
					case PolicyAlias.Simple:
						((SimplePolicy)retryPolicy).AddErrorFilter(appendedFilterSelector);
						break;
					default:
						throw new NotImplementedException();
				}
			}

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsNotEmpty(checkOriginExceptFiler);
			Assert.That(retryPolicy.Handle(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_SimplePolicyProcessor_FilterErrors_WhenErrorFilterIsAdded_AndNoFiltersExist(bool excludeFilterWork, bool useSelector)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			ISimplePolicyProcessor simpleProcessor;
			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				simpleProcessor = new SimplePolicyProcessor().AddErrorFilter(appendedFilter);
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				simpleProcessor = new SimplePolicyProcessor().AddErrorFilter(appendedFilterSelector);
			}

			Assert.That(simpleProcessor.ErrorFilter.ExcludedErrorFilters.Count, Is.EqualTo(1));
			Assert.That(simpleProcessor.ErrorFilter.IncludedErrorFilters.Count, Is.EqualTo(1));

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsEmpty();

			Assert.That(simpleProcessor.Execute(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
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
		public void Should_SimplePolicyProcessor_FilterErrors_WhenErrorFilterIsAdded_AndFiltersExist(bool excludeFilterWork, bool useSelector, bool checkOriginExceptFiler)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			var simpleProcessor = new SimplePolicyProcessor()
									.AddErrorFilter(errProvider.GetCatchBlockFilterFromIncludeAndExclude());

			if (!useSelector)
			{
				var appendedFilter = errProvider.GetNonEmptyCatchBlockFilter();
				simpleProcessor.AddErrorFilter(appendedFilter);
			}
			else
			{
				var appendedFilterSelector = errProvider.GetNonEmptyCatchBlockFilterSelector();
				simpleProcessor.AddErrorFilter(appendedFilterSelector);
			}

			Assert.That(simpleProcessor.ErrorFilter.ExcludedErrorFilters.Count, Is.EqualTo(2));
			Assert.That(simpleProcessor.ErrorFilter.IncludedErrorFilters.Count, Is.EqualTo(2));

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsNotEmpty(checkOriginExceptFiler);

			Assert.That(simpleProcessor.Execute(() => throw errorToHandle).ErrorFilterUnsatisfied, Is.EqualTo(excludeFilterWork));
		}
	}
}
