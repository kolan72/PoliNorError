using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class RetryErrorSaverParam
	{
		public static RetryErrorSaverParam From(Action<Exception> funcProcessor)
		{
			return new RetryErrorSaverParam() { _configureFunc = _action0(funcProcessor) };
		}

		public static RetryErrorSaverParam From(Action<Exception> funcProcessor, CancellationType convertType)
		{
			return new RetryErrorSaverParam() { _configureFunc = _action1(funcProcessor, convertType) };
		}

		public static RetryErrorSaverParam From(Action<Exception, CancellationToken> funcProcessor)
		{
			return new RetryErrorSaverParam() { _configureFunc = _action2(funcProcessor) };
		}

		public static RetryErrorSaverParam From(Func<Exception, Task> funcProcessorAsync)
		{
			return new RetryErrorSaverParam() { _configureFunc = _func0(funcProcessorAsync) };
		}

		public static RetryErrorSaverParam From(Func<Exception, Task> funcProcessorAsync, CancellationType convertType)
		{
			return new RetryErrorSaverParam() { _configureFunc = _func1(funcProcessorAsync, convertType) };
		}

		public static RetryErrorSaverParam From(Func<Exception, CancellationToken, Task> funcProcessorAsync)
		{
			return new RetryErrorSaverParam() { _configureFunc = _func2(funcProcessorAsync) };
		}

		public static RetryErrorSaverParam From(IErrorProcessor errorProcessor)
		{
			return new RetryErrorSaverParam() { _configureFunc = _funcErrorProcessor(errorProcessor) };
		}

		public static implicit operator RetryErrorSaverParam(Action<Exception> actProcessor) => From(actProcessor);

		public static implicit operator RetryErrorSaverParam(Action<Exception, CancellationToken> funcProcessor) => From(funcProcessor);

		public static implicit operator RetryErrorSaverParam(Func<Exception, Task> funcProcessorAsync) => From(funcProcessorAsync);

		public static implicit operator RetryErrorSaverParam(Func<Exception, CancellationToken, Task> funcProcessorAsync) => From(funcProcessorAsync);

		public static implicit operator RetryErrorSaverParam(BasicErrorProcessor errorProcessor) => From(errorProcessor);

		private Func<RetryPolicy, RetryPolicy> _configureFunc = fb => fb;

		private readonly static Func<Action<Exception>, Func<RetryPolicy, RetryPolicy>> _action0 = (onBPE) => (fb) => fb.UseCustomErrorSaverOf(onBPE);
		private readonly static Func<Action<Exception>, CancellationType, Func<RetryPolicy, RetryPolicy>> _action1 = (onBPE, convertType) => (fb) => fb.UseCustomErrorSaverOf(onBPE, convertType);
		private readonly static Func<Action<Exception, CancellationToken>, Func<RetryPolicy, RetryPolicy>> _action2 = (onBPE) => (fb) => fb.UseCustomErrorSaverOf(onBPE);
		private readonly static Func<Func<Exception, Task>, Func<RetryPolicy, RetryPolicy>> _func0 = (onBPE) => fb => fb.UseCustomErrorSaverOf(onBPE);
		private readonly static Func<Func<Exception, Task>, CancellationType, Func<RetryPolicy, RetryPolicy>> _func1 = (onBPE, convertType) => fb => fb.UseCustomErrorSaverOf(onBPE, convertType);
		private readonly static Func<Func<Exception, CancellationToken, Task>, Func<RetryPolicy, RetryPolicy>> _func2 = (onBPE) => (fb) => fb.UseCustomErrorSaverOf(onBPE);

		private readonly static Func<IErrorProcessor, Func<RetryPolicy, RetryPolicy>> _funcErrorProcessor = (onBPE) => (fb) => fb.UseCustomErrorSaver(onBPE);

		internal RetryPolicy ConfigurePolicy(RetryPolicy fallbackPolicy)
		{
			return _configureFunc(fallbackPolicy);
		}
	}
}
