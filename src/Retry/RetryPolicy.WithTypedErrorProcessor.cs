using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class RetryPolicy
	{
		public RetryPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		public RetryPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(actionProcessor);
		}

		public RetryPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(actionProcessor, cancellationType);
		}

		public RetryPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		public RetryPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(funcProcessor, cancellationType);
		}

		public RetryPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<RetryPolicy, TException>(funcProcessor);
		}

		public RetryPolicy WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<RetryPolicy, TException>(errorProcessor);
		}
	}
}