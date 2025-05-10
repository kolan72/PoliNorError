using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Policy;

namespace PoliNorError
{
	internal class PolicyResultHandlerCollection
	{
		private int _curNum;

		public PolicyResultHandlerCollection()
		{
			Handlers = new List<IHandlerRunner>();
			GenericHandlers = new List<IHandlerRunnerT>();
		}

		internal void AddHandler(Func<PolicyResult, Task> func)
		{
			AddHandler((pr, _) => func(pr));
		}

		internal void AddHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func);
			AddHandler(handler);
		}

		internal void AddHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			AddHandler<T>((pr, _) => func(pr));
		}

		internal void AddHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			var handler = ASyncHandlerRunnerT.Create(func);
			AddGenericHandler(handler);
		}

		internal void AddHandler(Action<PolicyResult> act)
		{
			AddHandler((pr, _) => act(pr));
		}

		internal void AddHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act);
			AddHandler(handler);
		}

		internal void AddHandler<T>(Action<PolicyResult<T>> act)
		{
			AddHandler<T>((pr, _) => act(pr));
		}

		internal void AddHandler<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			var handler = SyncHandlerRunnerT.Create(act);
			AddGenericHandler(handler);
		}

		internal PolicyResult<T> Handle<T>(PolicyResult<T> policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(GenericHandlers, token);
		}

		internal PolicyResult Handle(PolicyResult policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(Handlers, token);
		}

		internal async Task<PolicyResult> HandleAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (Handlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					return policyRetryResult.HandleResultSync(Handlers, token);
				case HandlerRunnerSyncType.Misc:
					return await policyRetryResult.HandleResultMisc(Handlers, configureAwait, token).ConfigureAwait(configureAwait);
				case HandlerRunnerSyncType.Async:
					return await policyRetryResult.HandleResultAsync(Handlers, configureAwait, token).ConfigureAwait(configureAwait);
				case HandlerRunnerSyncType.None:
					return policyRetryResult;
				default:
					throw new NotImplementedException();
			}
		}

		internal async Task<PolicyResult<T>> HandleAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (GenericHandlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					return policyRetryResult.HandleResultSync(GenericHandlers, token);
				case HandlerRunnerSyncType.Misc:
					return await policyRetryResult.HandleResultMisc(GenericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
				case HandlerRunnerSyncType.Async:
					return await policyRetryResult.HandleResultAsync(GenericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
				case HandlerRunnerSyncType.None:
					return policyRetryResult;
				default:
					throw new NotImplementedException();
			}
		}

		private void AddGenericHandler(IHandlerRunnerT handlerRunnerT)
		{
			handlerRunnerT.CollectionIndex = _curNum++;
			GenericHandlers.Add(handlerRunnerT);
		}

		private void AddHandler(IHandlerRunner handlerRunner)
		{
			handlerRunner.CollectionIndex = _curNum++;
			Handlers.Add(handlerRunner);
		}

		internal List<IHandlerRunner> Handlers { get; }
		internal List<IHandlerRunnerT> GenericHandlers { get; }
	}
}
