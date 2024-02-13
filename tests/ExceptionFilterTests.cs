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

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_ErrorFilter_Created_By_FromIncludedError_Add_ErrorFilter_Expressions_Correctly(bool generic)
		{
			ErrorFilter filter = null;
			if (generic)
			{
				filter = ErrorFilter.FromIncludedError<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.Include<ArgumentException>((_) => true);
				filter.Exclude<ArgumentException>((_) => true);
			}
			else
			{
				filter = ErrorFilter.FromIncludedError((ex) => ex.Message == "Test");
				filter.Include((_) => true);
				filter.Exclude((_) => true);
			}

			Assert.That(filter.ExcludedErrorFilters.Count(), Is.EqualTo(1));
			Assert.That(filter.IncludedErrorFilters.Count(), Is.EqualTo(2));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_ErrorFilter_Created_By_FromExcludedError_Add_ErrorFilter_Expressions_Correctly(bool generic)
		{
			ErrorFilter filter = null;
			if (generic)
			{
				filter = ErrorFilter.FromExcludedError<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.Include<ArgumentException>((_) => true);
				filter.Exclude<ArgumentException>((_) => true);
			}
			else
			{
				filter = ErrorFilter.FromExcludedError((ex) => ex.Message == "Test");
				filter.Include((_) => true);
				filter.Exclude((_) => true);
			}

			Assert.That(filter.ExcludedErrorFilters.Count(), Is.EqualTo(2));
			Assert.That(filter.IncludedErrorFilters.Count(), Is.EqualTo(1));
		}
	}
}