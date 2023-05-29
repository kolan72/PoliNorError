using System;
using System.Threading;

namespace PoliNorError
{
	internal class SimplePolicyProcessor : PolicyProcessor
	{
		public PolicyResult Execute(Action action, CancellationToken token = default)
		{
			PolicyResult result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				action();
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				HandleCatchBlockAndChangeResult(ex, result, CatchBlockProcessErrorInfo.FromSimple(), token);
			}
			return result;
		}

		public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			var result = PolicyResult<T>.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = func();
				result.SetOk();
				result.SetResult(resAction);
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				HandleCatchBlockAndChangeResult(ex, result, CatchBlockProcessErrorInfo.FromSimple(), token);
			}
			return result;
		}
	}
}
