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
			CatchBlockFilter filter = null;
			if (generic)
			{
				filter = new CatchBlockFilter().IncludeError<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.IncludeError<ArgumentException>((_) => true);
				filter.ExcludeError<ArgumentException>((_) => true);
			}
			else
			{
				filter = new CatchBlockFilter().IncludeError<ArgumentException>((ex) => ex.Message == "Test");
				filter.IncludeError((_) => true);
				filter.ExcludeError((_) => true);
			}

			Assert.That(filter.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(1));
			Assert.That(filter.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(2));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_ErrorFilter_Created_By_FromExcludedError_Add_ErrorFilter_Expressions_Correctly(bool generic)
		{
			CatchBlockFilter filter = null;
			if (generic)
			{
				filter = new CatchBlockFilter().ExcludeError<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.IncludeError<ArgumentException>((_) => true);
				filter.ExcludeError<ArgumentException>((_) => true);
			}
			else
			{
				filter = new CatchBlockFilter().ExcludeError((ex) => ex.Message == "Test");
				filter.IncludeError((_) => true);
				filter.ExcludeError((_) => true);
			}

			Assert.That(filter.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(2));
			Assert.That(filter.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(1));
		}

		[Test]
		public void Should_Empty_Create_Empty_Instance()
		{
			var instance = CatchBlockFilter.Empty();
			Assert.That(instance, Is.Not.Null);
			Assert.That(instance.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(0));
			Assert.That(instance.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(0));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_FilledCatchBlockFilter_CreateByIncluding_Add_ErrorFilter_Expressions_Correctly(bool generic)
		{
			NonEmptyCatchBlockFilter filter = null;
			if (generic)
			{
				filter = NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.IncludeError<ArgumentException>((_) => true);
				filter.ExcludeError<ArgumentException>((_) => true);
			}
			else
			{
				filter = NonEmptyCatchBlockFilter.CreateByIncluding((ex) => ex.Message == "Test");
				filter.IncludeError((_) => true);
				filter.ExcludeError((_) => true);
			}

			Assert.That(filter.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(1));
			Assert.That(filter.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(2));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_FilledCatchBlockFilter_CreateByExcluding_Add_ErrorFilter_Expressions_Correctly(bool generic)
		{
			NonEmptyCatchBlockFilter filter = null;
			if (generic)
			{
				filter = NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>((ex) => ex.ParamName == "Test");
				filter.IncludeError<ArgumentException>((_) => true);
				filter.ExcludeError<ArgumentException>((_) => true);
			}
			else
			{
				filter = NonEmptyCatchBlockFilter.CreateByExcluding((ex) => ex.Message == "Test");
				filter.IncludeError((_) => true);
				filter.ExcludeError((_) => true);
			}

			Assert.That(filter.ErrorFilter.ExcludedErrorFilters.Count(), Is.EqualTo(2));
			Assert.That(filter.ErrorFilter.IncludedErrorFilters.Count(), Is.EqualTo(1));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_CatchBlockFilter_IncludeError_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = CatchBlockFilter.Empty().IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
			Exception errorToHandler;
			if (errFilterUnsatisfied)
			{
				errorToHandler = new TestExceptionWithInnerArgumentNullException();
			}
			else
			{
				errorToHandler = new TestExceptionWithInnerArgumentException();
			}

			var actualErrFilterUnsatisfied = !filter.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(actualErrFilterUnsatisfied, Is.EqualTo(errFilterUnsatisfied));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_CatchBlockFilter_ExcludeError_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = CatchBlockFilter.Empty().ExcludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
			Exception errorToHandler;
			if (errFilterUnsatisfied)
			{
				errorToHandler = new TestExceptionWithInnerArgumentException();
			}
			else
			{
				errorToHandler = new TestExceptionWithInnerArgumentNullException();
			}

			var actualErrFilterUnsatisfied = !filter.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(actualErrFilterUnsatisfied, Is.EqualTo(errFilterUnsatisfied));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_NonEmptyCatchBlockFilter_IncludeError_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>().IncludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
			Exception errorToHandler;
			if (errFilterUnsatisfied)
			{
				errorToHandler = new TestExceptionWithInnerArgumentNullException();
			}
			else
			{
				errorToHandler = new TestExceptionWithInnerArgumentException();
			}

			var actualErrFilterUnsatisfied = !filter.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(actualErrFilterUnsatisfied, Is.EqualTo(errFilterUnsatisfied));
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_NonEmptyCatchBlockFilter_ExcludeError_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>().ExcludeError<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
			Exception errorToHandler;
			if (errFilterUnsatisfied)
			{
				errorToHandler = new TestExceptionWithInnerArgumentException();
			}
			else
			{
				errorToHandler = new TestExceptionWithInnerArgumentNullException();
			}

			var actualErrFilterUnsatisfied = !filter.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(actualErrFilterUnsatisfied, Is.EqualTo(errFilterUnsatisfied));
		}
	}
}