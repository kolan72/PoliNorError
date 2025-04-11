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
		[TestCase(true)]
		[TestCase(false)]
		public void Should_AppendFilter_When_OriginalFilterIsEmpty(bool excludeFilterWork)
		{
			var errProvider =  new AppendFilterExceptionProvider(excludeFilterWork);
			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsEmpty();

			var errorFiltter = new PolicyProcessor.ExceptionFilter();
			var appendedFilter = errProvider.GetFilterFromNonEmptyCatchBlockFilter();

			errorFiltter.AppendFilter(appendedFilter);

			Assert.That(errorFiltter.ExcludedErrorFilters.Count, Is.EqualTo(1));
			Assert.That(errorFiltter.IncludedErrorFilters.Count, Is.EqualTo(1));

			Assert.That(errorFiltter.GetCanHandle()(errorToHandle), Is.EqualTo(!excludeFilterWork));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, true)]
		[TestCase(false, false)]
		public void Should_AppendFilter_When_OriginalFilterIsNotEmpty(bool excludeFilterWork, bool checkOriginExceptFiler)
		{
			var errProvider = new AppendFilterExceptionProvider(excludeFilterWork);

			var errorFilter = errProvider.GetFilterFromIncludeAndExclude();

			var appendedFilter = errProvider.GetFilterFromNonEmptyCatchBlockFilter();

			var errorToHandle = errProvider.GetErrorWhenOriginalFilterIsNotEmpty(checkOriginExceptFiler);

			errorFilter.AppendFilter(appendedFilter);

			Assert.That(errorFilter.ExcludedErrorFilters.Count, Is.EqualTo(2));
			Assert.That(errorFilter.IncludedErrorFilters.Count, Is.EqualTo(2));

			Assert.That(errorFilter.GetCanHandle()(errorToHandle), Is.EqualTo(!excludeFilterWork));
		}

		public class AppendFilterExceptionProvider
		{
			private const string excParamName = "Test";
			private const string excParamNameToExclude = "Test2";

			private readonly bool _excludeFilterWork;

			public AppendFilterExceptionProvider(bool excludeFilterWork)
			{
				_excludeFilterWork = excludeFilterWork;
			}

			public Exception GetErrorWhenOriginalFilterIsEmpty()
			{
				if (!_excludeFilterWork)
				{
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
					return new ArgumentNullException(excParamNameToExclude, "");
				}
				else
				{
					return new ArgumentNullException(excParamName, "");
				}
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			}

			public Exception GetErrorWhenOriginalFilterIsNotEmpty(bool checkOriginExceptFiler)
			{
				if (!_excludeFilterWork)
				{
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
					return checkOriginExceptFiler ? new ArgumentException("", excParamNameToExclude) : new ArgumentNullException(excParamNameToExclude, "");
				}
				else
				{
					return checkOriginExceptFiler ? new ArgumentException("", excParamName) : new ArgumentNullException(excParamName, "");
				}
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 
			}

			public PolicyProcessor.ExceptionFilter GetFilterFromNonEmptyCatchBlockFilter()
			{
				return GetNonEmptyCatchBlockFilter()
									.ErrorFilter;
			}

			public NonEmptyCatchBlockFilter GetNonEmptyCatchBlockFilter()
			{
				return NonEmptyCatchBlockFilter
								.CreateByIncluding<ArgumentNullException>()
								.ExcludeError<ArgumentNullException>((ex) => ex.ParamName == excParamName);
			}

			public PolicyProcessor.ExceptionFilter GetFilterFromIncludeAndExclude()
			{
				var errorFilter = new PolicyProcessor.ExceptionFilter();
				errorFilter.AddIncludedErrorFilter<ArgumentException>();
				errorFilter.AddExcludedErrorFilter<ArgumentException>((ex) => ex.ParamName == excParamName);
				return errorFilter;
			}

			public Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> GetNonEmptyCatchBlockFilterSelector()
			{
				return (e) =>
							e.IncludeError<ArgumentNullException>()
							.ExcludeError<ArgumentNullException>((ex) => ex.ParamName == excParamName);
			}
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
		[TestCase(true)]
		[TestCase(false)]
		public void Should_NonEmptyCatchBlockFilter_Created_From_ErrorSet_By_CreateByIncluding_Method_Correct(bool inner)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			var filter = NonEmptyCatchBlockFilter.CreateByIncluding(errorSet);
			var canHandle = IsErrorCanBeHandledByNonEmptyCatchBlockFilter(filter, inner);
			Assert.That(canHandle, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_NonEmptyCatchBlockFilter_With_ErrorSet_Added_By_IncludeErrorSet_Correct(bool inner)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			var filter = NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>().IncludeErrorSet(errorSet);
			var canHandle = IsErrorCanBeHandledByNonEmptyCatchBlockFilter(filter, inner);
			Assert.That(canHandle, Is.True);
		}

		private bool IsErrorCanBeHandledByNonEmptyCatchBlockFilter(NonEmptyCatchBlockFilter filter, bool inner)
		{
			Exception errorToHandler;
			if (inner)
			{
				errorToHandler = new TestExceptionWithInnerArgumentNullException();
			}
			else
			{
				errorToHandler = new ArgumentException("TestName", inner.ToString());
			}
			return filter.ErrorFilter.GetCanHandle()(errorToHandler);
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(null, true)]
		public void Should_NonEmptyCatchBlockFilter_Created_From_ErrorSet_By_CreateByExcluding_Method_Correct(bool? inner, bool canBeHandled)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			var filter = NonEmptyCatchBlockFilter.CreateByExcluding(errorSet);
			Exception errorToHandler;
			if (canBeHandled)
			{
				errorToHandler = new InvalidOperationException();
				Assert.That(filter.ErrorFilter.GetCanHandle()(errorToHandler), Is.EqualTo(true));
			}
			else
			{
				var canHandle = IsErrorCanBeHandledByNonEmptyCatchBlockFilter(filter, inner.Value);
				Assert.That(canHandle, Is.EqualTo(false));
			}
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(null, true)]
		public void Should_NonEmptyCatchBlockFilter_With_ErrorSet_Added_By_ExcludeErrorSet_Correct(bool? inner, bool canBeHandled)
		{
			var errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			var filter = NonEmptyCatchBlockFilter.CreateByExcluding<InvalidOperationException>().ExcludeErrorSet(errorSet);
			Exception errorToHandler;
			if (canBeHandled)
			{
				errorToHandler = new NullReferenceException();
				Assert.That(filter.ErrorFilter.GetCanHandle()(errorToHandler), Is.EqualTo(true));
			}
			else
			{
				var canHandle = IsErrorCanBeHandledByNonEmptyCatchBlockFilter(filter, inner.Value);
				Assert.That(canHandle, Is.EqualTo(false));
			}
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
		public void Should_NonEmptyCatchBlockFilter_CreateByIncluding_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
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

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_NonEmptyCatchBlockFilter_CreateByExcluding_Works_Correctly_ForInnerError(bool errFilterUnsatisfied)
		{
			var filter = NonEmptyCatchBlockFilter.CreateByExcluding<ArgumentException>(CatchBlockFilter.ErrorType.InnerError);
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