using System;
using System.Collections.Generic;
using System.Linq;
using static PoliNorError.HandleErrorPolicyBase;

namespace PoliNorError
{
	internal class HandlerRunnersCollection
	{
		private List<IEnumerable<IHandlerRunner>> _handlerRunnerBases;

		private static readonly Func<IEnumerable<IHandlerRunner>, bool> _func = (ie) => ie.Any();

		public static HandlerRunnersCollection FromSyncAndNotSync(IEnumerable<IHandlerRunner> syncCollection, IEnumerable<IHandlerRunner> asyncCollection)
		{
			return new HandlerRunnersCollection() { _handlerRunnerBases = new List<IEnumerable<IHandlerRunner>>() { syncCollection, asyncCollection } };
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
