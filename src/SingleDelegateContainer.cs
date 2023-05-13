using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SingleDelegateContainer : SingleDelegateContainerBase
	{
		private SingleDelegateContainer(){}

		public static SingleDelegateContainer FromSync(Action execute) => new SingleDelegateContainer
		{
			Execute = execute,
			UseSync = SyncPolicyDelegateType.Sync
		};

		public static SingleDelegateContainer FromNotSync(Func<CancellationToken, Task> executeAsync) => new SingleDelegateContainer
		{
			ExecuteAsync = executeAsync,
			UseSync = SyncPolicyDelegateType.Async
		};

		public void ClearDelegate()
		{
			if (UseSync == SyncPolicyDelegateType.Async)
			{
				ExecuteAsync = null;
			}
			else
			{
				Execute = null;
			}
			UseSync = SyncPolicyDelegateType.None;
		}

		public Func<CancellationToken, Task> ExecuteAsync { get; private set; }
		public Action Execute { get; private set; }
	}
}
