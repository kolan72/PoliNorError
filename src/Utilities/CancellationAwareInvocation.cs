using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PoliNorError
{
	internal static class CancellationAwareInvocation
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static OperationCanceledException InvokeCapturingCancel(Action action, CancellationToken token)
		{
			try
			{
				action();
				return null;
			}
			catch (OperationCanceledException oe) when (token.IsCancellationRequested)
			{
				return oe;
			}
			catch (AggregateException ae) when (token.IsCancellationRequested)
			{
				return ae.GetCancellationException(token);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static OperationCanceledException InvokeCapturingCancel<TParam>(Action<TParam> action, TParam param, CancellationToken token)
		{
			try
			{
				action(param);
				return null;
			}
			catch (OperationCanceledException oe) when (token.IsCancellationRequested)
			{
				return oe;
			}
			catch (AggregateException ae) when (token.IsCancellationRequested)
			{
				return ae.GetCancellationException(token);
			}
		}

	}
}
