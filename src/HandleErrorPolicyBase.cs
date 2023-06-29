using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class HandleErrorPolicyBase
	{
		protected string _policyName;

		internal IPolicyBase _wrappedPolicy;

		private readonly List<IHandlerRunner> _handlers;

		private protected HandleErrorPolicyBase(IPolicyProcessor policyProcessor)
		{
			_handlers = new List<IHandlerRunner>();
			PolicyProcessor = policyProcessor;
		}

		internal void AddAsyncHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func, _handlers.Count);
			_handlers.Add(handler);
		}

		internal void AddSyncHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act, _handlers.Count);
			_handlers.Add(handler);
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

		internal DefaultErrorProcessor GetDefaultErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => new DefaultErrorProcessor(onBeforeProcessError, onBeforeProcessErrorAsync);

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
