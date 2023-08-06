using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class ErrorProcessorDelegate
	{
		public static ErrorProcessorDelegate Default() => new ErrorProcessorDelegate();

		public static ErrorProcessorDelegate From(Action<Exception> onBeforeProcessError, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _action1(onBeforeProcessError, convertType) };
		}

		public static ErrorProcessorDelegate From(Action<Exception, CancellationToken> onBeforeProcessError)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _action2(onBeforeProcessError) };
		}

		public static ErrorProcessorDelegate From(Func<Exception, Task> onBeforeProcessErrorAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _func1(onBeforeProcessErrorAsync, convertType) };
		}

		public static ErrorProcessorDelegate From(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _func2(onBeforeProcessErrorAsync) };
		}

		public static implicit operator ErrorProcessorDelegate(Action<Exception, CancellationToken> onBeforeProcessError) => From(onBeforeProcessError);

		public static implicit operator ErrorProcessorDelegate(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => From(onBeforeProcessErrorAsync);

		private Func<IPolicyBase, IPolicyBase> _configureFunc = fb => fb;

		private readonly static Func<Action<Exception>, ConvertToCancelableFuncType, Func<IPolicyBase, IPolicyBase>> _action1 = (onBPE, convertType) => (fb) => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Action<Exception, CancellationToken>, Func<IPolicyBase, IPolicyBase>> _action2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Func<Exception, Task>, ConvertToCancelableFuncType, Func<IPolicyBase, IPolicyBase>> _func1 = (onBPE, convertType) => fb => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Func<Exception, CancellationToken, Task>, Func<IPolicyBase, IPolicyBase>> _func2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);

		internal IPolicyBase ConfigurePolicy(IPolicyBase fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}

		private protected ErrorProcessorDelegate() { }
	}
}
