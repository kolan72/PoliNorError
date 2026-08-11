using NUnit.Framework;
using System;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError.Tests
{
	[TestFixture]
	internal class ExceptionFilterExtensionsTests
	{
		[Test]
		public void Should_ReturnFalse_WhenFilterMatches()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>();

			var ex = new ArgumentException("test");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.True);
			Assert.That(filterException, Is.Null);
		}

		[Test]
		public void Should_ReturnFalse_WhenFilterMatches_And_RethrowTrue()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentNullException>();

#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var ex = new ArgumentNullException("paramName", "message");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, true, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.True);
			Assert.That(filterException, Is.Null);
		}

		[Test]
		public void Should_ReturnTrue_WhenFilterDoesNotMatch_And_RethrowIsTrue()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>();

			var ex = new NullReferenceException("null ref");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, true, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.True);
			Assert.That(filterAccepted, Is.False);
			Assert.That(filterException, Is.Null);
			Assert.That(ex.Data.Contains(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY), Is.True);
		}

		[Test]
		public void Should_ReturnFalse_WhenFilterDoesNotMatch_And_RethrowIsFalse()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>();

			var ex = new NullReferenceException("null ref");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.False);
			Assert.That(filterException, Is.Null);
		}

		[Test]
		public void Should_NotSetDataKey_WhenFilterDoesNotMatch_And_RethrowIsFalse()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>();

			var ex = new InvalidOperationException("invalid op");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, false, out _, out _);

			Assert.That(result, Is.False);
			Assert.That(ex.Data.Contains(PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY), Is.False);
		}

		[Test]
		public void Should_SetDataKeyValueToTrue_WhenFilterDoesNotMatch_And_RethrowIsTrue()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<InvalidOperationException>();

			var ex = new ArgumentException("arg");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, true, out _, out _);

			Assert.That(result, Is.True);
			Assert.That(ex.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY], Is.True);
		}

		[Test]
		public void Should_ReturnFalse_WithEmptyFilter_WhenNoFiltersConfigured()
		{
			var filter = new ExceptionFilter();

			var ex = new Exception("any exception");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, false, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.True);
			Assert.That(filterException, Is.Null);
		}

		[Test]
		public void Should_ReturnFalse_WithEmptyFilter_WhenNoFiltersAndRethrowEnabled()
		{
			var filter = new ExceptionFilter();

			var ex = new Exception("any exception");
			bool result = filter.ShouldPropagateFilterUnsatisfied(ex, true, out bool filterAccepted, out Exception filterException);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.True);
			Assert.That(filterException, Is.Null);
		}

		[Test]
		public void Should_ReturnCorrectly_WhenExcludedFilterPreventsMatching()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>().ExcludeError<ArgumentException>((e) => e.ParamName == "param");

#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			var excludedEx = new ArgumentException("excluded", "param");
			bool excludedResult = filter.ShouldPropagateFilterUnsatisfied(excludedEx, true, out bool excludedAccepted, out _);

			Assert.That(excludedResult, Is.True);
			Assert.That(excludedAccepted, Is.False);

			var includedEx = new ArgumentException("included", "not_param");
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			bool includedResult = filter.ShouldPropagateFilterUnsatisfied(includedEx, false, out bool includedAccepted, out _);

			Assert.That(includedResult, Is.False);
			Assert.That(includedAccepted, Is.True);
		}

		[Test]
		public void Should_ReturnFalse_WhenInnerExceptionDoesNotMatch()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);

			var outer = new Exception("Test", new NullReferenceException("not argument"));
			bool result = filter.ShouldPropagateFilterUnsatisfied(outer, false, out bool filterAccepted, out _);

			Assert.That(result, Is.False);
			Assert.That(filterAccepted, Is.False);
		}

		[Test]
		public void Should_ReturnFalse_WhenInnerExceptionMatches()
		{
			var filter = new ExceptionFilter();
			filter.IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);

			var outer = new Exception("Test", new NullReferenceException("not arg"));
			bool notMatchResult = filter.ShouldPropagateFilterUnsatisfied(outer, false, out bool notMatchAccepted, out _);

			Assert.That(notMatchResult, Is.False);
			Assert.That(notMatchAccepted, Is.False);

			var matchingOuter = new Exception("Test", new ArgumentException("matching argument"));
			bool matchResult = filter.ShouldPropagateFilterUnsatisfied(matchingOuter, false, out bool matchAccepted, out Exception filterException);

			Assert.That(matchResult, Is.False);
			Assert.That(matchAccepted, Is.True);
			Assert.That(filterException, Is.Null);
		}
	}
}
