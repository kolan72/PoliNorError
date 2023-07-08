using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface INeedDelegateCollection
	{
		IPolicyDelegateCollection AndDelegate(Action action);
		IPolicyDelegateCollection AndDelegate(Func<CancellationToken, Task> func);
	}

	public interface INeedDelegateCollection<T>
	{
		IPolicyDelegateCollection<T> AndDelegate(Func<T> func);
		IPolicyDelegateCollection<T> AndDelegate(Func<CancellationToken, Task<T>> func);
	}
}
