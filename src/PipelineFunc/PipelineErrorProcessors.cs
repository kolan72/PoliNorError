using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Represents a generic collection of <see cref="DefaultErrorProcessor"/> and
	/// <see cref="DefaultErrorProcessor{TParam}"/> instances for pipeline configuration,
	/// providing fluent factory methods that mirror the constructors of
	/// <see cref="DefaultErrorProcessor"/> and <see cref="DefaultErrorProcessor{TParam}"/>.
	/// </summary>
	/// <typeparam name="TContext">The type of the parameter carried by <see cref="ProcessingErrorInfo{TParam}"/>.</typeparam>
	public class PipelineErrorProcessors<TContext> : IEnumerable<IErrorProcessor>
	{
		private readonly List<IErrorProcessor> _processors = new List<IErrorProcessor>();

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that receives the exception.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc()));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that receives the exception,
		/// with a specified cancellation type.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <param name="actionCancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception> actionProcessor, CancellationType actionCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc(), actionCancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that receives the exception
		/// and a cancellation token.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception, CancellationToken> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(actionProcessor.ToErrorProcessorFunc()));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that receives the exception.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc()));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that receives the exception,
		/// with a specified cancellation type.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="actionCancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, Task> funcProcessor, CancellationType actionCancellationType)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc(), actionCancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that receives the exception
		/// and a cancellation token.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, CancellationToken, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor(funcProcessor.ToErrorProcessorFunc()));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that receives
		/// the exception and the typed processing error info.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(actionProcessor));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that receives
		/// the exception, the typed processing error info, and a cancellation token.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>, CancellationToken> actionProcessor)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(actionProcessor));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that receives
		/// the exception and the typed processing error info, with a specified cancellation type.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>> actionProcessor, CancellationType cancellationType)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(actionProcessor, cancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that receives
		/// the exception and the typed processing error info.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(funcProcessor));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that receives
		/// the exception, the typed processing error info, and a cancellation token.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, CancellationToken, Task> funcProcessor)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(funcProcessor));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that receives
		/// the exception and the typed processing error info, with a specified cancellation type.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an error occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(funcProcessor, cancellationType));
			return this;
		}

		/// <summary>
		/// Gets the number of processors currently in the collection.
		/// </summary>
		public int Count => _processors.Count;

		/// <inheritdoc/>
		public IEnumerator<IErrorProcessor> GetEnumerator() => _processors.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
