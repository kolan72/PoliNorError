using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal sealed class SingleDelegateContainer<T> : SingleDelegateContainerBase
	{
		private SingleDelegateContainer(){ }

		public static SingleDelegateContainer<T> FromSync(Func<T> execute)
		{
			return new SingleDelegateContainer<T>
			{
				Execute = execute,
				UseSync = SyncPolicyDelegateType.Sync
			};
		}

		public static SingleDelegateContainer<T> FromNotSync(Func<CancellationToken, Task<T>> executeAsync)
		{
			return new SingleDelegateContainer<T>
			{
				ExecuteAsync = executeAsync,
				UseSync = SyncPolicyDelegateType.Async
			};
		}

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

		public Func<CancellationToken, Task<T>> ExecuteAsync { get; private set; }
		public Func<T> Execute { get; private set; }
	}
}
