using System;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	public class ExceptionFilterSlim
	{
		public Func<Exception, bool> CanHandle { get; }

		public ExceptionFilterSlim(ExceptionFilter errorFilter)
		{
			CanHandle = errorFilter.GetCanHandle();
		}
	}
}
