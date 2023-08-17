using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class ErrorProcessorDelegate
	{
		public static ErrorProcessorDelegate Default() => new ErrorProcessorDelegate();

		public static ErrorProcessorDelegate From(Action<Exception> funcProcessor, CancellationType convertType = CancellationType.Precancelable)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _action1(funcProcessor, convertType) };
		}

		public static ErrorProcessorDelegate From(Action<Exception, CancellationToken> funcProcessor)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _action2(funcProcessor) };
		}

		public static ErrorProcessorDelegate From(Func<Exception, Task> funcProcessorAsync, CancellationType convertType = CancellationType.Precancelable)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _func1(funcProcessorAsync, convertType) };
		}

		public static ErrorProcessorDelegate From(Func<Exception, CancellationToken, Task> funcProcessorAsync)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _func2(funcProcessorAsync) };
		}

		public static ErrorProcessorDelegate From(IErrorProcessor errorProcessor)
		{
			return new ErrorProcessorDelegate() { _configureFunc = _funcErrorProcessor(errorProcessor) };
		}

		public static implicit operator ErrorProcessorDelegate(Action<Exception, CancellationToken> funcProcessor) => From(funcProcessor);

		public static implicit operator ErrorProcessorDelegate(Func<Exception, CancellationToken, Task> funcProcessorAsync) => From(funcProcessorAsync);

		public static implicit operator ErrorProcessorDelegate(DefaultErrorProcessor errorProcessor) => From(errorProcessor);

		private Func<IPolicyBase, IPolicyBase> _configureFunc = fb => fb;

		private readonly static Func<Action<Exception>, CancellationType, Func<IPolicyBase, IPolicyBase>> _action1 = (onBPE, convertType) => (fb) => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Action<Exception, CancellationToken>, Func<IPolicyBase, IPolicyBase>> _action2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Func<Exception, Task>, CancellationType, Func<IPolicyBase, IPolicyBase>> _func1 = (onBPE, convertType) => fb => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Func<Exception, CancellationToken, Task>, Func<IPolicyBase, IPolicyBase>> _func2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);

		private readonly static Func<IErrorProcessor, Func<IPolicyBase, IPolicyBase>> _funcErrorProcessor = (onBPE) => (fb) => fb.WithErrorProcessor(onBPE);

		internal IPolicyBase ConfigurePolicy(IPolicyBase fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}

		private protected ErrorProcessorDelegate() { }
	}
}
