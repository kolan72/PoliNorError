using NUnit.Framework;
using System;
using System.Linq;

namespace PoliNorError.Tests
{
	internal class ExceptionFilterTests
	{
		[Test]
		public void Should_ExceptionFilter_Work_For_NoFilter()
		{
			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			Assert.AreEqual(true, errorFiltter.GetCanHandle()(new Exception()));
		}

		[Test]
		public void Should_ExceptionFilter_Work_For_IncludeFilter()
		{
			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			errorFiltter.AddIncludedErrorFilter((ex) => ex.GetType().Equals(typeof(ArgumentNullException)));
			Assert.AreEqual(1, errorFiltter.IncludedErrorFilters.Count());

			Assert.AreEqual(true, errorFiltter.GetCanHandle()(new ArgumentNullException("Test", (Exception)null)));
			Assert.AreEqual(false, errorFiltter.GetCanHandle()(new Exception("Test")));
		}

		[Test]
		public void Should_ExceptionFilter_Work_For_ExcludeFilter()
		{
			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			errorFiltter.AddExcludedErrorFilter((ex) => ex.Message == "Test");
			Assert.AreEqual(1, errorFiltter.ExcludedErrorFilters.Count());

			Assert.AreEqual(false, errorFiltter.GetCanHandle()(new Exception("Test")));
			Assert.AreEqual(true, errorFiltter.GetCanHandle()(new Exception("Test2")));
		}
	}
}