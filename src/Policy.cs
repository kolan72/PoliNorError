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

		private readonly PolicyResultHandlerCollection _policyResultHandlerCollection;

		protected Policy(IPolicyProcessor policyProcessor)
		{
			_policyResultHandlerCollection = new PolicyResultHandlerCollection();
			PolicyProcessor = policyProcessor;
		}

		internal void AddAsyncHandler(Func<PolicyResult, Task> func)
		{
			_policyResultHandlerCollection.AddHandler(func);
		}

		internal void AddAsyncHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			_policyResultHandlerCollection.AddHandler(func);
		}

		internal void AddAsyncHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			_policyResultHandlerCollection.AddHandler(func);
		}

		internal void AddAsyncHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			_policyResultHandlerCollection.AddHandler(func);
		}

		internal void AddSyncHandler(Action<PolicyResult> act)
		{
			_policyResultHandlerCollection.AddHandler(act);
		}

		internal void AddSyncHandler(Action<PolicyResult, CancellationToken> act)
		{
			_policyResultHandlerCollection.AddHandler(act);
		}

		internal void AddSyncHandler<T>(Action<PolicyResult<T>> act)
		{
			_policyResultHandlerCollection.AddHandler(act);
		}

		internal void AddSyncHandler<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			_policyResultHandlerCollection.AddHandler(act);
		}

		protected PolicyResult<T> HandlePolicyResult<T>(PolicyResult<T> policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(_policyResultHandlerCollection.GenericHandlers, token);
		}

		protected PolicyResult HandlePolicyResult(PolicyResult policyRetryResult, CancellationToken token)
		{
			return policyRetryResult.HandleResultForceSync(_policyResultHandlerCollection.Handlers, token);
		}

		protected async Task HandlePolicyResultAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (_policyResultHandlerCollection.Handlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					policyRetryResult.HandleResultSync(_policyResultHandlerCollection.Handlers, token);
					break;
				case HandlerRunnerSyncType.Misc:
					await policyRetryResult.HandleResultMisc(_policyResultHandlerCollection.Handlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.Async:
					await policyRetryResult.HandleResultAsync(_policyResultHandlerCollection.Handlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.None:
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected async Task HandlePolicyResultAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			switch (_policyResultHandlerCollection.GenericHandlers.MapToSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					policyRetryResult.HandleResultSync(_policyResultHandlerCollection.GenericHandlers, token);
					break;
				case HandlerRunnerSyncType.Misc:
					await policyRetryResult.HandleResultMisc(_policyResultHandlerCollection.GenericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.Async:
					await policyRetryResult.HandleResultAsync(_policyResultHandlerCollection.GenericHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.None:
					break;
				default:
					throw new NotImplementedException();
			}
		}

		internal void SetWrap(IPolicyBase policyToWrap)
		{
			if (HasPolicyWrapperFactory)
			{
				throw new NotImplementedException("More than one wrapped policy is not supported.");
			}
			_policyWrapperFactory = new PolicyWrapperFactory(policyToWrap);
		}

		internal void SetWrap(IEnumerable<IPolicyBase> policies, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			if (HasPolicyWrapperFactory)
			{
				throw new ArgumentException("More than one wrapped PolicyCollection is not supported.");
			}
			if (throwOnWrappedCollectionFailed == ThrowOnWrappedCollectionFailed.None)
			{
				throw new ArgumentException($"Value must be {nameof(ThrowOnWrappedCollectionFailed.LastError)} or {nameof(ThrowOnWrappedCollectionFailed.CollectionError)}.");
			}
			_policyWrapperFactory = new PolicyWrapperFactory(policies, throwOnWrappedCollectionFailed);
		}

		internal (Action Act, PolicyWrapper Wrapper) WrapDelegateIfNeed(Action action, CancellationToken token)
		{
			if (!HasPolicyWrapperFactory)
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
			if (!HasPolicyWrapperFactory)
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
			if (!HasPolicyWrapperFactory)
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
			if (!HasPolicyWrapperFactory)
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

		protected bool HasPolicyWrapperFactory => _policyWrapperFactory != null;

		public void ResetWrap()
		{
			_policyWrapperFactory = null;
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
