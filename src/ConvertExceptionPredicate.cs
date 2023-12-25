using System;

namespace PoliNorError
{
	internal delegate bool ConvertExceptionPredicate<TException>(Exception exception, out TException typedException);
}
