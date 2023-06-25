using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class HandleErrorPolicyBase
	{
		private int _curNum;
		protected string _policyName;

		internal IPolicyBase _wrappedPolicy;

		private readonly List<IHandlerRunner> _policyResultHandlers;
		private readonly List<IHandlerRunner> _policyResultAsyncHandlers;
		private readonly HandlerRunnersCollection _handlerRunnersCollection;

		private protected HandleErrorPolicyBase(IPolicyProcessor policyProcessor)
		{
			_policyResultHandlers = new List<IHandlerRunner>();
			_policyResultAsyncHandlers = new List<IHandlerRunner>();
			_handlerRunnersCollection = HandlerRunnersCollection.FromSyncAndNotSync(_policyResultHandlers, _policyResultAsyncHandlers);
			PolicyProcessor = policyProcessor;
		}

		internal void AddAsyncHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func, ++_curNum);
			_policyResultAsyncHandlers.Add(handler);
		}

		internal void AddSyncHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act, ++_curNum);
			_policyResultHandlers.Add(handler);
		}

		protected PolicyResult HandlePolicyResult(PolicyResult policyRetryResult, CancellationToken token)
		{
			var sortedHandlers = GetOrderedHandlers();
			return policyRetryResult.HandleResultForceSync(sortedHandlers, token);
		}

		private IOrderedEnumerable<IHandlerRunner> GetOrderedHandlers() => _policyResultHandlers.Union(_policyResultAsyncHandlers).OrderBy(h => h.Num);

		protected async Task HandlePolicyResultAsync(PolicyResult policyRetryResult, bool configureAwait = false, CancellationToken token = default)
		{
			var sortedHandlers = GetOrderedHandlers();

			switch (GetHanlerRunnersSyncType())
			{
				case HandlerRunnerSyncType.Sync:
					policyRetryResult.HandleResultSync(sortedHandlers, token);
					break;
				case HandlerRunnerSyncType.Misc:
					await policyRetryResult.HandleResultMisc(sortedHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.Async:
					await policyRetryResult.HandleResultAsync(sortedHandlers, configureAwait, token).ConfigureAwait(configureAwait);
					break;
				case HandlerRunnerSyncType.None:
					break;
				default:
					throw new NotImplementedException();
			}
		}

		internal DefaultErrorProcessor GetDefaultErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => new DefaultErrorProcessor(onBeforeProcessError, onBeforeProcessErrorAsync);

		private HandlerRunnerSyncType GetHanlerRunnersSyncType()
		{
			return _handlerRunnersCollection.MapToSyncType();
		}

		public string PolicyName
		{
			get { return _policyName ?? GetType().Name; }
			internal set { _policyName = value; }
		}

		public IPolicyProcessor PolicyProcessor { get; }

		public enum HandlerRunnerSyncType
		{
			None = 0,
			Sync,
			Async,
			Misc
		}
	}
}
