﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	internal static class EnumerableExtension
	{
		public static bool IsEmpty<T>(this IEnumerable<T> source)
		{
			return !source.Any();
		}

		public static void ActionForAll<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var element in source)
			{
				action(element);
			}
		}

		public static void ActionForAll<T>(this IList<T> source, Action<IEnumerable<T>> collectionAct, bool excludeLastPolicy)
		{
			if (!excludeLastPolicy)
			{
				collectionAct(source);
			}
			else if (source.Count > 1)
			{
				collectionAct(source.SkipLast());
			}
		}

		public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> source)
		{
			using (var e = source.GetEnumerator())
			{
				if (e.MoveNext())
				{
					for (var value = e.Current; e.MoveNext(); value = e.Current)
					{
						yield return value;
					}
				}
			}
		}

		public static ConditionCheckResult CheckCollectionsForCondition<T>(this IEnumerable<T> collection, Func<T, bool> condition)
		{
			int firstMapIndex = collection.TakeWhile(t => !condition(t)).Count();
			if (firstMapIndex == collection.Count())
				return ConditionCheckResult.ForNoOne();
			if (collection.Skip(firstMapIndex+1).Any(t => condition(t)))
			{
				return ConditionCheckResult.ForMoreThenOne();
			}
			else
			{
				return ConditionCheckResult.ForOnlyOne(firstMapIndex);
			}
		}

		public class ConditionCheckResult
		{
			public static ConditionCheckResult ForNoOne() => new ConditionCheckResult() { ConditionMet = ConditionMetType.NoOne };
			public static ConditionCheckResult ForMoreThenOne() => new ConditionCheckResult() { ConditionMet = ConditionMetType.MoreThenOne };
			public static ConditionCheckResult ForOnlyOne(int indx) => new ConditionCheckResult() { ConditionMet = ConditionMetType.OnlyOne, Indx = indx };

			public int Indx { get; private set; } = -1;
			public ConditionMetType ConditionMet { get; private set; }

			public enum ConditionMetType
			{
				NoOne = 0,
				OnlyOne,
				MoreThenOne
			}
		}
	}
}
