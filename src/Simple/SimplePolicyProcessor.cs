using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class SimplePolicyProcessor : PolicyProcessor, ISimplePolicyProcessor
	{
		public SimplePolicyProcessor(IBulkErrorProcessor bulkErrorProcessor = null) : base(bulkErrorProcessor) { }

		public static ISimplePolicyProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor = null) => new SimplePolicyProcessor(bulkErrorProcessor);

		public PolicyResult Execute(Action action, CancellationToken token = default)
		{
			if (action == null)
				return PolicyResult.ForSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(action)}' is null."));

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
				HandleCatchBlockAndChangeResult(ex, result, ProcessErrorInfo.FromSimple(), token);
			}
			return result;
		}

		public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

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
				HandleCatchBlockAndChangeResult(ex, result, ProcessErrorInfo.FromSimple(), token);
			}
			return result;
		}

		public async Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult.ForNotSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

			PolicyResult result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				await func(token).ConfigureAwait(configureAwait);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				await HandleCatchBlockAndChangeResultAsync(ex, result, ProcessErrorInfo.FromSimple(), token, configureAwait).ConfigureAwait(configureAwait);
			}
			return result;
		}

		public async Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return PolicyResult<T>.ForNotSync().SetFailedWithError(new NoDelegateException($"The argument '{nameof(func)}' is null."));

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = await func(token).ConfigureAwait(configureAwait);
				result.SetResult(resAction);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				result.AddError(ex);
				await HandleCatchBlockAndChangeResultAsync(ex, result, ProcessErrorInfo.FromSimple(), token, configureAwait).ConfigureAwait(configureAwait);
			}
			return result;
		}
	}
}
