using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class FallbackErrorProcessor : InvokePolicyParams<FallbackPolicy>
	{
		public static FallbackErrorProcessor Default() => new FallbackErrorProcessor();

		public static FallbackErrorProcessor From(Action<Exception> onBeforeProcessError, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackErrorProcessor() { _configureFunc = _action1(onBeforeProcessError, convertType) };
		}

		public static FallbackErrorProcessor From(Action<Exception, CancellationToken> onBeforeProcessError)
		{
			return new FallbackErrorProcessor() { _configureFunc = _action2(onBeforeProcessError) };
		}

		public static FallbackErrorProcessor From(Func<Exception, Task> onBeforeProcessErrorAsync, ConvertToCancelableFuncType convertType = ConvertToCancelableFuncType.Precancelable)
		{
			return new FallbackErrorProcessor() { _configureFunc = _func1(onBeforeProcessErrorAsync, convertType) };
		}

		public static FallbackErrorProcessor From(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync)
		{
			return new FallbackErrorProcessor() { _configureFunc = _func2(onBeforeProcessErrorAsync) };
		}

		public static implicit operator FallbackErrorProcessor(Action<Exception, CancellationToken> onBeforeProcessError) => From(onBeforeProcessError);

		public static implicit operator FallbackErrorProcessor(Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) => From(onBeforeProcessErrorAsync);
	}
}
