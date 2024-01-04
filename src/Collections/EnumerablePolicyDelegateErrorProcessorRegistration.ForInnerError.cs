using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static partial class EnumerablePolicyDelegateErrorProcessorRegistration
	{
		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		internal static T WithInnerErrorProcessorOf<T, TException>(this T policyDelegateCollection, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor) where T : IEnumerable<PolicyDelegateBase>, ICanAddErrorProcessor where TException : Exception
				=> policyDelegateCollection.WithInnerErrorProcessorOf(funcProcessor, _addErrorProcessorAction);
	}
}
