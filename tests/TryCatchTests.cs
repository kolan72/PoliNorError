using NUnit.Framework;
using PoliNorError.TryCatch;
using System;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class TryCatchTests
	{
		[Test]
		[TestCase(false)]
		[TestCase(true)]
		public void Should_TryCatchResult_Initialized_From_PolicyResult_Correctly_When_Error_Or_Ok(bool isError)
		{
			var policyResult = new PolicyResult();
			if (isError)
			{
				policyResult.SetFailedWithError(new Exception());
			}
			else
			{
				policyResult.SetOk();
			}
			var tryCatchResult = new TryCatchResult(policyResult);
			if (isError)
			{
				Assert.That(tryCatchResult.Error, Is.Not.Null);
				Assert.That(tryCatchResult.IsError, Is.True);
			}
			else
			{
				Assert.That(tryCatchResult.Error, Is.Null);
				Assert.That(tryCatchResult.IsError, Is.False);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CatchBlockHandlerCollectionWrapper_Wrap_Correctly(bool handleByFirst)
		{
			var collection = new List<CatchBlockHandler>();
			var errorToThrow = new InvalidOperationException();
			int i = 0;
			int k = 0;
			var nofilterHandler = CatchBlockHandlerFactory
				.ForAllExceptions()
				.WithErrorProcessorOf((_) => i++);
			var filteredHandler = CatchBlockHandlerFactory
				.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>())
				.WithErrorProcessorOf((_) => k++);

			if (handleByFirst)
			{
				collection.Add(nofilterHandler);
				collection.Add(filteredHandler);
			}
			else
			{
				collection.Add(filteredHandler);
				collection.Add(nofilterHandler);
			}
			var sp = CatchBlockHandlerCollectionWrapper.Wrap(collection);
			var res = sp.Handle(() => throw errorToThrow);
			var error = res.GetErrorInWrappedResults();

			if (handleByFirst)
			{
				Assert.That(i, Is.EqualTo(1));
				Assert.That(k, Is.EqualTo(0));
			}
			else
			{
				Assert.That(i, Is.EqualTo(0));
				Assert.That(k, Is.EqualTo(1));
			}

			Assert.That(error, Is.EqualTo(errorToThrow));
		}
	}
}
