using NUnit.Framework;
using PoliNorError.TryCatch;
using System;
using System.Collections.Generic;

namespace PoliNorError.Tests
{
	internal class TryCatchTests
	{
		[Test]
		[TestCase(false, null, null)]
		[TestCase(true, true, true)]
		[TestCase(true, true, false)]
		[TestCase(true, false, null)]
		public void Should_TryCatchResult_Initialized_From_PolicyResult_Correctly_When_Error_Or_Ok(bool isError, bool? isWrapped, bool? isGeneric)
		{
			PolicyResult policyResult;
			if (isGeneric == true)
			{
				policyResult = new PolicyResult<int>();
			}
			else
			{
				policyResult = new PolicyResult();
			}

			if (isError)
			{
				if (isWrapped == true)
				{
					policyResult.SetOk();
					if (isGeneric == true)
					{
						//Strengthen - even if PolicyResult<T>.Result is set, TryCatchResult.Result should be equal to default.
						((PolicyResult<int>)policyResult).SetResult(1);
					}
					//Imitate an error in a wrapped policy.
					var polWrappedResult = new PolicyResult().SetFailedWithError(new Exception());
					policyResult.WrappedPolicyResults = new List<PolicyDelegateResult>() { new PolicyDelegateResult(polWrappedResult, "", null) };
				}
				else
				{
					policyResult.SetFailedWithError(new Exception());
				}
			}
			else
			{
				policyResult.SetOk();
				if (isGeneric == true)
				{
					((PolicyResult<int>)policyResult).SetResult(1);
				}
			}

			TryCatchResult tryCatchResult;
			if (isGeneric == true)
			{
				tryCatchResult = new TryCatchResult<int>((PolicyResult<int>)policyResult);
			}
			else
			{
				tryCatchResult = new TryCatchResult(policyResult);
			}

			if (isError)
			{
				Assert.That(tryCatchResult.Error, Is.Not.Null);
				Assert.That(tryCatchResult.IsError, Is.True);
				if (isGeneric == true)
				{
					Assert.That(((TryCatchResult<int>)tryCatchResult).Result, Is.EqualTo(default(int)));
				}
			}
			else
			{
				Assert.That(tryCatchResult.Error, Is.Null);
				Assert.That(tryCatchResult.IsError, Is.False);
				if (isGeneric == true)
				{
					Assert.That(((TryCatchResult<int>)tryCatchResult).Result, Is.EqualTo(1));
				}
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
