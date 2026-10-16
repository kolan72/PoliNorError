using NUnit.Framework;
using NUnit.Framework.Legacy;
using PoliNorError.TryCatch;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNorError.Extensions.PolicyErrorFiltering;

namespace PoliNorError.Tests
{
	internal class CatchBlockHandlersTests
	{
		[Test]
		[TestCase(PolicyAlias.Simple, true)]
		[TestCase(PolicyAlias.Simple, false)]
		[TestCase(PolicyAlias.Retry, true)]
		[TestCase(PolicyAlias.Retry, false)]
		public void Should_Exception_In_ErrorFilters_Be_Stored_In_CatchBlockErrors(PolicyAlias policyAlias, bool include)
		{
			IPolicyBase policy = null;
			switch (policyAlias)
			{
				case PolicyAlias.Simple:
					policy = new SimplePolicy();
					break;
				case PolicyAlias.Retry:
					policy = new RetryPolicy(10);
					break;
			}

			bool testFn(Exception _) => throw new Exception("Error in filter");
			if (include)
			{
				((Policy)policy).IncludeError((Func<Exception, bool>)testFn);
			}
			else
			{
				((Policy)policy).ExcludeError((Func<Exception, bool>)testFn);
			}
			void action() => throw new Exception("Test");
			var res = policy.Handle(action);
			ClassicAssert.NotNull(res.UnprocessedError);
			ClassicAssert.AreEqual(1, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(CatchBlockExceptionSource.ErrorFilter, res.CatchBlockErrors.FirstOrDefault().ExceptionSource);
			Assert.That(res.Errors.Count(), Is.EqualTo(1));
			Assert.That(res.IsFailed, Is.EqualTo(true));
			Assert.That(res.CatchBlockErrors.FirstOrDefault().IsCritical, Is.EqualTo(true));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CatchBlockHandler_InitByFilter_Correctly(bool forAll)
		{
			CatchBlockHandler handler;
			Exception errorToHandler;
			if (forAll)
			{
				handler = CatchBlockHandlerFactory.ForAllExceptions();
				errorToHandler = TestExceptionHolder.TestException;
			}
			else
			{
				handler = CatchBlockHandlerFactory.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());
				errorToHandler = new NullReferenceException();
			}
			var result = handler.ErrorFilter.GetCanHandle()(errorToHandler);
			Assert.That(result, Is.True);
		}

		[Test]
		public void Should_CatchBlockHandler_Initialized_Correctly_By_Including_ErrorSet()
		{
			var errorSet = ErrorSet.FromError<NullReferenceException>();
			var handler = CatchBlockHandlerFactory.FilterExceptionsByIncluding(errorSet);
			var result = handler.ErrorFilter.GetCanHandle()(new NullReferenceException());
			Assert.That(result, Is.True);
		}

		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_CatchBlockHandler_Initialized_Correctly_By_Excluding_ErrorSet(bool canHandle)
		{
			var errorSet = ErrorSet.FromError<NullReferenceException>();
			var handler = CatchBlockHandlerFactory.FilterExceptionsByExcluding(errorSet);
			bool result;
			if (canHandle)
			{
				result = handler.ErrorFilter.GetCanHandle()(new InvalidOperationException());
			}
			else
			{
				result = handler.ErrorFilter.GetCanHandle()(new NullReferenceException());
			}
			Assert.That(result, Is.EqualTo(canHandle));
		}

		[Test]
		public void Should_NonEmptyCatchBlockFilter_ToCatchBlockHandler_Method_Create_Handler_Correctly()
		{
			var handler = NonEmptyCatchBlockFilter
							.CreateByIncluding<NullReferenceException>()
							.ToCatchBlockHandler();
			Assert.That(handler, Is.Not.Null);
			Assert.That(handler.CatchBlockFilter, Is.Not.Null);
		}

		[Test]
		public void Should_CatchBlockForAllHandler_ToTryCatch_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var tryCatch = CatchBlockHandlerFactory.ForAllExceptions()
							.ToTryCatch();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);
		}

		[Test]
		public void Should_CatchBlockFilteredHandler_ToTryCatch_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var tryCatch = NonEmptyCatchBlockFilter
						.CreateByIncluding<NullReferenceException>()
						.ToCatchBlockHandler()
						.ToTryCatch();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
		}

		[Test]
		public void Should_CatchBlockFilteredHandler_ToTryCatchBuilder_Create_ITryCatch_WithOneCatchBlockHandler()
		{
			var builder = NonEmptyCatchBlockFilter
						.CreateByIncluding<NullReferenceException>()
						.ToCatchBlockHandler()
						.ToTryCatchBuilder();
			Assert.That(builder, Is.TypeOf<TryCatchBuilder>());
			var tryCatch = builder.Build();
			Assert.That(tryCatch.CatchBlockCount, Is.EqualTo(1));
		}
	}
}
