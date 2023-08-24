using System.Collections.Generic;
using System.Linq;
using static PoliNorError.Policy;

namespace PoliNorError
{
	internal static class EnumerableHandlerRunnerBaseExtensions
	{
		public static HandlerRunnerSyncType MapToSyncType(this IEnumerable<IHandlerRunnerBase> handlerRunners)
		{
			return HandlerRunnersCollection.FromSyncAndNotSync(handlerRunners.Where(hr => hr.SyncRun), handlerRunners.Where(hr => !hr.SyncRun)).MapToSyncType();
		}
	}
}
