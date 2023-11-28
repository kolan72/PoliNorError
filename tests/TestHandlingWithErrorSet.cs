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
		internal static Action GetTwoGenericParamAction(TestErrorSetMatch testErrorSetMatch, string errorParamName = null)
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
				}
			};
		}
	}
}
