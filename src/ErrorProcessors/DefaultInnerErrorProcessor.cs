using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Error processor that processes exceptions only if the inner exception is of the specified type.
	/// </summary>
	/// <typeparam name="TException">The type of inner exception to process.</typeparam>
	public class DefaultInnerErrorProcessor<TException> : IErrorProcessor where TException : Exception
	{
		private readonly DefaultInnerErrorProcessorT<TException> _errorProcessor;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with a synchronous action processor.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		public DefaultInnerErrorProcessor(Action<TException, ProcessingErrorInfo> actionProcessor)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with a synchronous action processor that accepts a cancellation token.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		public DefaultInnerErrorProcessor(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(actionProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with a synchronous action processor and specified cancellation type.
		/// </summary>
		/// <param name="actionProcessor">The action to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		public DefaultInnerErrorProcessor(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(actionProcessor, cancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with an asynchronous function processor.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		public DefaultInnerErrorProcessor(Func<TException, ProcessingErrorInfo, Task> funcProcessor)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(funcProcessor);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with an asynchronous function processor and specified cancellation type.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		/// <param name="cancellationType">The type of cancellation handling.</param>
		public DefaultInnerErrorProcessor(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(funcProcessor, cancellationType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultInnerErrorProcessor{TException}"/> class with an asynchronous function processor that accepts a cancellation token.
		/// </summary>
		/// <param name="funcProcessor">The asynchronous function to execute when an inner exception of the specified type occurs.</param>
		public DefaultInnerErrorProcessor(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			_errorProcessor = DefaultInnerErrorProcessorT<TException>.Create(funcProcessor);
		}

		/// <summary>
		/// Processes the exception synchronously if its inner exception is of the specified type.
		/// </summary>
		/// <param name="error">The exception to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Optional processing error information.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The processed exception.</returns>
		public Exception Process(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, CancellationToken cancellationToken = default)
		{
			return _errorProcessor.Process(error, catchBlockProcessErrorInfo, cancellationToken);
		}

		/// <summary>
		/// Processes the exception asynchronously if its inner exception is of the specified type.
		/// </summary>
		/// <param name="error">The exception to process.</param>
		/// <param name="catchBlockProcessErrorInfo">Optional processing error information.</param>
		/// <param name="configAwait">Whether to configure await.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>A task that represents the asynchronous operation and contains the processed exception.</returns>
		public async Task<Exception> ProcessAsync(Exception error, ProcessingErrorInfo catchBlockProcessErrorInfo = null, bool configAwait = false, CancellationToken cancellationToken = default)
		{
			return await _errorProcessor.ProcessAsync(error, catchBlockProcessErrorInfo, configAwait, cancellationToken).ConfigureAwait(configAwait);
		}
	}

	internal class DefaultInnerErrorProcessorT<TException> : ErrorProcessorBase<ProcessingErrorInfo> where TException : Exception
	{
		public static DefaultInnerErrorProcessorT<TException> Create(Action<TException, ProcessingErrorInfo> actionProcessor)
		{
			var action = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultInnerErrorProcessorT<TException> Create(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor)
		{
			var action = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetSyncRunner(action);
			return res;
		}

		public static DefaultInnerErrorProcessorT<TException> Create(Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType)
		{
			var action = ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetSyncRunner(action, cancellationType);
			return res;
		}

		public static DefaultInnerErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, Task> funcProcessor)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetAsyncRunner(func);
			return res;
		}

		public static DefaultInnerErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetAsyncRunner(func, cancellationType);
			return res;
		}

		public static DefaultInnerErrorProcessorT<TException> Create(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor)
		{
			var func = ErrorProcessorFuncConverter.Convert(funcProcessor, ConvertExceptionDelegates.ToInnerException);

			var res = new DefaultInnerErrorProcessorT<TException>();
			res.SetAsyncRunner(func);
			return res;
		}

		protected override Func<ProcessingErrorInfo, ProcessingErrorInfo> ParameterConverter => (_) => _;
	}
}
