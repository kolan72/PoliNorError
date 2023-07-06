using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface IPolicyNeedDelegateCollection
	{
		PolicyDelegateCollection AndDelegate(Action action);
		PolicyDelegateCollection AndDelegate(Func<CancellationToken, Task> func);
	}

	public interface IPolicyNeedDelegateCollection<T>
	{
		PolicyDelegateCollection<T> AndDelegate(Func<T> func);
		PolicyDelegateCollection<T> AndDelegate(Func<CancellationToken, Task<T>> func);
	}
}
