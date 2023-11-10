using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class PolicyCollectionErrorProcessorRegistration
	{
		private static readonly Action<ICanAddErrorProcessor, IErrorProcessor> _addErrorProcessorAction = (pr, erPr) => ((PolicyCollection)pr).LastOrDefault()?.PolicyProcessor.AddErrorProcessor(erPr);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, CancellationToken> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessorOf(this PolicyCollection policyCollection, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
						=> policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType, _addErrorProcessorAction);

		public static PolicyCollection WithErrorProcessor(this PolicyCollection policyCollection, IErrorProcessor errorProcessor)
						=> policyCollection.WithErrorProcessor(errorProcessor, _addErrorProcessorAction);
	}
}
