using System;

namespace PoliNorError.Tests
{
	internal enum TestErrorSetMatch
	{
		NoMatch,
		FirstParam,
		SecondParam,
		ThirdParam
	}

	internal static class TestHandlingForErrorSet
	{
		internal static PolicyResult HandlePolicyWithErrorSet(IPolicyBase policyBase, TestErrorSetMatch testErrorSetMatch)
		{
			return policyBase.Handle(GetTwoGenericParamAction(testErrorSetMatch));
		}

		internal static Action GetTwoGenericParamAction(TestErrorSetMatch testErrorSetMatch, string errorParamName = null, bool testInnerException  = false)
		{
			return () =>
			{
				if (!testInnerException)
				{
					switch (testErrorSetMatch)
					{
						case TestErrorSetMatch.NoMatch:
							throw new Exception("Test");
						case TestErrorSetMatch.FirstParam:
							throw new ArgumentException("Test");
						case TestErrorSetMatch.SecondParam:
							throw new ArgumentNullException(errorParamName, "Test");
					}
				}
				else
				{
					switch (testErrorSetMatch)
					{
						case TestErrorSetMatch.NoMatch:
							throw new ArgumentException("Test");
						case TestErrorSetMatch.FirstParam:
							throw new TestExceptionWithInnerArgumentException();
						case TestErrorSetMatch.SecondParam:
							throw new TestExceptionWithInnerArgumentNullException();
					}
				}
			};
		}

		internal static Func<int> GetTwoGenericParamFunc(TestErrorSetMatch testErrorSetMatch, string errorParamName = null)
		{
			return () =>
			{
				switch (testErrorSetMatch)
				{
					case TestErrorSetMatch.NoMatch:
						throw new Exception("Test");
					case TestErrorSetMatch.FirstParam:
						throw new ArgumentException("Test");
					case TestErrorSetMatch.SecondParam:
						throw new ArgumentNullException(errorParamName, "Test");
					default:
						throw new NotImplementedException();
				}
			};
		}
	}

#pragma warning disable RCS1194 // Implement exception constructors.
	public class TestExceptionWithInnerArgumentException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
	{
		public TestExceptionWithInnerArgumentException() : base("", new ArgumentException("Test")) { }
	}

#pragma warning disable RCS1194 // Implement exception constructors.
	public class TestExceptionWithInnerArgumentNullException : Exception
#pragma warning restore RCS1194 // Implement exception constructors.
	{
		public TestExceptionWithInnerArgumentNullException(string e = null) : base("", new ArgumentNullException(nameof(e)))
		{
			E = e;
		}

		public string E { get; }
	}
}
