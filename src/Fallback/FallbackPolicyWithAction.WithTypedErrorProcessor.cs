using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicyWithAction
	{
		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAction WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessor<FallbackPolicyWithAction, TException>(errorProcessor);
		}
	}
}
