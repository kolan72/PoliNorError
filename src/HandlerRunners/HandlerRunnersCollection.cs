using System;
using System.Collections.Generic;
using System.Linq;
using static PoliNorError.Policy;

namespace PoliNorError
{
	internal class HandlerRunnersCollection
	{
		private List<IEnumerable<IHandlerRunnerBase>> _handlerRunnerBases;

		private static readonly Func<IEnumerable<IHandlerRunnerBase>, bool> _func = (ie) => ie.Any();

		public static HandlerRunnersCollection FromSyncAndNotSync(IEnumerable<IHandlerRunnerBase> syncCollection, IEnumerable<IHandlerRunnerBase> asyncCollection)
		{
			return new HandlerRunnersCollection() { _handlerRunnerBases = new List<IEnumerable<IHandlerRunnerBase>>() { syncCollection, asyncCollection } };
		}

		public HandlerRunnerSyncType MapToSyncType()
		{
			var result = _handlerRunnerBases.CheckCollectionsForCondition(_func);
			switch (result.ConditionMet)
			{
				case EnumerableExtension.ConditionCheckResult.ConditionMetType.NoOne:
					return HandlerRunnerSyncType.None;
				case EnumerableExtension.ConditionCheckResult.ConditionMetType.OnlyOne:
					return result.Indx == 0 ? HandlerRunnerSyncType.Sync : HandlerRunnerSyncType.Async;
				case EnumerableExtension.ConditionCheckResult.ConditionMetType.MoreThenOne:
					return HandlerRunnerSyncType.Misc;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
