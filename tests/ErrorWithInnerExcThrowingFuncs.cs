using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public static class ErrorWithInnerExcThrowingFuncs
	{
		public static void ActionWithInner() => throw new TestExceptionWithInnerException();
		public static void ActionWithInnerWithMsg(string innerExceptionMsg) => throw new TestExceptionWithInnerException("", innerExceptionMsg);

		public static void Action() => throw new Exception();

		public static int FunWithInner() => throw new TestExceptionWithInnerException();
		public static int FunWithInnerWithMsg(string innerExceptionMsg) => throw new TestExceptionWithInnerException("", innerExceptionMsg);

		public static int Fun() => throw new Exception();

		public static async Task AsyncFuncWithInner(CancellationToken _) { await Task.Delay(1); throw new TestExceptionWithInnerException(); }
		public static async Task AsyncFunc(CancellationToken _) { await Task.Delay(1); throw new Exception(); }

		public static async Task<int> AsyncFuncWithInnerT(CancellationToken _) { await Task.Delay(1); throw new TestExceptionWithInnerException(""); }

		public static int FuncWithInner() => throw new TestExceptionWithInnerException();

		public class TestExceptionWithInnerException : Exception
		{
			public TestExceptionWithInnerException() : this("", new TestInnerException())
			{
			}

			public TestExceptionWithInnerException(string message, string innerExceptionMessage) : this(message, new TestInnerException(innerExceptionMessage))
			{
			}

			public TestExceptionWithInnerException(string message) : base(message)
			{
			}

			public TestExceptionWithInnerException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}

		public class TestInnerException : Exception
		{
			public TestInnerException()
			{
			}

			public TestInnerException(string message) : base(message)
			{
			}

			public TestInnerException(string message, Exception innerException) : base(message, innerException)
			{
			}
		}
	}
}
