using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class ErrorProcessors : IEnumerable<DefaultErrorProcessor>
	{
		private readonly List<DefaultErrorProcessor> _processors = new List<DefaultErrorProcessor>();

		public ErrorProcessors Add(Action<Exception> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc()));
			return this;
		}

		public ErrorProcessors Add(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc(), actionCancellationType));
			return this;
		}

		public ErrorProcessors Add(Action<Exception, CancellationToken> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc()));
			return this;
		}

		public ErrorProcessors Add(Func<Exception, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc()));
			return this;
		}

		public ErrorProcessors Add(Func<Exception, Task> funcProcessor, CancellationType actionCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc(), actionCancellationType));
			return this;
		}

		public ErrorProcessors Add(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc()));
			return this;
		}

		public ErrorProcessors AddWithInfo(Action<Exception, ProcessingErrorInfo> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor));
			return this;
		}

		public ErrorProcessors AddWithInfo(Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor));
			return this;
		}

		public ErrorProcessors AddWithInfo(Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType actionCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor, actionCancellationType));
			return this;
		}

		public ErrorProcessors AddWithInfo(Func<Exception, ProcessingErrorInfo, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor));
			return this;
		}

		public ErrorProcessors AddWithInfo(Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor));
			return this;
		}

		public ErrorProcessors AddWithInfo(Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType funcCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor, funcCancellationType));
			return this;
		}

		public int Count => _processors.Count;

		public IEnumerator<DefaultErrorProcessor> GetEnumerator() => _processors.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
