using System;
using System.Collections.Generic;

namespace PoliNorError.TryCatch
{
	internal static class CatchBlockHandlerCollectionWrapper
	{
		internal static SimplePolicy Wrap(IReadOnlyList<CatchBlockHandler> catchBlockHandlers)
		{
			if (catchBlockHandlers is null || catchBlockHandlers.Count == 0)
			{
				throw new ArgumentNullException(nameof(catchBlockHandlers), $"{nameof(catchBlockHandlers)} cannot be null or empty!");
			}
			var currentPolicy = new SimplePolicy(catchBlockHandlers[0].CatchBlockFilter, catchBlockHandlers[0].BulkErrorProcessor, true);
			for (int i = 1; i < catchBlockHandlers.Count; i++)
			{
				var handler = catchBlockHandlers[i];
				currentPolicy = currentPolicy.WrapUp(new SimplePolicy(handler.CatchBlockFilter, handler.BulkErrorProcessor, true)).OuterPolicy;
			}
			return currentPolicy;
		}
	}
}
