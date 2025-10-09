using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class SimplePolicyProcessorTests
	{
		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public void Should_CatchBlockError_Handled_For_Handle(bool throwError, int CatchBlockErrorsCount, int k)
		{
			void save() => throw new Exception();

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = retryPolTest.Execute(save);
			ClassicAssert.IsTrue(res.Errors.Count() == 1);

			ClassicAssert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(k, i);
		}

		[Test]
		public void Should_Handle_If_NoError_Work()
		{
			void save() => Expression.Empty();
			var retryPolTest = new SimplePolicyProcessor();
			var res = retryPolTest.Execute(save);
			ClassicAssert.IsTrue(res.NoError);
		}

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public void Should_CatchBlockError_Handled_For_HandleT(bool throwError, int CatchBlockErrorsCount, int k)
		{
			int save() => throw new Exception();

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);

			var res = retryPolTest.Execute(save);
			ClassicAssert.IsTrue(res.Errors.Count() == 1);

			ClassicAssert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(k, i);
		}

		[Test]
		public void Should_HandleT_If_NoError_Work()
		{
			int save() => 1;
			var retryPolTest = new SimplePolicyProcessor();
			var res = retryPolTest.Execute(save);
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(1, res.Result);
		}

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public async Task Should_CatchBlockError_Handled_For_AsyncHandle(bool throwError, int CatchBlockErrorsCount, int k)
		{
			async Task saveAsync(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			ClassicAssert.IsTrue(res.Errors.Count() == 1);

			ClassicAssert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(k, i);
		}

		[Test]
		public async Task Should_HandleAsync_If_NoError_Work()
		{
			async Task saveAsync(CancellationToken _) => await Task.Delay(1);
			var retryPolTest = new SimplePolicyProcessor();
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			ClassicAssert.IsTrue(res.NoError);
		}

		[Test]
		[TestCase(true, 1, 0)]
		[TestCase(false, 0, 1)]
		public async Task Should_CatchBlockError_Handled_For_AsyncHandleT(bool throwError, int CatchBlockErrorsCount, int k)
		{
			async Task<int> saveAsync(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

			int i = 0;
			void errorProcessorFunc(Exception ex) { if (throwError) throw ex; else ++i; }
			var retryPolTest = new SimplePolicyProcessor().WithErrorProcessorOf(errorProcessorFunc);
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			ClassicAssert.IsTrue(res.Errors.Count() == 1);

			ClassicAssert.AreEqual(CatchBlockErrorsCount, res.CatchBlockErrors.Count());
			ClassicAssert.AreEqual(k, i);
		}

		[Test]
		public async Task Should_HandleTAsync_If_NoError_Work()
		{
			async Task<int> saveAsync(CancellationToken _) { await Task.Delay(1); return 1; }
			var retryPolTest = new SimplePolicyProcessor();
			var res = await retryPolTest.ExecuteAsync(saveAsync);
			ClassicAssert.IsTrue(res.NoError);
			ClassicAssert.AreEqual(1, res.Result);
		}

		[Test]
		public void Should_Execute_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				void save() => throw new ApplicationException();
				var processor = SimplePolicyProcessor.CreateDefault();
				var tryResCount = processor.Execute(save, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public void Should_ExecuteT_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				int save() => throw new ApplicationException();
				var processor = SimplePolicyProcessor.CreateDefault();
				var tryResCount = processor.Execute(save, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public void Should_Execute_WithContext_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				void save() => throw new ApplicationException();
				var processor = new SimplePolicyProcessor();
				var tryResCount = processor.Execute(save, 1, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public void Should_ExecuteT_WithContext_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

#pragma warning disable RCS1163 // Unused parameter.
				int save(int i) => throw new ApplicationException();
#pragma warning restore RCS1163 // Unused parameter.
				var processor = new SimplePolicyProcessor();
				var tryResCount = processor.Execute(save, 1, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public void Should_Execute_With_Param_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

#pragma warning disable RCS1163 // Unused parameter.
				void save(int i) => throw new ApplicationException();
#pragma warning restore RCS1163 // Unused parameter.
				var processor = new SimplePolicyProcessor();
				var tryResCount = processor.Execute(save, 1, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);
			}
		}

		[Test]
		public async Task Should_ExecuteAsync_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

				async Task save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
				const int i = 0;
				var processor = SimplePolicyProcessor.CreateDefault();
				var tryResCount = await processor.ExecuteAsync(save, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);

				Assert.That(i, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_ExecuteAsync_With_Param_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(int i, CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => k = i, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new SimplePolicyProcessor();
				var result = await processor.ExecuteAsync(f, 1, token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_ExecuteAsync_With_Context_With_ConfigAwait_False__BeCancelable()
		{
			using (var cts = new CancellationTokenSource())
			{
				cts.Cancel();
				var token = cts.Token;
				var k = 0;
				Task f(CancellationToken ct)
				{
					return Task.Run(() => Task.Run(() => k = 1, token).GetAwaiter().GetResult(), ct);
				}
				var processor = new SimplePolicyProcessor();
				var result = await processor.ExecuteAsync(f, 1, token).ConfigureAwait(false);

				Assert.That(result.IsCanceled, Is.True);
				Assert.That(result.IsSuccess, Is.False);

				Assert.That(result.NoError, Is.True);

				Assert.That(k, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_ExecuteAsyncT_BeCancelable()
		{
			var cancelTokenSource = new CancellationTokenSource();
			cancelTokenSource.Cancel();

			async Task<int> save(CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
			const int i = 0;
			var processor = SimplePolicyProcessor.CreateDefault();
			var tryResCount = await processor.ExecuteAsync(save, cancelTokenSource.Token);

			ClassicAssert.AreEqual(true, tryResCount.IsCanceled);
			ClassicAssert.AreEqual(0, i);
			cancelTokenSource.Dispose();
		}

		[Test]
		public async Task Should_ExecuteAsync_WithParam_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

#pragma warning disable RCS1163 // Unused parameter.
				async Task save(int k, CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
#pragma warning restore RCS1163 // Unused parameter.
				const int i = 0;
				var processor = new SimplePolicyProcessor();
				var tryResCount = await processor.ExecuteAsync(save, 1, false, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);

				Assert.That(i, Is.EqualTo(0));
			}
		}

		[Test]
		public async Task Should_ExecuteAsyncT_WithParam_BeCancelable()
		{
			using (var cancelTokenSource = new CancellationTokenSource())
			{
				cancelTokenSource.Cancel();

#pragma warning disable RCS1163 // Unused parameter.
				async Task<int> save(int k, CancellationToken _) { await Task.Delay(1); throw new ApplicationException(); }
#pragma warning restore RCS1163 // Unused parameter.
				const int i = 0;
				var processor = new SimplePolicyProcessor();
				var tryResCount = await processor.ExecuteAsync(save, 1, false, cancelTokenSource.Token);

				Assert.That(tryResCount.IsCanceled, Is.True);
				Assert.That(tryResCount.IsSuccess, Is.False);

				Assert.That(tryResCount.NoError, Is.True);

				Assert.That(i, Is.EqualTo(0));
			}
		}

		[Test]
		public void Should_PolicyResult_Be_Failed_And_Canceled_If_Canceled_During_Error_Processors_Run()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var processor = SimplePolicyProcessor.CreateDefault();
			var res =  processor.WithErrorProcessorOf((Exception _, CancellationToken __) => cancelTokenSource.Cancel())
					 .Execute(() => throw new Exception(), cancelTokenSource.Token);
			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.IsTrue(res.IsCanceled);
			ClassicAssert.IsFalse(res.IsSuccess);
			cancelTokenSource.Dispose();
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_Generic_IncludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_With_TwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.IncludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			processor.IncludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false, true)]
		[TestCase(TestErrorSetMatch.NoMatch, true, false)]
		[TestCase(TestErrorSetMatch.FirstParam, false, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false, false)]
		public void Should_IncludeErrorSet_With_IErrorSetParam_ForInnerExceptions_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, bool consistsOfErrorAndInnerError)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			processor.IncludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_IncludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault().IncludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

	    [Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_Generic_ExcludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_ExcludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = SimplePolicyProcessor.CreateDefault().ExcludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Execute(saveWithInclude);
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_With_TwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			processor.ExcludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true, true)]
		[TestCase(TestErrorSetMatch.NoMatch, false, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true, false)]
		[TestCase(TestErrorSetMatch.SecondParam, true, false)]
		public void Should_ExcludeErrorSet_With_IErrorSetParam_ForInnerExceptions_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, bool consistsOfErrorAndInnerError)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			ErrorSet errorSet;
			if (consistsOfErrorAndInnerError)
			{
				errorSet = ErrorSet.FromError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			else
			{
				errorSet = ErrorSet.FromInnerError<ArgumentException>().WithInnerError<ArgumentNullException>();
			}
			processor.ExcludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, null, true));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_With_IErrorSetParam_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			var errorSet = ErrorSet.FromError<ArgumentException>().WithError<ArgumentNullException>();
			processor.ExcludeErrorSet(errorSet);

			var tryResCountWithNoInclude = processor.Execute(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName));
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_Execute_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = proc.Execute(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_ExecuteT_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = proc.Execute<int>(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_ExecuteAsync_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = await proc.ExecuteAsync(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public async Task Should_PolicyResult_Contains_NoDelegateException_When_ExecuteTAsync_Null_Delegate()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			var simpleResult = await proc.ExecuteAsync<int>(null);
			ClassicAssert.IsTrue(simpleResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, simpleResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), simpleResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		[TestCase(false, true)]
		public async Task Should_WithInnerErrorProcessor_HandleError_Correctly(bool sync, bool withCancellationType)
		{
			var processor = SimplePolicyProcessor.CreateDefault();
			var innerProcessors = new InnerErrorProcessorFuncs();

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.Action);
				}

				processor.Execute(ActionWithInner);
				processor.Execute(Action);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFunc);
				}

				await processor.ExecuteAsync(AsyncFuncWithInner);
				await processor.ExecuteAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.I, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithToken);
				processor.Execute(ActionWithInner);
				processor.Execute(Action);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithToken);
				await processor.ExecuteAsync(AsyncFuncWithInner);
				await processor.ExecuteAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.J, Is.EqualTo(1));

			if (sync)
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfo);
				}
				processor.Execute(ActionWithInner);
				processor.Execute(Action);
			}
			else
			{
				if (withCancellationType)
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo, CancellationType.Precancelable);
				}
				else
				{
					processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfo);
				}
				await processor.ExecuteAsync(AsyncFuncWithInner);
				await processor.ExecuteAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.K, Is.EqualTo(1));

			if (sync)
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.ActionWithErrorInfoWithToken);
				processor.Execute(ActionWithInner);
				processor.Execute(Action);
			}
			else
			{
				processor.WithInnerErrorProcessorOf<TestInnerException>(innerProcessors.AsyncFuncWithErrorInfoWithToken);
				await processor.ExecuteAsync(AsyncFuncWithInner);
				await processor.ExecuteAsync(AsyncFunc);
			}

			Assert.That(innerProcessors.L, Is.EqualTo(1));
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_IncludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var processor = SimplePolicyProcessor
							.CreateDefault();
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				processor = processor.IncludeInnerError<TestInnerException>();
			}
			else
			{
				processor = processor.IncludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			PolicyResult result;
			if (withInnerError)
			{
				if (satisfyFilterFunc == true)
				{
					result = processor.Execute(((Action<string>)ActionWithInnerWithMsg).Apply("Test"));
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Execute(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"));
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else
				{
					result = processor.Execute(ActionWithInner);
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
			}
			else
			{
				result = processor.Execute(Action);
				Assert.That(result.ErrorFilterUnsatisfied, Is.True);
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_ExcludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var processor = SimplePolicyProcessor
							.CreateDefault();
			if ((withInnerError && !satisfyFilterFunc.HasValue) || !withInnerError)
			{
				processor = processor.ExcludeInnerError<TestInnerException>();
			}
			else
			{
				processor = processor.ExcludeInnerError<TestInnerException>(ex => ex.Message == "Test");
			}

			PolicyResult result;
			if (withInnerError)
			{
				if (satisfyFilterFunc == true)
				{
					result = processor.Execute(((Action<string>)ActionWithInnerWithMsg).Apply("Test"));
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Execute(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"));
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else
				{
					result = processor.Execute(ActionWithInner);
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
			}
			else
			{
				result = processor.Execute(Action);
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		public void Should_WithErrorProcessor_AddErrorProcessorInCollection()
		{
			var proc = SimplePolicyProcessor.CreateDefault();
			proc.AddErrorProcessor(new TestErrorProcessor());
			ClassicAssert.IsTrue(proc.Count() == 1);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Rethrow_Or_Handle_If_ProcessorCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForExecute(bool errorInFilter)
		{
			if(errorInFilter)
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = proc.Execute(ActionWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
				Assert.That(res.CatchBlockErrors.FirstOrDefault().ExceptionSource, Is.EqualTo(CatchBlockExceptionSource.ErrorFilter));
			}
			else
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>();
				Assert.Throws<TestExceptionWithInnerException>(() => proc.Execute(ActionWithInner));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Rethrow_Or_Handle_If_ProcessorCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForExecuteAsync(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = await proc.ExecuteAsync(AsyncFuncWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>();
				Assert.ThrowsAsync<TestExceptionWithInnerException>(async() => await proc.ExecuteAsync(AsyncFuncWithInner));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Rethrow_Or_Handle_If_ProcessorCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForExecuteT(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = proc.Execute(FuncWithInner);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>();
				Assert.Throws<TestExceptionWithInnerException>(() => proc.Execute(FuncWithInner));
			}
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_Rethrow_Or_Handle_If_ProcessorCreated_With_ThrowIfErrorFilterUnsatisfied_True_ForExecuteAsyncT(bool errorInFilter)
		{
			if (errorInFilter)
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>((_) => throw new Exception("Test"));
				var res = await proc.ExecuteAsync(AsyncFuncWithInnerT);
				Assert.That(res.CatchBlockErrors.Count(), Is.EqualTo(1));
				Assert.That(res.Errors.Count(), Is.EqualTo(1));
			}
			else
			{
				var proc = new SimplePolicyProcessor(true).ExcludeError<TestExceptionWithInnerException>();
				Assert.ThrowsAsync<TestExceptionWithInnerException>(async () => await proc.ExecuteAsync(AsyncFuncWithInnerT));
			}
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Execute_For_Action_With_Generic_Param_WithErrorProcessorOf_Action_Process_Correctly(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			SimplePolicyProcessor processor;

			if (!withCancellationType)
			{
				processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);
			}
			else
			{
				processor = new SimplePolicyProcessor(true)
						.WithErrorContextProcessorOf<int>(action, CancellationType.Precancelable);
			}

			PolicyResult result = null;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}
			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(false, true)]
		[TestCase(true, false)]
		[TestCase(false, false)]
		public void Should_Execute_For_Action_With_Generic_Param_WithErrorProcessorOf_AsyncFunc_Process_Correctly(bool shouldWork, bool withCancellationType)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			SimplePolicyProcessor processor;

			if (!withCancellationType)
			{
				processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(fn);
			}
			else
			{
				processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(fn, CancellationType.Precancelable);
			}

			PolicyResult result = null;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Execute_For_Action_With_Generic_Param_WithErrorProcessorOf_AsyncFunc_With_Token_Param_Process_Correctly(bool shouldWork)
		{
			int m = 0;

			async Task fn(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				await Task.Delay(1);
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
						.WithErrorContextProcessorOf<int>(fn);

			PolicyResult result;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Execute_For_Action_With_Generic_Param_WithErrorProcessorOf_Action_With_Token_Param_Process_Correctly(bool shouldWork)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi, CancellationToken __)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
						.WithErrorContextProcessorOf<int>(action);

			PolicyResult result;

			if (shouldWork)
			{
				result = processor.Execute(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
			}
			else
			{
				result = processor.Execute(() => throw new InvalidOperationException());
				Assert.That(m, Is.EqualTo(0));
			}

			Assert.That(result.NoError, Is.False);
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Execute_With_TParam_For_Action_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = processor.Execute((_) => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
#pragma warning disable RCS1021 // Convert lambda expression body to expression-body.
				result = processor.Execute((v) => { addable += v; }, 5);
#pragma warning restore RCS1021 // Convert lambda expression body to expression-body.
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Execute_With_TParam_For_Func_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = processor.Execute<int, int>(() => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = processor.Execute(() => 1, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public void Should_Execute_With_TParam_For_Func_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = processor.Execute<int, int>((_) => throw new InvalidOperationException(), 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = processor.Execute((v) => addable += v, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ExecuteAsync_With_TParam_For_NonGeneric_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = await processor.ExecuteAsync(async(_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.ExecuteAsync(async (_) => await Task.Delay(1), 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ExecuteAsync_With_TParam_For_NonGeneric_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult result = null;
			if (throwEx)
			{
				result = await processor.ExecuteAsync(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.ExecuteAsync(async (v,_) => {await Task.Delay(1); addable += v;}, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ExecuteAsync_With_TParam_For_Generic_AsyncFunc_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = await processor.ExecuteAsync<int, int>(async (_) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.ExecuteAsync(async (_) => { await Task.Delay(1); return 1; }, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task Should_ExecuteAsync_With_TParam_For_Generic_AsyncFunc_With_TParam_WithErrorProcessorOf_Action_Process_Correctly(bool throwEx)
		{
			int m = 0;
			int addable = 1;

			void action(Exception _, ProcessingErrorInfo<int> pi)
			{
				m = pi.Param;
			}

			var processor = new SimplePolicyProcessor(true)
							.WithErrorContextProcessorOf<int>(action);

			PolicyResult<int> result = null;
			if (throwEx)
			{
				result = await processor.ExecuteAsync<int, int>(async (_, __) => { await Task.Delay(1); throw new InvalidOperationException(); }, 5);
				Assert.That(m, Is.EqualTo(5));
				Assert.That(result.NoError, Is.False);
			}
			else
			{
				result = await processor.ExecuteAsync(async (v, _) => { await Task.Delay(1); addable += v; return addable; }, 5);
				Assert.That(m, Is.EqualTo(0));
				Assert.That(addable, Is.EqualTo(6));
				Assert.That(result.Result, Is.EqualTo(6));
				Assert.That(result.NoError, Is.True);
			}
			Assert.That(result.IsSuccess, Is.True);
		}

		private class TestErrorProcessor : IErrorProcessor
		{
			public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
			{
				throw new NotImplementedException();
			}

			public Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
			{
				throw new NotImplementedException();
			}
		}
	}
}
