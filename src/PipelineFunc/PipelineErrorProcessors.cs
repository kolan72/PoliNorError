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
		/// Adds an <see cref="IErrorProcessor"/> instance to the collection.
		/// </summary>
		/// <param name="errorProcessor">The error processor to add.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> Add(IErrorProcessor errorProcessor)
		{
			_processors.Add(errorProcessor);
			return this;
		}

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
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// inner exceptions of the specified type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException> actionProcessor)
			where TException : Exception
		{
			var converted = actionProcessor.ToActionForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// inner exceptions of the specified type and receives a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException, CancellationToken> actionProcessor)
			where TException : Exception
		{
			var converted = actionProcessor.ToActionForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// inner exceptions of the specified type, with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="actionCancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException> actionProcessor, CancellationType actionCancellationType)
			where TException : Exception
		{
			var converted = actionProcessor.ToActionForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc, actionCancellationType);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, Task> funcProcessor)
			where TException : Exception
		{
			var converted = funcProcessor.ToFuncForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type and receives a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, CancellationToken, Task> funcProcessor)
			where TException : Exception
		{
			var converted = funcProcessor.ToFuncForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type, with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="funcCancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, Task> funcProcessor, CancellationType funcCancellationType)
			where TException : Exception
		{
			var converted = funcProcessor.ToFuncForInnerException();
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc, funcCancellationType);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// inner exceptions of the specified type and receives the typed processing error info.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException, ProcessingErrorInfo<TContext>> actionProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
			{
				if (ConvertExceptionDelegates.ToInnerException(ex, out TException inner))
					actionProcessor(inner, info);
			}));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// inner exceptions of the specified type and receives the typed processing error info and a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException, ProcessingErrorInfo<TContext>, CancellationToken> actionProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info, token) =>
			{
				if (ConvertExceptionDelegates.ToInnerException(ex, out TException inner))
					actionProcessor(inner, info, token);
			}));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// inner exceptions of the specified type and receives the typed processing error info,
		/// with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Action<TException, ProcessingErrorInfo<TContext>> actionProcessor, CancellationType cancellationType)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
			{
				if (ConvertExceptionDelegates.ToInnerException(ex, out TException inner))
					actionProcessor(inner, info);
			}, cancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type and receives the typed processing error info.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, ProcessingErrorInfo<TContext>, Task> funcProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
				ConvertExceptionDelegates.ToInnerException(ex, out TException inner)
					? funcProcessor(inner, info)
					: Task.CompletedTask));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type and receives the typed processing error info and a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, ProcessingErrorInfo<TContext>, CancellationToken, Task> funcProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info, token) =>
				ConvertExceptionDelegates.ToInnerException(ex, out TException inner)
					? funcProcessor(inner, info, token)
					: Task.CompletedTask));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// inner exceptions of the specified type and receives the typed processing error info,
		/// with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The type of inner exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForInnerException<TException>(Func<TException, ProcessingErrorInfo<TContext>, Task> funcProcessor, CancellationType cancellationType)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
				ConvertExceptionDelegates.ToInnerException(ex, out TException inner)
					? funcProcessor(inner, info)
					: Task.CompletedTask, cancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// exceptions of the exact specified type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException> actionProcessor)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// exceptions of the exact specified type and receives a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException, CancellationToken> actionProcessor)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from a synchronous action that processes
		/// exceptions of the exact specified type, with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="actionCancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException> actionProcessor, CancellationType actionCancellationType)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc, actionCancellationType);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, Task> funcProcessor)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type and receives a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, CancellationToken, Task> funcProcessor)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type, with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="funcCancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, Task> funcProcessor, CancellationType funcCancellationType)
			where TException : Exception
		{
			var converted = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.TryAsExact);
			var errorProcessorFunc = converted.ToErrorProcessorFunc();
			var processor = new DefaultErrorProcessor(errorProcessorFunc, funcCancellationType);
			_processors.Add(processor);
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// exceptions of the exact specified type and receives the typed processing error info.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException, ProcessingErrorInfo<TContext>> actionProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
			{
				if (ConvertExceptionDelegates.TryAsExact(ex, out TException typedException))
					actionProcessor(typedException, info);
			}));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// exceptions of the exact specified type and receives the typed processing error info and a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException, ProcessingErrorInfo<TContext>, CancellationToken> actionProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info, token) =>
			{
				if (ConvertExceptionDelegates.TryAsExact(ex, out TException typedException))
					actionProcessor(typedException, info, token);
			}));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from a synchronous action that processes
		/// exceptions of the exact specified type and receives the typed processing error info,
		/// with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="actionProcessor">The action to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the action.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Action<TException, ProcessingErrorInfo<TContext>> actionProcessor, CancellationType cancellationType)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
			{
				if (ConvertExceptionDelegates.TryAsExact(ex, out TException typedException))
					actionProcessor(typedException, info);
			}, cancellationType));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type and receives the typed processing error info.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, ProcessingErrorInfo<TContext>, Task> funcProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
				ConvertExceptionDelegates.TryAsExact(ex, out TException typedException)
					? funcProcessor(typedException, info)
					: Task.CompletedTask));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type and receives the typed processing error info and a cancellation token.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, ProcessingErrorInfo<TContext>, CancellationToken, Task> funcProcessor)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info, token) =>
				ConvertExceptionDelegates.TryAsExact(ex, out TException typedException)
					? funcProcessor(typedException, info, token)
					: Task.CompletedTask));
			return this;
		}

		/// <summary>
		/// Adds a <see cref="DefaultErrorProcessor{TParam}"/> built from an asynchronous function that processes
		/// exceptions of the exact specified type and receives the typed processing error info,
		/// with a specified cancellation type.
		/// </summary>
		/// <typeparam name="TException">The exact type of exception to process.</typeparam>
		/// <param name="funcProcessor">The asynchronous function to execute when an exception of type <typeparamref name="TException"/> occurs.</param>
		/// <param name="cancellationType">Specifies how cancellation is handled for the function.</param>
		/// <returns>The current collection instance.</returns>
		public PipelineErrorProcessors<TContext> AddForException<TException>(Func<TException, ProcessingErrorInfo<TContext>, Task> funcProcessor, CancellationType cancellationType)
			where TException : Exception
		{
			_processors.Add(new DefaultErrorProcessor<TContext>((ex, info) =>
				ConvertExceptionDelegates.TryAsExact(ex, out TException typedException)
					? funcProcessor(typedException, info)
					: Task.CompletedTask, cancellationType));
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
