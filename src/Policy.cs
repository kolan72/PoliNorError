using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Represents the abstract base class for all policies in the PoliNorError library.
	/// </summary>
	public abstract class Policy
	{
		protected string _policyName;

		private PolicyWrapperFactory _policyWrapperFactory;

		private readonly IPolicyResultHandlerCollection _policyResultHandlerCollection;

		/// <summary>
		/// Initializes a new instance of the <see cref="Policy"/> class with the specified policy processor.
		/// </summary>
		/// <param name="policyProcessor">The policy processor to use for this policy.</param>
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
			return _policyResultHandlerCollection.Handle(policyRetryResult, token);
		}

		protected PolicyResult HandlePolicyResult(PolicyResult policyRetryResult, CancellationToken token)
		{
			return _policyResultHandlerCollection.Handle(policyRetryResult, token);
		}

		protected async Task<PolicyResult> HandlePolicyResultAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			return await _policyResultHandlerCollection.HandleAsync(policyRetryResult, configureAwait, token).ConfigureAwait(configureAwait);
		}

		protected async Task<PolicyResult<T>> HandlePolicyResultAsync<T>(PolicyResult<T> policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			return await _policyResultHandlerCollection.HandleAsync(policyRetryResult, configureAwait, token).ConfigureAwait(configureAwait);
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

		/// <summary>
		/// Gets a value indicating whether this policy has a wrapped policy factory.
		/// </summary>
		protected bool HasPolicyWrapperFactory => _policyWrapperFactory != null;

		/// <summary>
		/// Resets the wrapped policy by clearing the policy wrapper factory.
		/// </summary>
		public void ResetWrap()
		{
			_policyWrapperFactory = null;
		}

		/// <summary>
		/// Gets or sets the name of this policy. If not set, returns the type name of the policy.
		/// </summary>
		public string PolicyName
		{
			get { return _policyName ?? GetType().Name; }
			internal set { _policyName = value; }
		}

		/// <summary>
		/// Gets the policy processor associated with this policy.
		/// </summary>
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
