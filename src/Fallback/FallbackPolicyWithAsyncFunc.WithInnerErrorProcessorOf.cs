using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicyWithAsyncFunc : IWithInnerErrorProcessor<FallbackPolicyWithAsyncFunc>
	{
		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAsyncFunc WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAsyncFunc, TException>(funcProcessor);
		}
	}
}
