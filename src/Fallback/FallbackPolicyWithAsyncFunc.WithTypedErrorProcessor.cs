using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicyWithAsyncFunc
	{
		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<FallbackPolicyWithAsyncFunc, TException>(errorProcessor);
		}
	}
}
