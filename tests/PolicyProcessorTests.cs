using NUnit.Framework;
using NUnit.Framework.Legacy;
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
			ClassicAssert.AreEqual(true, errorFiltter.GetCanHandle()(new Exception()));
		}

		[Test]
		public void Should_ExceptionFilter_Work_For_IncludeFilter()
		{
			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			errorFiltter.AddIncludedErrorFilter((ex) => ex.GetType().Equals(typeof(ArgumentNullException)));
			ClassicAssert.AreEqual(1, errorFiltter.IncludedErrorFilters.Count());

			ClassicAssert.AreEqual(true, errorFiltter.GetCanHandle()(new ArgumentNullException("Test", (Exception)null)));
			ClassicAssert.AreEqual(false, errorFiltter.GetCanHandle()(new Exception("Test")));
		}

		[Test]
		public void Should_ExceptionFilter_Work_For_ExcludeFilter()
		{
			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			errorFiltter.AddExcludedErrorFilter((ex) => ex.Message == "Test");
			ClassicAssert.AreEqual(1, errorFiltter.ExcludedErrorFilters.Count());

			ClassicAssert.AreEqual(false, errorFiltter.GetCanHandle()(new Exception("Test")));
			ClassicAssert.AreEqual(true, errorFiltter.GetCanHandle()(new Exception("Test2")));
		}
	}
}