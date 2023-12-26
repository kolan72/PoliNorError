using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class SimplePolicyProcessorInnerErrorProcessorRegistration
	{
		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor simplePolicyProcessor, Action<TException, CancellationToken> actionProcessor) where TException : Exception
			=> simplePolicyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Action<TException> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor, cancellationType);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, Task> funcProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor, cancellationType);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, CancellationToken, Task> funcProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(actionProcessor, cancellationType);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor, cancellationType);

		public static ISimplePolicyProcessor WithInnerErrorProcessorOf<TException>(this ISimplePolicyProcessor policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where TException : Exception
			=> policyProcessor.WithInnerErrorProcessorOf<ISimplePolicyProcessor, TException>(funcProcessor);
	}
}
