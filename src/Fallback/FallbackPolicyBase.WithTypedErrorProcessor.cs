using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
    public partial class FallbackPolicyBase : IWithInnerErrorProcessor<FallbackPolicyBase>
    {
		public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
		}

		public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
        {
            return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor);
        }

        public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
        {
            return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(actionProcessor, cancellationType);
        }

        public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
        {
            return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
        }

        public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
        {
            return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor, cancellationType);
        }

        public FallbackPolicyBase WithTypedErrorProcessorOf<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
        {
            return this.WithTypedErrorProcessorOf<FallbackPolicyBase, TException>(funcProcessor);
        }

        public FallbackPolicyBase WithTypedErrorProcessor<TException>(DefaultTypedErrorProcessor<TException> errorProcessor) where TException : Exception
        {
            return this.WithTypedErrorProcessor<FallbackPolicyBase, TException>(errorProcessor);
        }
    }
}