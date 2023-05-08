using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PoliNorError.Tests
{
	internal class PolicyProcessorTests
	{
		[Test]
		public void Should_CanRetryHolder_CanRetry_Be_True_When_IncludeAndExcludeAreEmpty()
		{
			var canRetryMock = PolicyProcessor.CanHandleHolder.Create(Array.Empty<Expression<Func<Exception, bool>>>(), Array.Empty<Expression<Func<Exception, bool>>>());
			Assert.AreEqual(true, canRetryMock.GetCanHandle()(new Exception()));
		}

		[Test]
		public void Should_CanRetryHolder_CanRetry_Be_True_When_IncludeAndExcludeAreNull()
		{
			var canRetryMock = PolicyProcessor.CanHandleHolder.Create();
			Assert.AreEqual(true, canRetryMock.GetCanHandle()(new Exception()));
		}

		[Test]
		public void Should_CanRetryHolder_CanRetry_Be_False_If_Error_Satisfy_IncludeAndExclude_Filter()
		{
			var includedFilter = new List<Expression<Func<Exception, bool>>>() { (ex)  =>ex.GetType().Equals(typeof(ArgumentNullException))};

			var excludedFilter = new List<Expression<Func<Exception, bool>>>() { (ex) => ex.Message == "Test"};

			var canRetryMock = PolicyProcessor.CanHandleHolder.Create(includedFilter, excludedFilter);
			Assert.AreEqual(false, canRetryMock.GetCanHandle()(new ArgumentNullException("Test", (Exception)null)));
		}

		[Test]
		public void Should_CanRetryHolder_CanRetry_Be_False_If_Error_Satisfy_OnlyExclude_Filter()
		{
			var excludedFilter = new List<Expression<Func<Exception, bool>>>() { (ex) => ex.Message == "Test" };

			var canRetryMock = PolicyProcessor.CanHandleHolder.Create(null, excludedFilter);
			Assert.AreEqual(false, canRetryMock.GetCanHandle()(new ArgumentNullException("Test", (Exception)null)));
		}

		[Test]
		public void Should_CanRetryHolder_CanRetry_Be_True_If_Error_Satisfy_OnlyInclude_Filter()
		{
			var includedFilter = new List<Expression<Func<Exception, bool>>>() { (ex) => ex.GetType().Equals(typeof(ArgumentNullException))};

			var canRetryMock = PolicyProcessor.CanHandleHolder.Create(includedFilter, null);
			Assert.AreEqual(true, canRetryMock.GetCanHandle()(new ArgumentNullException("Test", (Exception)null)));
		}
	}
}