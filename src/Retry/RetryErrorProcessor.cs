using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class RetryErrorProcessor : InvokePolicyParams<RetryPolicy>
	{
		public static RetryErrorProcessor Default() => new RetryErrorProcessor();

		public static RetryErrorProcessor From(Action<Exception> onBeforeProcessError, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new RetryErrorProcessor() { _configureFunc = _action1(onBeforeProcessError, convertType) };
		}

		public static RetryErrorProcessor From(Action<Exception, CancellationToken> onBeforeProcessError)
		{
			return new RetryErrorProcessor() { _configureFunc = _action2(onBeforeProcessError) };
		}

		public static RetryErrorProcessor From(Func<Exception, Task> onBeforeProcessErrorAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new RetryErrorProcessor() { _configureFunc = _func1(onBeforeProcessErrorAsync, convertType) };
		}

		public static RetryErrorProcessor From(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync)
		{
			return new RetryErrorProcessor() { _configureFunc = _func2(onBeforeProcessErrorAsync) };
		}

		public static implicit operator RetryErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError) => From(onBeforeProcessError);

		public static implicit operator RetryErrorProcessor(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => From(onBeforeProcessErrorAsync);
	}
}
