using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError.TryCatch
{
	internal static class CatchBlockHandlerCollectionWrapper
	{
		internal static SimplePolicy Wrap(IEnumerable<CatchBlockHandler> catchBlockHandlers)
		{
			var firstHandler = catchBlockHandlers?.FirstOrDefault();
			if (firstHandler is null)
			{
				throw new ArgumentNullException(nameof(catchBlockHandlers), $"{nameof(catchBlockHandlers)} can not be null!");
			}
			var currentPolicy = new SimplePolicy(firstHandler.CatchBlockFilter, firstHandler.BulkErrorProcessor, true);
			foreach (var handler in catchBlockHandlers.Skip(1))
			{
				currentPolicy = currentPolicy.WrapUp(new SimplePolicy(handler.CatchBlockFilter, handler.BulkErrorProcessor, true)).OuterPolicy;
			}
			return currentPolicy;
		}
	}
}
