using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public partial class FallbackPolicyBase : IWithInnerErrorProcessor<FallbackPolicyBase>
	{
		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor, cancellationType);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor, cancellationType);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor, cancellationType);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor, cancellationType);
		}

		public FallbackPolicyBase WithInnerErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
		{
			return this.WithInnerErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
		}
	}
}
