using NUnit.Framework;
using System;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class ExceptionFilterSlimTests
	{
		[Test]
		public void Should_SlimMatchGetCanHandle_ForNoFilter()
		{
			var errorFilter = new ExceptionFilter();
			var slim = errorFilter.GetSlim();

			Assert.That(slim.CanHandle(new Exception()), Is.EqualTo(errorFilter.GetCanHandle()(new Exception())));
		}

		[Test]
		public void Should_SlimMatchGetCanHandle_ForIncludeFilter()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.AddIncludedErrorFilter((ex) => ex.GetType().Equals(typeof(ArgumentNullException)));
			var slim = errorFilter.GetSlim();

			Assert.That(slim.CanHandle(new ArgumentNullException("Test", (Exception)null)), Is.EqualTo(true));
			Assert.That(slim.CanHandle(new Exception("Test")), Is.EqualTo(false));
			Assert.That(slim.CanHandle(new ArgumentNullException("Test", (Exception)null)), Is.EqualTo(errorFilter.GetCanHandle()(new ArgumentNullException("Test", (Exception)null))));
		}

		[Test]
		public void Should_SlimMatchGetCanHandle_ForExcludeFilter()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.AddExcludedErrorFilter((ex) => ex.Message == "Test");
			var slim = errorFilter.GetSlim();

			Assert.That(slim.CanHandle(new Exception("Test")), Is.EqualTo(false));
			Assert.That(slim.CanHandle(new Exception("Test2")), Is.EqualTo(true));
		}

		[Test]
		public void Should_SlimMatchGetCanHandle_ForIncludeAndExcludeFilters()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>().ExcludeError<ArgumentException>((e) => e.ParamName == "param");
			var slim = errorFilter.GetSlim();

#pragma warning disable S3928
			var excludedEx = new ArgumentException("excluded", "param");
			Assert.That(slim.CanHandle(excludedEx), Is.EqualTo(false));

			var includedEx = new ArgumentException("included", "not_param");
			Assert.That(slim.CanHandle(includedEx), Is.EqualTo(true));
#pragma warning restore S3928
		}

		[Test]
		public void Should_SlimMatchGetCanHandle_ForInnerErrorIncludeFilter()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
			var slim = errorFilter.GetSlim();

			Assert.That(slim.CanHandle(new Exception("Test", new NullReferenceException("not argument"))), Is.EqualTo(false));
			Assert.That(slim.CanHandle(new Exception("Test", new ArgumentException("matching argument"))), Is.EqualTo(true));
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_WhenFilterMatches()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>();
			var slim = errorFilter.GetSlim();

			var ex = new ArgumentException("test");
			bool slimResult = slim.ShouldPropagateFilterUnsatisfied(ex, false, out bool slimAccepted, out Exception slimException);
			bool filterResult = errorFilter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(slimResult, Is.EqualTo(filterResult));
			Assert.That(slimAccepted, Is.EqualTo(filterAccepted));
			Assert.That(slimException, Is.EqualTo(filterException));
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_WhenFilterDoesNotMatch_And_RethrowIsTrue()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>();
			var slim = errorFilter.GetSlim();

			var ex = new NullReferenceException("null ref");
			bool slimResult = slim.ShouldPropagateFilterUnsatisfied(ex, true, out bool slimAccepted, out Exception slimException);
			bool filterResult = errorFilter.ShouldPropagateFilterUnsatisfied(ex, true, out bool filterAccepted, out Exception filterException);

			Assert.That(slimResult, Is.EqualTo(filterResult));
			Assert.That(slimAccepted, Is.EqualTo(filterAccepted));
			Assert.That(slimException, Is.EqualTo(filterException));
			Assert.That(ex.Data.Contains(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY), Is.True);
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_WhenFilterDoesNotMatch_And_RethrowIsFalse()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>();
			var slim = errorFilter.GetSlim();

			var ex = new NullReferenceException("null ref");
			bool slimResult = slim.ShouldPropagateFilterUnsatisfied(ex, false, out bool slimAccepted, out Exception slimException);
			bool filterResult = errorFilter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(slimResult, Is.EqualTo(filterResult));
			Assert.That(slimAccepted, Is.EqualTo(filterAccepted));
			Assert.That(slimException, Is.EqualTo(filterException));
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_ForEmptyFilter()
		{
			var errorFilter = new ExceptionFilter();
			var slim = errorFilter.GetSlim();

			var ex = new Exception("any exception");
			bool slimResult = slim.ShouldPropagateFilterUnsatisfied(ex, false, out bool slimAccepted, out Exception slimException);
			bool filterResult = errorFilter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(slimResult, Is.EqualTo(filterResult));
			Assert.That(slimAccepted, Is.EqualTo(filterAccepted));
			Assert.That(slimException, Is.EqualTo(filterException));
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_ForInnerError()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);

			var slim = errorFilter.GetSlim();

			var outer = new Exception("Test", new NullReferenceException("not argument"));
			bool slimResult = slim.ShouldPropagateFilterUnsatisfied(outer, false, out bool slimAccepted, out _);
			bool filterResult = errorFilter.ShouldPropagateFilterUnsatisfied(outer, false, out bool filterAccepted, out _);

			Assert.That(slimResult, Is.EqualTo(filterResult));
			Assert.That(slimAccepted, Is.EqualTo(filterAccepted));
		}

		[Test]
		public void Should_SlimMatchGetCanHandle_WhenFilterAppended()
		{
			var errorFilter = new ExceptionFilter();
			var appendedFilter = new ExceptionFilter();
			appendedFilter.AddExcludedErrorFilter((ex) => ex.Message == "excluded");

			errorFilter.AppendFilter(appendedFilter);
			var slim = errorFilter.GetSlim();

			Assert.That(slim.CanHandle(new Exception("excluded")), Is.EqualTo(false));
			Assert.That(slim.CanHandle(new Exception("other")), Is.EqualTo(true));
			Assert.That(slim.CanHandle(new Exception("excluded")), Is.EqualTo(errorFilter.GetCanHandle()(new Exception("excluded"))));
		}

		[Test]
		public void Should_SlimShouldPropagateFilterUnsatisfied_Match_FilterVersion_WhenExcludedFilterPreventsMatching()
		{
			var errorFilter = new ExceptionFilter();
			errorFilter.IncludeError<ArgumentException>().ExcludeError<ArgumentException>((e) => e.ParamName == "param");
			var slim = errorFilter.GetSlim();

#pragma warning disable S3928
			var excludedEx = new ArgumentException("excluded", "param");
			var includedEx = new ArgumentException("included", "not_param");
#pragma warning restore S3928

			bool slimExcludedResult = slim.ShouldPropagateFilterUnsatisfied(excludedEx, true, out bool slimExcludedAccepted, out _);
			bool filterExcludedResult = errorFilter.ShouldPropagateFilterUnsatisfied(excludedEx, true, out bool filterExcludedAccepted, out _);

			Assert.That(slimExcludedResult, Is.EqualTo(filterExcludedResult));
			Assert.That(slimExcludedAccepted, Is.EqualTo(filterExcludedAccepted));

			bool slimIncludedResult = slim.ShouldPropagateFilterUnsatisfied(includedEx, false, out bool slimIncludedAccepted, out _);
			bool filterIncludedResult = errorFilter.ShouldPropagateFilterUnsatisfied(includedEx, false, out bool filterIncludedAccepted, out _);

			Assert.That(slimIncludedResult, Is.EqualTo(filterIncludedResult));
			Assert.That(slimIncludedAccepted, Is.EqualTo(filterIncludedAccepted));
		}
	}
}
