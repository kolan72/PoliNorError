using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError.Tests
{
	public interface IErrorProcessorRegistration
	{
		void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor);
		void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor);
		void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor);
		void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Action<Exception> actionProcessor);
		void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor);
		void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor);
		void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, Task> funcProcessor);
		void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor);
		void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType);
		void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType);
		void WithErrorProcessor(IErrorProcessor errorProcessor);
		int Count { get; }
	}

	public class CatchBlockHandlerErrorProcessorRegistration : IErrorProcessorRegistration
	{
		private readonly CatchBlockHandler _handler = CatchBlockHandler.ForAllExceptions();

		public int Count => _handler.BulkErrorProcessor.Count();

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _handler.WithErrorProcessor(new DefaultErrorProcessor());

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_handler.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_handler.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_handler.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_handler.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_handler.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}

	public class BulkErrorProcessorErrorProcessorRegistration : IErrorProcessorRegistration
	{
		private readonly BulkErrorProcessor _processor = new BulkErrorProcessor(PolicyAlias.Simple);
		public int Count => _processor.Count();

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _processor.WithErrorProcessor(new DefaultErrorProcessor());

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}

	public class PolicyProcessorErrorProcessorRegistration : IErrorProcessorRegistration
	{
		private readonly SimplePolicyProcessor _processor = new SimplePolicyProcessor();
		public int Count => _processor.Count();

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _processor.WithErrorProcessor(errorProcessor);

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_processor.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}

	public class PolicyDelegateCollectionErrorProcessorRegistration : IErrorProcessorRegistration
	{
		private readonly IPolicyDelegateCollection _policyDelegateCollection;

		public PolicyDelegateCollectionErrorProcessorRegistration(bool empty = false)
		{
			_policyDelegateCollection = PolicyDelegateCollection.Create();
			if (!empty)
			{
				_policyDelegateCollection = _policyDelegateCollection.WithSimple().AndDelegate(() => Expression.Empty()).WithSimple().AndDelegate(() => Expression.Empty());
			}
		}

		public int Count => _policyDelegateCollection.LastOrDefault()?.Policy.PolicyProcessor.Count() ?? 0;

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _policyDelegateCollection.WithErrorProcessor(errorProcessor);

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}

	public class PolicyDelegateCollectionErrorProcessorRegistration<T> : IErrorProcessorRegistration
	{
		private readonly IPolicyDelegateCollection<T> _policyDelegateCollection;

		public PolicyDelegateCollectionErrorProcessorRegistration(bool empty = false)
		{
			_policyDelegateCollection = PolicyDelegateCollection<T>.Create();
			if (!empty)
			{
				_policyDelegateCollection = _policyDelegateCollection.WithSimple().AndDelegate(() => default).WithSimple().AndDelegate(() => default);
			}
		}

		public int Count => _policyDelegateCollection.LastOrDefault()?.Policy.PolicyProcessor.Count() ?? 0;

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _policyDelegateCollection.WithErrorProcessor(errorProcessor);

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyDelegateCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}

	public class PolicyCollectionErrorProcessorRegistration : IErrorProcessorRegistration
	{
		private readonly PolicyCollection _policyCollection;

		public PolicyCollectionErrorProcessorRegistration(bool empty = false)
		{
			_policyCollection = PolicyCollection.Create();
			if (!empty)
			{
				_policyCollection = _policyCollection.WithSimple().WithSimple();
			}
		}

		public int Count => _policyCollection.LastOrDefault()?.PolicyProcessor.Count() ?? 0;

		public void WithErrorProcessor(IErrorProcessor errorProcessor) => _policyCollection.WithErrorProcessor(errorProcessor);

		public void WithErrorProcessorOf(Action<Exception, CancellationToken> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor);
		}

		public void WithErrorProcessorOf(Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, actionProcessor, cancellationType);
		}

		public void WithErrorProcessorOf(Func<Exception, Task> funcProcessor, CancellationType cancellationType)
		{
			_policyCollection.WithErrorProcessorOf(funcProcessor, cancellationType);
		}
	}
}
