using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public sealed partial class FallbackPolicyWithAction : IWithInnerErrorProcessor<FallbackPolicyWithAction>
	{
		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(actionProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor, cancellationType);
		}

		public new FallbackPolicyWithAction WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyWithAction, TException>(funcProcessor);
		}
	}
}
