using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Linq;
using NUnit.Framework.Legacy;
using static PoliNorError.Tests.ErrorWithInnerExcThrowingFuncs;

namespace PoliNorError.Tests
{
	internal class DefaultFallbackProcessorTests
	{
		[Test]
		public void Should_FallbackT_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }
			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(save);
			var polResult = processor.Fallback(() => throw new Exception("Test_Fallback"), (_) => 1);
			ClassicAssert.AreEqual(2, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.AreEqual(1, polResult.Result);
		}

		[Test]
		public void Should_FallbackT_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(onErrorTask);
			var polResult = processor.Fallback(() => throw new Exception("Test_Fallback"), (_) => 1);
			ClassicAssert.AreEqual(2, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
			ClassicAssert.AreEqual(1, polResult.Result);
		}

		[Test]
		public void Should_Fallback_CallFallback()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			void fallback(CancellationToken _) { i++; }

			var res = processor.Fallback(() => throw new Exception(), fallback);

			ClassicAssert.AreEqual(2, i);
			ClassicAssert.IsFalse(res.IsFailed);
		}

		[Test]
		public void Should_FallbackT_Returns_FallbackValue_If_Error()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			int fallback(CancellationToken _) { i++; return i; }

			var res = processor.Fallback(() => throw new Exception(), fallback);

			ClassicAssert.AreEqual(2, res.Result);
			ClassicAssert.IsFalse(res.IsFailed);
		}

		[Test]
		public void Should_FallbackT_Not_Executed_If_Error_And_Canceled_During_Error_Processors_Run()
		{
			var cancelTokenSource = new CancellationTokenSource();
			var processor = new DefaultFallbackProcessor();

			int i = 0;
			int p = 0;

			int fallback(CancellationToken _) { i++; return i; }

			var res = processor
							.WithErrorProcessorOf((Exception _, CancellationToken __) => {p++; cancelTokenSource.Cancel();})
							.Fallback(() => throw new Exception(), fallback, cancelTokenSource.Token);

			ClassicAssert.IsTrue(res.IsFailed);
			ClassicAssert.IsTrue(res.IsCanceled);
			ClassicAssert.AreEqual(1, p);
			ClassicAssert.AreEqual(0, i);
			ClassicAssert.AreEqual(0, res.Result);
			cancelTokenSource.Dispose();
		}

		[Test]
		public void Should_FallbackT_Returns_SuccessValue_If_NoError()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			int fallback(CancellationToken _) { i++; return i; }

			var res = processor.Fallback(() => i, fallback);

			ClassicAssert.AreEqual(1, res.Result);
			ClassicAssert.IsFalse(res.IsFailed);
			ClassicAssert.IsTrue(res.NoError);
		}

		[Test]
		public void Should_Fallback_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }
			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(save);
			var polResult = processor.Fallback(()=>throw new Exception("Test_Fallback"), (_) => Expression.Empty());
			ClassicAssert.AreEqual(2, i);
			ClassicAssert.IsFalse(polResult.IsFailed);
		}

		[Test]
		public void Should_Fallback_Result_Correct_When_Error_In_CatchBlockProcessing()
		{
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback(() => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.IsNotNull(polResult.UnprocessedError);
			ClassicAssert.AreEqual(CatchBlockExceptionSource.PolicyRule, polResult.CatchBlockErrors.FirstOrDefault().ExceptionSource);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		public void Should_FallbackT_Result_Correct_When_Error_In_CatchBlockProcessing()
		{
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback<int>(() => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			ClassicAssert.IsTrue(polResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
			ClassicAssert.AreEqual(CatchBlockExceptionSource.PolicyRule, polResult.CatchBlockErrors.FirstOrDefault().ExceptionSource);
			ClassicAssert.AreEqual(PolicyResultFailedReason.PolicyProcessorFailed, polResult.FailedReason);
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_Generic_IncludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = new DefaultFallbackProcessor();
			processor.IncludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(true, true)]
		[TestCase(true, false)]
		[TestCase(true, null)]
		[TestCase(false, null)]
		public void Should_IncludeInnerError_Work(bool withInnerError, bool? satisfyFilterFunc)
		{
			var processor = FallbackProcessor
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
					result = processor.Fallback(((Action<string>)ActionWithInnerWithMsg).Apply("Test"), (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Fallback(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"), (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else
				{
					result = processor.Fallback(ActionWithInner, (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
			}
			else
			{
				result = processor.Fallback(Action, (_) => { });
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
			var processor = FallbackProcessor
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
					result = processor.Fallback(((Action<string>)ActionWithInnerWithMsg).Apply("Test"), (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
				else if (satisfyFilterFunc == false)
				{
					result = processor.Fallback(((Action<string>)ActionWithInnerWithMsg).Apply("Test2"), (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.False);
				}
				else
				{
					result = processor.Fallback(ActionWithInner, (_) => { });
					Assert.That(result.ErrorFilterUnsatisfied, Is.True);
				}
			}
			else
			{
				result = processor.Fallback(Action, (_) => { });
				Assert.That(result.ErrorFilterUnsatisfied, Is.False);
			}
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_IncludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = FallbackProcessor.CreateDefault()
											 .IncludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, true)]
		[TestCase(TestErrorSetMatch.FirstParam, false)]
		[TestCase(TestErrorSetMatch.SecondParam, false)]
		public void Should_IncludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = FallbackProcessor.CreateDefault();
			processor.IncludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Fallback(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_No_ErrorProcessor_Process_When_ErrorFilterUnsatisfied()
		{
			const int init_value = 1;
			int i = init_value;
			void save(Exception _, CancellationToken __) { i++; }

			var processor = new DefaultFallbackProcessor();
			processor.IncludeError<Exception>((ane) => ane.Message == "Test2")
					  .WithErrorProcessorOf(save);
			void saveWithInclude() => throw new Exception("Test");
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			ClassicAssert.IsTrue(tryResCountWithNoInclude.ErrorFilterUnsatisfied);
			ClassicAssert.AreEqual(init_value, i);
		}

		[Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_Generic_ExcludeError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = new DefaultFallbackProcessor();
			processor.ExcludeError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase("Test2", false, "Test")]
		[TestCase("Test", true, "Test")]
		public void Should_ExcludeError_BasedOnExpression_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = FallbackProcessor.CreateDefault()
											 .ExcludeError(ex => ex.Message == paramName);
			void saveWithInclude() => throw new Exception(errorParamName);
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		[TestCase(TestErrorSetMatch.NoMatch, false)]
		[TestCase(TestErrorSetMatch.FirstParam, true)]
		[TestCase(TestErrorSetMatch.SecondParam, true)]
		public void Should_ExcludeErrorSet_WithTwoGenericParams_Work(TestErrorSetMatch testErrorSetMatch, bool errFilterUnsatisfied, string errorParamName = null)
		{
			var processor = FallbackProcessor.CreateDefault();
			processor.ExcludeErrorSet<ArgumentException, ArgumentNullException>();

			var tryResCountWithNoInclude = processor.Fallback(TestHandlingForErrorSet.GetTwoGenericParamAction(testErrorSetMatch, errorParamName), (_) => Expression.Empty());
			ClassicAssert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_Fallback_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = proc.Fallback(null, (_) => { });
			ClassicAssert.IsTrue(fallbackResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}

		[Test]
		public void Should_PolicyResult_Contains_NoDelegateException_When_FallbackT_Null_Delegate()
		{
			var proc = FallbackProcessor.CreateDefault();
			var fallbackResult = proc.Fallback(null, (_) => 1);
			ClassicAssert.IsTrue(fallbackResult.IsFailed);
			ClassicAssert.AreEqual(PolicyResultFailedReason.DelegateIsNull, fallbackResult.FailedReason);
			ClassicAssert.AreEqual(typeof(NoDelegateException), fallbackResult.Errors.FirstOrDefault()?.GetType());
		}
	}
}