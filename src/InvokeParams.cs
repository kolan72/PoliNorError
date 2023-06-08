using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class InvokeParams<T> where T : IPolicyBase
	{
		public static InvokeParams<T> Default() => new InvokeParams<T>();

		public static InvokeParams<T> From(Action<Exception> onBeforeProcessError, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new InvokeParams<T>() { _configureFunc = _action1(onBeforeProcessError, convertType) };
		}

		public static InvokeParams<T> From(Action<Exception, CancellationToken> onBeforeProcessError)
		{
			return new InvokeParams<T>() { _configureFunc = _action2(onBeforeProcessError) };
		}

		public static InvokeParams<T> From(Func<Exception, Task> onBeforeProcessErrorAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new InvokeParams<T>() { _configureFunc = _func1(onBeforeProcessErrorAsync, convertType) };
		}

		public static InvokeParams<T> From(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync)
		{
			return new InvokeParams<T>() { _configureFunc = _func2(onBeforeProcessErrorAsync) };
		}

		public static implicit operator InvokeParams<T>(Action<Exception, CancellationToken> onBeforeProcessError) => From(onBeforeProcessError);

		public static implicit operator InvokeParams<T>(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => From(onBeforeProcessErrorAsync);

		private Func<T, T> _configureFunc = fb => fb;

		private readonly static Func<Action<Exception>, ConvertToCancelableFuncType, Func<T, T>> _action1 = (onBPE, convertType) => (fb) => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Action<Exception, CancellationToken>, Func<T, T>> _action2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Func<Exception, Task>, ConvertToCancelableFuncType, Func<T, T>> _func1 = (onBPE, convertType) => fb => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Func<Exception, CancellationToken, Task>, Func<T, T>> _func2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);

		internal T ConfigurePolicy(T fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}

		private protected InvokeParams() { }
	}
}
