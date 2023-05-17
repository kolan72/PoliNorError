using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract class InvokePolicyParams<T> where T : IPolicyBase
	{
		protected Func<T, T> _configureFunc = fb => fb;

		protected readonly static Func<Action<Exception>, ConvertToCancelableFuncType, Func<T, T>> _action1 = (onBPE, convertType) => (fb) => fb.WithErrorProcessorOf(onBPE, convertType);
		protected readonly static Func<Action<Exception, CancellationToken>, Func<T, T>> _action2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		protected readonly static Func<Func<Exception, Task>, ConvertToCancelableFuncType, Func<T, T>> _func1 = (onBPE, convertType) => fb => fb.WithErrorProcessorOf(onBPE, convertType);
		protected readonly static Func<Func<Exception, CancellationToken, Task>, Func<T, T>> _func2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);

		internal T ConfigurePolicy(T fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}

		private protected InvokePolicyParams() { }
	}
}
