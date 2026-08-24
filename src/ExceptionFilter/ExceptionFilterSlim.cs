using System;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal class ExceptionFilterSlim
	{
		public Func<Exception, bool> CanHandle { get; }

		public ExceptionFilterSlim(ExceptionFilter errorFilter)
		{
			CanHandle = errorFilter.GetCanHandle();
		}
	}
}
