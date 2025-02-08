using System;

namespace PoliNorError
{
	internal static class ThrowHelper
	{
		public static void ThrowIfNotImplemented<TService, TImplementor>(TService service, out TImplementor implementor) where TImplementor : TService
		{
			if (!(service is TImplementor inproc))
			{
				throw new NotImplementedException($"Supported only for the {typeof(TImplementor).Name} implementation of the {typeof(TService).Name}.");
			}
			implementor = inproc;
		}
	}
}
