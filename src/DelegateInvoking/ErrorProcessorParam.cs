using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class ErrorProcessorParam
	{
		internal static ErrorProcessorParam Default() => new ErrorProcessorParam();

		public static ErrorProcessorParam From(Action<Exception> funcProcessor)
		{
			return new ErrorProcessorParam() { _configureFunc = _action0(funcProcessor) };
		}

		public static ErrorProcessorParam From(Action<Exception> funcProcessor, CancellationType convertType)
		{
			return new ErrorProcessorParam() { _configureFunc = _action1(funcProcessor, convertType) };
		}

		public static ErrorProcessorParam From(Action<Exception, CancellationToken> funcProcessor)
		{
			return new ErrorProcessorParam() { _configureFunc = _action2(funcProcessor) };
		}

		public static ErrorProcessorParam From(Func<Exception, Task> funcProcessorAsync)
		{
			return new ErrorProcessorParam() { _configureFunc = _func0(funcProcessorAsync) };
		}

		public static ErrorProcessorParam From(Func<Exception, Task> funcProcessorAsync, CancellationType convertType)
		{
			return new ErrorProcessorParam() { _configureFunc = _func1(funcProcessorAsync, convertType) };
		}

		public static ErrorProcessorParam From(Func<Exception, CancellationToken, Task> funcProcessorAsync)
		{
			return new ErrorProcessorParam() { _configureFunc = _func2(funcProcessorAsync) };
		}

		public static ErrorProcessorParam From(IErrorProcessor errorProcessor)
		{
			return new ErrorProcessorParam() { _configureFunc = _funcErrorProcessor(errorProcessor) };
		}

		public static implicit operator ErrorProcessorParam(Action<Exception> actProcessor) => From(actProcessor);

		public static implicit operator ErrorProcessorParam(Action<Exception, CancellationToken> funcProcessor) => From(funcProcessor);

		public static implicit operator ErrorProcessorParam(Func<Exception, Task> funcProcessorAsync) => From(funcProcessorAsync);

		public static implicit operator ErrorProcessorParam(Func<Exception, CancellationToken, Task> funcProcessorAsync) => From(funcProcessorAsync);

		public static implicit operator ErrorProcessorParam(BasicErrorProcessor errorProcessor) => From(errorProcessor);

		public static implicit operator ErrorProcessorParam(DefaultErrorProcessor errorProcessor) => From(errorProcessor);

		private Func<IPolicyBase, IPolicyBase> _configureFunc = fb => fb;

		private readonly static Func<Action<Exception>, Func<IPolicyBase, IPolicyBase>> _action0 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Action<Exception>, CancellationType, Func<IPolicyBase, IPolicyBase>> _action1 = (onBPE, convertType) => (fb) => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Action<Exception, CancellationToken>, Func<IPolicyBase, IPolicyBase>> _action2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Func<Exception, Task>, Func<IPolicyBase, IPolicyBase>> _func0 = (onBPE) => fb => fb.WithErrorProcessorOf(onBPE);
		private readonly static Func<Func<Exception, Task>, CancellationType, Func<IPolicyBase, IPolicyBase>> _func1 = (onBPE, convertType) => fb => fb.WithErrorProcessorOf(onBPE, convertType);
		private readonly static Func<Func<Exception, CancellationToken, Task>, Func<IPolicyBase, IPolicyBase>> _func2 = (onBPE) => (fb) => fb.WithErrorProcessorOf(onBPE);

		private readonly static Func<IErrorProcessor, Func<IPolicyBase, IPolicyBase>> _funcErrorProcessor = (onBPE) => (fb) => fb.WithErrorProcessor(onBPE);

		internal IPolicyBase ConfigurePolicy(IPolicyBase fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}

		private protected ErrorProcessorParam() { }
	}

	internal static class PolicyErrorProcessorExtensions
	{
		public static ErrorProcessorParam GetValueOrDefault(this ErrorProcessorParam policyErrorProcessor) => policyErrorProcessor ?? ErrorProcessorParam.Default();
	}
}
