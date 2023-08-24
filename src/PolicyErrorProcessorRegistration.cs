using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Provides a set of static methods to add error processor to policy.
	/// </summary>
	public static class PolicyErrorProcessorRegistration
	{
		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(actionProcessor, cancellationType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor);
			return errorPolicyBase;
		}

		public static T WithErrorProcessorOf<T>(this T errorPolicyBase, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessorOf(funcProcessor, actionProcessor, convertToCancelableFuncType);
			return errorPolicyBase;
		}

		public static T WithErrorProcessor<T>(this T errorPolicyBase, IErrorProcessor errorProcessor) where T : IPolicyBase
		{
			errorPolicyBase.PolicyProcessor.WithErrorProcessor(errorProcessor);
			return errorPolicyBase;
		}
	}
}
