using NUnit.Framework;
using PoliNorError.TryCatch;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
			int catchBlockCount = 1;
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
						//Imitate an error in a wrapped policy.
						var polWrappedResult = new PolicyResult<int>().SetFailedWithError(new Exception());
						((PolicyResult<int>)policyResult).WrappedPolicyResults = new List<PolicyDelegateResult<int>>() { new PolicyDelegateResult<int>(polWrappedResult, "", null) };
					}
					else
					{
						//Imitate an error in a wrapped policy.
						var polWrappedResult = new PolicyResult().SetFailedWithError(new Exception());
						policyResult.WrappedPolicyResults = new List<PolicyDelegateResult>() { new PolicyDelegateResult(polWrappedResult, "", null) };
					}
					catchBlockCount = 2;
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

			TryCatchResultBase tryCatchResult;
			if (isGeneric == true)
			{
				tryCatchResult = new TryCatchResult<int>((PolicyResult<int>)policyResult, catchBlockCount);
			}
			else
			{
				tryCatchResult = new TryCatchResult(policyResult, catchBlockCount);
			}

			if (isError)
			{
				Assert.That(tryCatchResult.Error, Is.Not.Null);
				Assert.That(tryCatchResult.IsError, Is.True);
				Assert.That(tryCatchResult.IsSuccess, Is.False);
				if (isGeneric == true)
				{
					Assert.That(((TryCatchResult<int>)tryCatchResult).Result, Is.EqualTo(default(int)));
				}

				Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(0));
			}
			else
			{
				Assert.That(tryCatchResult.Error, Is.Null);
				Assert.That(tryCatchResult.IsError, Is.False);
				Assert.That(tryCatchResult.IsSuccess, Is.True);
				Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(-1));
				if (isGeneric == true)
				{
					Assert.That(((TryCatchResult<int>)tryCatchResult).Result, Is.EqualTo(1));
				}
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_Execute_Or_ExecuteAsync_Result_Has_IsError_False_When_NoError(bool isSync, bool withEmptyFilter)
		{
			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWhenNoError(withEmptyFilter);
			var tryCatchBuilder = tryCatchBuilderFactory.CreateBuilder();

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.HasCatchBlockForAll, Is.EqualTo(withEmptyFilter));

			TryCatchResult tryCatchResult = null;
			if (isSync)
			{
				tryCatchResult = tryCatch
										.Execute(() => { });
			}
			else
			{
				tryCatchResult = await tryCatch
										.ExecuteAsync(async (_) => await Task.Delay(1));
			}
			Assert.That(tryCatchResult.IsError, Is.False);
			Assert.That(tryCatchResult.IsSuccess, Is.True);
			Assert.That(tryCatchResult.Error, Is.Null);
			Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(-1));
			Assert.That(tryCatchBuilderFactory.I, Is.EqualTo(0));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_ExecuteT_Or_ExecuteAsyncT_Result_Has_IsError_False_When_NoError(bool isSync, bool withEmptyFilter)
		{
			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWhenNoError(withEmptyFilter);
			var tryCatchBuilder = tryCatchBuilderFactory.CreateBuilder();

			var tryCatch = tryCatchBuilder.Build();
			Assert.That(tryCatch.HasCatchBlockForAll, Is.EqualTo(withEmptyFilter));

			TryCatchResult<int> tryCatchResult = null;
			if (isSync)
			{
				tryCatchResult = tryCatch
										.Execute(() => 1);
			}
			else
			{
				tryCatchResult = await tryCatch
										.ExecuteAsync(async (_) => { await Task.Delay(1); return 1; });
			}
			Assert.That(tryCatchResult.IsError, Is.False);
			Assert.That(tryCatchResult.IsSuccess, Is.True);
			Assert.That(tryCatchResult.Error, Is.Null);
			Assert.That(tryCatchResult.Result, Is.EqualTo(1));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(-1));
			Assert.That(tryCatchBuilderFactory.I, Is.EqualTo(0));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Execute_Or_ExecuteAsync_WithEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool isSync)
		{
			int i = 0;
			var errorToThrow = new InvalidOperationException();
			var tryCatch = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory
											.ForAllExceptions()
											.WithErrorProcessorOf((_) => i++))
											.Build();

			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);

			TryCatchResult tryCatchResult = null;
			if (isSync)
			{
				tryCatchResult = tryCatch
											.Execute(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch
										.ExecuteAsync(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}
			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(0));
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ExecuteT_Or_ExecuteAsyncT_WithEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool isSync)
		{
			int i = 0;
			var errorToThrow = new InvalidOperationException();
			var tryCatch = TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory
											.ForAllExceptions()
											.WithErrorProcessorOf((_) => i++))
											.Build();

			Assert.That(tryCatch.HasCatchBlockForAll, Is.True);

			TryCatchResult<int> tryCatchResult = null;
			if (isSync)
			{
				tryCatchResult = tryCatch
											.Execute<int>(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch
										.ExecuteAsync<int>(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}
			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, Is.EqualTo(0));
			Assert.That(tryCatchResult.Result, Is.EqualTo(0));
			Assert.That(i, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_Execute_Or_ExecuteAsync_WithNonEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool handleByFirst, bool isSync)
		{
			var errorToThrow = new InvalidOperationException();

			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWithNonEmptyFilter(handleByFirst);
			var builder = tryCatchBuilderFactory.CreateBuilder();

			TryCatchResult tryCatchResult = null;
			var tryCatch = builder.Build();

			if (isSync)
			{
				tryCatchResult = tryCatch.Execute(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch.ExecuteAsync(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}

			if (handleByFirst)
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(1));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(0));
			}
			else
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(0));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(1));
			}

			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, handleByFirst ? Is.EqualTo(0) : Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(false, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public async Task Should_TryCatchForDI_Execute_Or_ExecuteAsync_WithNonEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool handleByFirst, bool isSync, bool initByOtherTryCatch)
		{
			var errorToThrow = new InvalidOperationException();
			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWithNonEmptyFilter(handleByFirst);

			TryCatchResult tryCatchResult = null;
			ITryCatch<TryCatchForDI> tryCatch = null;

			if (initByOtherTryCatch)
			{
				tryCatch = new TryCatchForDI(tryCatchBuilderFactory.CreateBuilder().Build());
			}
			else
			{
				tryCatch = new TryCatchForDI(tryCatchBuilderFactory);
			}

			if (isSync)
			{
				tryCatchResult = tryCatch.Execute(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch.ExecuteAsync(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}

			if (handleByFirst)
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(1));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(0));
			}
			else
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(0));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(1));
			}

			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, handleByFirst ? Is.EqualTo(0) : Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_ExecuteT_Or_ExecuteAsyncT_WithNonEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool handleByFirst, bool isSync)
		{
			var errorToThrow = new InvalidOperationException();

			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWithNonEmptyFilter(handleByFirst);
			var builder = tryCatchBuilderFactory.CreateBuilder();

			TryCatchResult<int> tryCatchResult = null;
			var tryCatch = builder.Build();

			if (isSync)
			{
				tryCatchResult = tryCatch.Execute<int>(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch.ExecuteAsync<int>(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}

			if (handleByFirst)
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(1));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(0));
			}
			else
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(0));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(1));
			}

			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, handleByFirst ? Is.EqualTo(0) : Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true, true)]
		[TestCase(false, true, true)]
		[TestCase(true, false, true)]
		[TestCase(false, false, true)]
		[TestCase(true, true, false)]
		[TestCase(false, true, false)]
		[TestCase(true, false, false)]
		[TestCase(false, false, false)]
		public async Task Should_TryCatchForDI_ExecuteT_Or_ExecuteAsyncT_WithNonEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool handleByFirst, bool isSync, bool initByOtherTryCatch)
		{
			var errorToThrow = new InvalidOperationException();

			TryCatchResult<int> tryCatchResult = null;

			var tryCatchBuilderFactory = new TryCatchBuilderFactoryWithNonEmptyFilter(handleByFirst);
			ITryCatch<TryCatchForDI> tryCatch = null;

			if (initByOtherTryCatch)
			{
				tryCatch = new TryCatchForDI(tryCatchBuilderFactory.CreateBuilder().Build());
			}
			else
			{
				tryCatch = new TryCatchForDI(tryCatchBuilderFactory);
			}

			if (isSync)
			{
				tryCatchResult = tryCatch.Execute<int>(() => throw errorToThrow);
			}
			else
			{
				tryCatchResult = await tryCatch.ExecuteAsync<int>(async (_) => { await Task.Delay(1); throw errorToThrow; });
			}

			if (handleByFirst)
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(1));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(0));
			}
			else
			{
				Assert.That(tryCatchBuilderFactory.FirstProcFlag, Is.EqualTo(0));
				Assert.That(tryCatchBuilderFactory.SecProcFlag, Is.EqualTo(1));
			}

			Assert.That(tryCatchResult.IsError, Is.True);
			Assert.That(tryCatchResult.IsSuccess, Is.False);
			Assert.That(tryCatchResult.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResult.ExceptionHandlerIndex, handleByFirst ? Is.EqualTo(0) : Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Execute_Or_ExecuteAsync_Throw_If_NoCatchBlockHandler_With_MatchedException_Provided(bool isSync, bool isGeneric)
		{
			var errorToThrow = new NullReferenceException();
			var filteredHandler = CatchBlockHandlerFactory
									.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

			var tryCatch = TryCatchBuilder
							.CreateFrom(filteredHandler)
							.Build();

			Exception resException = null;
			if (isSync)
			{
				if (isGeneric)
				{
					resException = Assert.Throws<NullReferenceException>(() => tryCatch.Execute<int>(() => throw errorToThrow));
				}
				else
				{
					resException = Assert.Throws<NullReferenceException>(() => tryCatch.Execute(() => throw errorToThrow));
				}
			}
			else
			{
				if (isGeneric)
				{
					resException = Assert.ThrowsAsync<NullReferenceException>(async () => await tryCatch.ExecuteAsync<int>(async (_) => { await Task.Delay(1); throw errorToThrow; }));
				}
				else
				{
					resException = Assert.ThrowsAsync<NullReferenceException>(async () => await tryCatch.ExecuteAsync(async (_) => { await Task.Delay(1); throw errorToThrow; }));
				}
			}
			Assert.That(resException, Is.EqualTo(errorToThrow));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_CatchBlockHandlerCollectionWrapper_Wrap_Correctly_If_ForAllHandler_Added(bool handleByFirst)
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
			(Exception ex, int index) = res.GetErrorInWrappedResults(collection.Count - 1);

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
			Assert.That(index, Is.EqualTo(0));
			Assert.That(ex, Is.EqualTo(errorToThrow));
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_PolicyResult_GetErrorInWrappedResults_Returns_Correct_ExceptionHandlerIndex(bool handleByLast)
		{
			var collection = new List<CatchBlockHandler>();
			var errorToThrow = new InvalidOperationException();

			var nullRefHandler = CatchBlockHandlerFactory
				.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());

			var invalidOperHandler = CatchBlockHandlerFactory
				.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

			if (handleByLast)
			{
				collection.Add(nullRefHandler);
				collection.Add(invalidOperHandler);
			}
			else
			{
				collection.Add(nullRefHandler);
				collection.Add(invalidOperHandler);
				collection.Add(nullRefHandler);
			}
			var sp = CatchBlockHandlerCollectionWrapper.Wrap(collection);
			var res = sp.Handle(() => throw errorToThrow);
			(Exception ex, int index) = res.GetErrorInWrappedResults(collection.Count - 1);
			Assert.That(index, Is.EqualTo(1));
			Assert.That(ex, Is.EqualTo(errorToThrow));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public async Task Should_Execute_Or_ExecuteAsync_With_MoreThanTwo_NonEmptyFilteredCatchBlockHandler_Returns_TryCatchResult_With_Error(bool isSync, bool isGeneric)
		{
			var errorToThrow = new NullReferenceException();

			var firstErrorHandler = CatchBlockHandlerFactory
						.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());

			var secondErrorHandler = CatchBlockHandlerFactory
									.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<OperationCanceledException>());

			var thirdErrorHandler = CatchBlockHandlerFactory
						.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

			var tryCatch = TryCatchBuilder
						.CreateFrom(firstErrorHandler)
						.AddCatchBlock(secondErrorHandler)
						.AddCatchBlock(CatchBlockHandlerFactory
									.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<ArgumentException>()))
						.AddCatchBlock(thirdErrorHandler)
						.Build();

			TryCatchResultBase tryCatchResultBase = null;
			if (isSync)
			{
				if (isGeneric)
				{
					tryCatchResultBase = tryCatch.Execute<int>(() => throw errorToThrow);
				}
				else
				{
					tryCatchResultBase = tryCatch.Execute(() => throw errorToThrow);
				}
			}
			else
			{
				if (isGeneric)
				{
					tryCatchResultBase = await tryCatch.ExecuteAsync<int>((_) => throw errorToThrow);
				}
				else
				{
					tryCatchResultBase = await tryCatch.ExecuteAsync((_) => throw errorToThrow);
				}
			}
			Assert.That(tryCatchResultBase.IsError, Is.True);
			Assert.That(tryCatchResultBase.Error, Is.EqualTo(errorToThrow));
			Assert.That(tryCatchResultBase.ExceptionHandlerIndex, Is.EqualTo(0));
		}

		[Test]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(true, true)]
		[TestCase(false, true)]
		public async Task Should_TryCatchResult_IsCanceled_Equals_True_When_Canceled(bool isSync, bool isGeneric)
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				var errorToThrow = new InvalidOperationException();
				var tryCatch = NonEmptyCatchBlockFilter
								.CreateByIncluding<InvalidOperationException>()
								.ToCatchBlockHandler()
								.ToTryCatch();

				TryCatchResultBase tryCatchResultBase = null;

				if (isSync)
				{
					if (isGeneric)
					{
						tryCatchResultBase = tryCatch.Execute<int>(() => { cancelTokenSource.Cancel(); throw errorToThrow; }, cancelTokenSource.Token);
					}
					else
					{
						tryCatchResultBase = tryCatch.Execute(() => { cancelTokenSource.Cancel(); throw errorToThrow; }, cancelTokenSource.Token);
					}
				}
				else
				{
					if (isGeneric)
					{
						tryCatchResultBase = await tryCatch.ExecuteAsync<int>((_) => { cancelTokenSource.Cancel(); throw errorToThrow; }, cancelTokenSource.Token);
					}
					else
					{
						tryCatchResultBase = await tryCatch.ExecuteAsync((_) => { cancelTokenSource.Cancel(); throw errorToThrow; }, cancelTokenSource.Token);
					}
				}
				Assert.That(tryCatchResultBase.IsCanceled, Is.True);
				Assert.That(tryCatchResultBase.IsSuccess, Is.False);
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_InvokeWithTryCatch_For_Action_Handle_Or_Throw_Exception_Correctly(bool canHandle)
		{
			Exception errorToThrow = null;
			if (canHandle)
			{
				errorToThrow = new NullReferenceException();
			}
			else
			{
				errorToThrow = new NotImplementedException();
			}

			var tryCatchFactory = new TryCatchBuilderFactoryForDelegateInvocationWithTryCatch();
			var tryCatch = tryCatchFactory.CreateTryCatch();

			Action action = () => throw errorToThrow;

			if (canHandle)
			{
				var result = action.InvokeWithTryCatch(tryCatch);
				Assert.That(result.IsError, Is.True);
				Assert.That(result.Error, Is.EqualTo(errorToThrow));
				Assert.That(tryCatchFactory.IsErrorProcessorCalled, Is.True);
			}
			else
			{
				var resException = Assert.Throws<NotImplementedException>(() => action.InvokeWithTryCatch(tryCatch));
				Assert.That(resException, Is.EqualTo(errorToThrow));
			}
		}

		[Test]
		public void Should_InvokeWithTryCatch_For_Action_Returns_Success_If_NoError()
		{
			var tryCatchFactory = new TryCatchBuilderFactoryForDelegateInvocationWithTryCatch();
			var tryCatch = tryCatchFactory.CreateTryCatch();

			Action action = () => {};

			var result = action.InvokeWithTryCatch(tryCatch);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(tryCatchFactory.IsErrorProcessorCalled, Is.False);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_InvokeWithTryCatch_For_Func_Handle_Or_Throw_Exception_Correctly(bool canHandle)
		{
			Exception errorToThrow = null;
			if (canHandle)
			{
				errorToThrow = new NullReferenceException();
			}
			else
			{
				errorToThrow = new NotImplementedException();
			}

			var tryCatchFactory = new TryCatchBuilderFactoryForDelegateInvocationWithTryCatch();
			var tryCatch = tryCatchFactory.CreateTryCatch();

			Func<int> action = () => throw errorToThrow;

			if (canHandle)
			{
				var result = action.InvokeWithTryCatch(tryCatch);
				Assert.That(result.IsError, Is.True);
				Assert.That(result.Error, Is.EqualTo(errorToThrow));
				Assert.That(tryCatchFactory.IsErrorProcessorCalled, Is.True);
			}
			else
			{
				var resException = Assert.Throws<NotImplementedException>(() => action.InvokeWithTryCatch(tryCatch));
				Assert.That(resException, Is.EqualTo(errorToThrow));
			}
		}

		[Test]
		public void Should_InvokeWithTryCatch_For_Func_Returns_Success_If_NoError()
		{
			var tryCatchFactory = new TryCatchBuilderFactoryForDelegateInvocationWithTryCatch();
			var tryCatch = tryCatchFactory.CreateTryCatch();

			Func<int> action = () => 1;

			var result = action.InvokeWithTryCatch(tryCatch);
			Assert.That(result.IsSuccess, Is.True);
			Assert.That(tryCatchFactory.IsErrorProcessorCalled, Is.False);
		}

		private class TryCatchBuilderFactoryForDelegateInvocationWithTryCatch
		{
			private readonly CatchBlockFilteredHandler _catchBlockFilteredHandler;

			public TryCatchBuilderFactoryForDelegateInvocationWithTryCatch()
			{
				_catchBlockFilteredHandler = CatchBlockHandlerFactory
										.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());

				_catchBlockFilteredHandler.WithErrorProcessorOf((_) => IsErrorProcessorCalled = true);
			}

			public ITryCatch CreateTryCatch()
			{
				return TryCatchBuilder
				.CreateFrom(_catchBlockFilteredHandler)
				.Build();
			}

			public bool IsErrorProcessorCalled { get; private set; }
		}

		private class TryCatchBuilderFactoryWhenNoError
		{
			private readonly bool _withEmptyFilter;
			private int _i;

			public TryCatchBuilderFactoryWhenNoError(bool withEmptyFilter)
			{
				_withEmptyFilter = withEmptyFilter;
			}

			public ITryCatchBuilder CreateBuilder()
			{
				return _withEmptyFilter
					? TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory
                                                                .ForAllExceptions()
                                                                .WithErrorProcessorOf((_) => _i++))
					: TryCatchBuilder.CreateFrom(CatchBlockHandlerFactory
                                                                .FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>())
                                                                .WithErrorProcessorOf((_) => _i++));
			}

			public int I => _i;
		}

		private class TryCatchBuilderFactoryWithNonEmptyFilter
		{
			private int _firstProcFlag;
			private int _secProcFlag;

			private readonly bool _handleByFirst;

			public TryCatchBuilderFactoryWithNonEmptyFilter(bool handleByFirst)
			{
				_handleByFirst = handleByFirst;
			}

			public ITryCatchBuilder CreateBuilder()
			{
				var testErrorHandler = CatchBlockHandlerFactory
										.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<InvalidOperationException>());

				var otherErrorHandler = CatchBlockHandlerFactory
										.FilterExceptionsBy(NonEmptyCatchBlockFilter.CreateByIncluding<NullReferenceException>());

				return _handleByFirst
					? TryCatchBuilder
                        .CreateFrom(testErrorHandler.WithErrorProcessorOf((_) => _firstProcFlag++))
                        .AddCatchBlock(otherErrorHandler.WithErrorProcessorOf((_) => _secProcFlag++))
					: TryCatchBuilder
                        .CreateFrom(otherErrorHandler.WithErrorProcessorOf((_) => _firstProcFlag++))
                        .AddCatchBlock(testErrorHandler.WithErrorProcessorOf((_) => _secProcFlag++));
			}

			public int FirstProcFlag => _firstProcFlag;
			public int SecProcFlag => _secProcFlag;
		}

		private class TryCatchForDI : TryCatchBase, ITryCatch<TryCatchForDI>
		{
			public TryCatchForDI(ITryCatch tryCatch) : base(tryCatch){}

			public TryCatchForDI(TryCatchBuilderFactoryWithNonEmptyFilter factory)
			{
				TryCatch = factory.CreateBuilder().Build();
			}
		}
	}
}
