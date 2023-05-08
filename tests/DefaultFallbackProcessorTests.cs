using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace PoliNorError.Tests
{
	internal class DefaultFallbackProcessorTests
	{
		[Test]
		public void Should_DefaultFallbackProcessor_FallbackT_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }
			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(save);
			var polResult = processor.Fallback(() => throw new Exception("Test_Fallback"), (_) => 1);
			Assert.AreEqual(2, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.AreEqual(1, polResult.Result);
		}

		[Test]
		public void Should_DefaultFallbackProcessor_FallbackT_Call_Error_Func_When_Error()
		{
			int i = 1;
			async Task onErrorTask(Exception _, CancellationToken __) { await Task.FromResult(i++); }

			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(onErrorTask);
			var polResult = processor.Fallback(() => throw new Exception("Test_Fallback"), (_) => 1);
			Assert.AreEqual(2, i);
			Assert.IsFalse(polResult.IsFailed);
			Assert.AreEqual(1, polResult.Result);
		}

		[Test]
		public void Should_Fallback_CallFallback()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			void fallback(CancellationToken _) { i++; }

			var res = processor.Fallback(() => throw new Exception(), fallback);

			Assert.AreEqual(2, i);
			Assert.IsFalse(res.IsFailed);
		}

		[Test]
		public void Should_FallbackT_Returns_FallbackValue_If_Error()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			int fallback(CancellationToken _) { i++; return i; }

			var res = processor.Fallback(() => throw new Exception(), fallback);

			Assert.AreEqual(2, res.Result);
			Assert.IsFalse(res.IsFailed);
		}

		[Test]
		public void Should_FallbackT_Returns_SuccessValue_If_NotError()
		{
			var processor = new DefaultFallbackProcessor();

			int i = 1;

			int fallback(CancellationToken _) { i++; return i; }

			var res = processor.Fallback(() => i, fallback);

			Assert.AreEqual(1, res.Result);
			Assert.IsFalse(res.IsFailed);
			Assert.IsTrue(res.IsOk);
		}

		[Test]
		public void Should_DefaultFallbackProcessor_Fallback_Call_Error_Action_When_Error()
		{
			int i = 1;
			void save(Exception _, CancellationToken __) { i++; }
			var processor = new DefaultFallbackProcessor().WithErrorProcessorOf(save);
			var polResult = processor.Fallback(()=>throw new Exception("Test_Fallback"), (_) => Expression.Empty());
			Assert.AreEqual(2, i);
			Assert.IsFalse(polResult.IsFailed);
		}

		[Test]
		public void Should_DefaultFallbackProcessor_Fallback_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback(() => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			Assert.IsTrue(polResult.IsFailed);
		}

		[Test]
		public void Should_DefaultFallbackProcessor_FallbackT_Result_IsFailed_Equals_True_When_Error_In_CatchBlockProcessing()
		{
			var processor = new DefaultFallbackProcessor();
			var polResult = processor.Fallback<int>(() => throw new Exception("Test_Save"), (_) => throw new Exception("Test_Fallback"));

			Assert.IsTrue(polResult.IsFailed);
		}

		[Test]
		[TestCase("Test", false, "Test")]
		[TestCase("Test2", true, "Test")]
		public void Should_Generic_ForError_Work(string paramName, bool errFilterUnsatisfied, string errorParamName)
		{
			var processor = new DefaultFallbackProcessor();
			processor.ForError<ArgumentNullException>((ane) => ane.ParamName == paramName);
			void saveWithInclude() => throw new ArgumentNullException(errorParamName);
			var tryResCountWithNoInclude = processor.Fallback(saveWithInclude, (_) => Expression.Empty());
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
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
			Assert.AreEqual(errFilterUnsatisfied, tryResCountWithNoInclude.ErrorFilterUnsatisfied);
		}
	}
}