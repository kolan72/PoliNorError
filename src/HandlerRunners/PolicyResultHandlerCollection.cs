using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static PoliNorError.Policy;

namespace PoliNorError
{
	internal interface IPolicyResultHandlerCollection
	{
		void AddHandler(Action<PolicyResult, CancellationToken> act);
		void AddHandler(Func<PolicyResult, CancellationToken, Task> func);
		void AddHandler<T>(Action<PolicyResult<T>, CancellationToken> act);
		void AddHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func);

		PolicyResult Handle(PolicyResult policyRetryResult, CancellationToken token = default);
		PolicyResult<T> Handle<T>(PolicyResult<T> policyRetryResult, CancellationToken token = default);
		Task<PolicyResult> HandleAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default);
		Task<PolicyResult<T>> HandleAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default);
	}

	internal static class IPolicyResultHandlerCollectionExtensions
	{
		public static void AddHandler(this IPolicyResultHandlerCollection handlerCollection, Func<PolicyResult, Task> func)
		{
			handlerCollection.AddHandler((pr, _) => func(pr));
		}

		public static void AddHandler<T>(this IPolicyResultHandlerCollection handlerCollection, Func<PolicyResult<T>, Task> func)
		{
			handlerCollection.AddHandler<T>((pr, _) => func(pr));
		}

		public static void AddHandler(this IPolicyResultHandlerCollection handlerCollection, Action<PolicyResult> act)
		{
			handlerCollection.AddHandler((pr, _) => act(pr));
		}

		public static void AddHandler<T>(this IPolicyResultHandlerCollection handlerCollection, Action<PolicyResult<T>> act)
		{
			handlerCollection.AddHandler<T>((pr, _) => act(pr));
		}
	}

	internal class PolicyResultHandlerCollection : IPolicyResultHandlerCollection
	{
		private int _curNum;

		public PolicyResultHandlerCollection()
		{
			Handlers = new List<IHandlerRunner>();
			GenericHandlers = new List<IHandlerRunnerT>();
		}

		public void AddHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func);
			AddHandler(handler);
		}

		public void AddHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			var handler = ASyncHandlerRunnerT.Create(func);
			AddGenericHandler(handler);
		}

		public void AddHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act);
			AddHandler(handler);
		}

		public void AddHandler<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			var handler = SyncHandlerRunnerT.Create(act);
			AddGenericHandler(handler);
		}

		public PolicyResult<T> Handle<T>(PolicyResult<T> policyRetryResult, CancellationToken token = default)
		{
			return policyRetryResult.HandleResultForceSync(GenericHandlers, token);
		}

		public PolicyResult Handle(PolicyResult policyRetryResult, CancellationToken token = default)
		{
			return policyRetryResult.HandleResultForceSync(Handlers, token);
		}

		public async Task<PolicyResult> HandleAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
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

		public async Task<PolicyResult<T>> HandleAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default)
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
