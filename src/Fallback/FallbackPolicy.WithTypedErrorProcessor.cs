using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicy
	{
		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor);
		}

		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor);
		}

		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor);
		}

		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicy WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicy, TException>(funcProcessor);
		}

		public new FallbackPolicy WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<FallbackPolicy, TException>(errorProcessor);
		}
	}
}