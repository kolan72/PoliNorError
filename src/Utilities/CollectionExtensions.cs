using System.Collections.Generic;

namespace PoliNorError
{
	public static class CollectionExtensions
	{
		public static void RemoveLast<T>(this IList<T> collection)
		{
			if (collection.Count > 0)
			{
				collection.RemoveAt(collection.Count - 1);
			}
		}
	}
}
