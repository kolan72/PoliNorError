using System.Linq;

namespace PoliNorError.Tests
{
	internal static class PredicateFuncsForTests
	{
		public static bool GenericPredicate(PolicyResult<int> pr) => pr.Errors.Any(e => e.Message == "Test");

		public static bool Predicate(PolicyResult pr) => pr.Errors.Any(e => e.Message == "Test");
	}
}
