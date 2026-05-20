using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// A generic collection of <see cref="DefaultErrorProcessor{TParam}"/> instances
	/// with fluent factory methods that mirror each constructor of <see cref="DefaultErrorProcessor{TParam}"/>.
	/// </summary>
	/// <typeparam name="TContext">The type of the parameter carried by <see cref="ProcessingErrorInfo{TParam}"/>.</typeparam>
	public class ContextErrorProcessors<TContext> : IEnumerable<DefaultErrorProcessor<TContext>>
	{
		private readonly List<DefaultErrorProcessor<TContext>> _processors = new List<DefaultErrorProcessor<TContext>>();

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that receives
		/// the exception and the typed processing error info.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an error occurs.</param>
		/// <returns>The current collection instance.</returns>
		public ContextErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>> actionProcessor)
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
		public ContextErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>, CancellationToken> actionProcessor)
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
		public ContextErrorProcessors<TContext> Add(Action<Exception, ProcessingErrorInfo<TContext>> actionProcessor, CancellationType cancellationType)
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
		public ContextErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, Task> funcProcessor)
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
		public ContextErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, CancellationToken, Task> funcProcessor)
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
		public ContextErrorProcessors<TContext> Add(Func<Exception, ProcessingErrorInfo<TContext>, Task> funcProcessor, CancellationType cancellationType)
		{
			_processors.Add(new DefaultErrorProcessor<TContext>(funcProcessor, cancellationType));
			return this;
		}

		/// <summary>
		/// Gets the number of processors currently in the collection.
		/// </summary>
		public int Count => _processors.Count;

		/// <inheritdoc/>
		public IEnumerator<DefaultErrorProcessor<TContext>> GetEnumerator() => _processors.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
