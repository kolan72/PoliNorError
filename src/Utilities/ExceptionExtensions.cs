using System;
using System.Linq;
using System.Threading;

namespace PoliNorError
{
	internal static class ExceptionExtensions
    {
        public static bool HasCanceledException(this AggregateException ae, CancellationToken token) => ae.Flatten().InnerExceptions
                                                                                                                        .Any(ie => ie is OperationCanceledException operationCanceledException && operationCanceledException.CancellationToken.Equals(token));
    }
}
