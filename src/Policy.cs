using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class Policy
	{
		protected string _policyName;

		private PolicyWrapperFactory _policyWrapperFactory;

		private readonly List<IHandlerRunner> _handlers;
		private readonly List<IHandlerRunnerT> _genericHandlers;

		private protected Policy(IPolicyProcessor policyProcessor)
		{
			_handlers = new List<IHandlerRunner>();
			_genericHandlers = new List<IHandlerRunnerT>();
			PolicyProcessor = policyProcessor;
		}

		internal void AddAsyncHandler(Func<PolicyResult, Task> func)
		{
			AddAsyncHandler((pr, _) => func(pr));
		}

		internal void AddAsyncHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func, _handlers.Count);
			_handlers.Add(handler);
		}

		internal void AddAsyncHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			AddAsyncHandler<T>((pr, _) => func(pr));
		}

		internal void AddAsyncHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			var handler = ASyncHandlerRunnerT.Create(func, _handlers.Count);
			_genericHandlers.Add(handler);
		}

		internal void AddSyncHandler(Action<PolicyResult> act)
		{
			AddSyncHandler((pr, _) => act(pr));
		}

		internal void AddSyncHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act, _handlers.Count);
			_handlers.Add(handler);
		}

		internal void AddSyncHandler<T>(Action<PolicyResult<T>> act)
		{
			AddSyncHandler<T>((pr, _) => act(pr));
		}

		internal void AddSyncHandler<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			var handler = SyncHandlerRunnerT.Create(act, _handlers.Count);
			_genericHandlers.Add(handler);
		}

		protected PolicyResult<T> HandlePolicyResult<T>(PolicyResult<T> policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(_genericHandlers, token);
		}

		protected PolicyResult HandlePolicyResult(PolicyResult policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(_handlers, token);
		}

		protected async Task HandlePolicyResultAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (_handlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					policyRetryResult.HandleResultSync(_handlers, token);
					break;
				case HandlerRunnerSyncType.Misc:
					await policyRetryResult.HandleResultMisc(_handlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.Async:
					await policyRetryResult.HandleResultAsync(_handlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.None:
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected async Task HandlePolicyResultAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (_genericHandlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					policyRetryResult.HandleResultSync(_genericHandlers, token);
					break;
				case HandlerRunnerSyncType.Misc:
					await policyRetryResult.HandleResultMisc(_genericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.Async:
					await policyRetryResult.HandleResultAsync(_genericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.None:
					break;
				default:
					throw new NotImplementedException();
			}
		}

		internal void SetWrap(IPolicyBase policyToWrap)
		{
			if (_policyWrapperFactory != null)
			{
				throw new NotImplementedException("More than one wrapped policy is not supported.");
			}
			_policyWrapperFactory = new PolicyWrapperFactory(policyToWrap);
		}

		internal void SetWrap(IEnumerable<IPolicyBase> policies, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			if (_policyWrapperFactory != null)
			{
				throw new ArgumentException("More than one wrapped PolicyCollection is not supported.");
			}
			_policyWrapperFactory = new PolicyWrapperFactory(policies, throwOnWrappedCollectionFailed);
		}

		internal (Action Act, PolicyWrapper Wrapper) WrapDelegateIfNeed(Action action, CancellationToken token)
		{
			if (_policyWrapperFactory == null)
			{
				return (action, null);
			}
			else
			{
				if (action == null)
					return (null, null);

				var wrapper = _policyWrapperFactory.CreateWrapper(action, token);
				return (wrapper.Handle, wrapper);
			}
		}

		internal (Func<T> Fn, PolicyWrapper<T> Wrapper) WrapDelegateIfNeed<T>(Func<T> fn, CancellationToken token)
		{
			if (_policyWrapperFactory == null)
			{
				return (fn, null);
			}
			else
			{
				if (fn == null)
					return (null, null);

				var wrapper = _policyWrapperFactory.CreateWrapper(fn, token);
				return (wrapper.Handle, wrapper);
			}
		}

		internal (Func<CancellationToken, Task> Fn, PolicyWrapper Wrapper) WrapDelegateIfNeed(Func<CancellationToken, Task> fn, CancellationToken token, bool configureAwait)
		{
			if (_policyWrapperFactory == null)
			{
				return (fn, null);
			}
			else
			{
				if (fn == null)
					return (null, null);

				var wrapper = _policyWrapperFactory.CreateWrapper(fn, token, configureAwait);
				return (wrapper.HandleAsync, wrapper);
			}
		}

		internal (Func<CancellationToken, Task<T>> Fn, PolicyWrapper<T> Wrapper) WrapDelegateIfNeed<T>(Func<CancellationToken, Task<T>> fn, CancellationToken token, bool configureAwait)
		{
			if (_policyWrapperFactory == null)
			{
				return (fn, null);
			}
			else
			{
				if (fn == null)
					return (null, null);

				var wrapper = _policyWrapperFactory.CreateWrapper(fn, token, configureAwait);
				return (wrapper.HandleAsync, wrapper);
			}
		}

		public string PolicyName
		{
			get { return _policyName ?? GetType().Name; }
			internal set { _policyName = value; }
		}

		public IPolicyProcessor PolicyProcessor { get; }

		internal enum HandlerRunnerSyncType
		{
			None = 0,
			Sync,
			Async,
			Misc
		}
	}
}
