using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// Implements a processor that handles a collection of error processors in sequence.
	/// </summary>
	public partial class BulkErrorProcessor : IBulkErrorProcessor
	{
		private readonly List<IErrorProcessor> _errorProcessors = new List<IErrorProcessor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="BulkErrorProcessor"/> class.
		/// </summary>
		public BulkErrorProcessor()
		{
		}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete. Use parameterless constructor instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
#pragma warning disable RCS1163 // Unused parameter.
		public BulkErrorProcessor(PolicyAlias policyAlias)
		{
		}
#pragma warning restore RCS1163 // Unused parameter.

		/// <inheritdoc/>
		public void AddProcessor(IErrorProcessor errorProcessor)
		{
			_errorProcessors.Add(errorProcessor);
		}

		/// <inheritdoc/>
		public BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
		{
			var errors = new List<ErrorProcessorException>();

			var earlyReturn = false;
			if (_errorProcessors.Count == 0)
			{
				earlyReturn = true;
			}

			if (!earlyReturn && token.IsCancellationRequested)
			{
				var oe = new ErrorProcessorException(new OperationCanceledException(token), null, ProcessStatus.Canceled);
				errors.Add(oe);
				earlyReturn = true;
			}

			if (earlyReturn)
			{
				return new BulkProcessResult(handlingError, errors, token.IsCancellationRequested);
			}

			var info = (errorContext ?? new ProcessingErrorContext()).ToProcessingErrorInfo();
			var curError = handlingError;

			foreach (var processor in _errorProcessors)
			{
				var result = ProcessOne(processor, info, curError, token);
				if (result.Exception != null)
				{
					errors.Add(result.Exception);
					if (result.Exception.ErrorStatus == ProcessStatus.Canceled)
					{
						return new BulkProcessResult(handlingError, errors);
					}
				}

				if (token.IsCancellationRequested)
				{
					var oe = new ErrorProcessorException(new OperationCanceledException(token), processor, ProcessStatus.Canceled);
					errors.Add(oe);
					return new BulkProcessResult(handlingError, errors, isCanceledBetweenProcessors: true);
				}

				curError = result.Error;
			}

			return new BulkProcessResult(handlingError, errors);
		}

		private static ProcessorResult ProcessOne(IErrorProcessor processor, ProcessingErrorInfo info, Exception curError, CancellationToken token)
		{
			try
			{
				return new ProcessorResult(processor.Process(curError, info, token));
			}
			catch (OperationCanceledException ex) when (token.IsCancellationRequested)
			{
				return new ProcessorResult(new ErrorProcessorException(ex, processor, ProcessStatus.Canceled), curError);
			}
			catch (AggregateException ex) when (ex.IsOperationCanceledWithRequestedToken(token))
			{
				return new ProcessorResult(new ErrorProcessorException(ex.GetCancellationException(), processor, ProcessStatus.Canceled), curError);
			}
			catch (Exception ex)
			{
				return new ProcessorResult(new ErrorProcessorException(ex, processor, ProcessStatus.Faulted), curError);
			}
		}

		/// <inheritdoc/>
		public async Task<BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default)
		{
			var errors = new FlexSyncEnumerable<ErrorProcessorException>(!configAwait);

			var earlyReturn = false;
			if (_errorProcessors.Count == 0)
			{
				earlyReturn = true;
			}

			if (!earlyReturn && token.IsCancellationRequested)
			{
				var oe = new ErrorProcessorException(new OperationCanceledException(token), null, ProcessStatus.Canceled);
				errors.Add(oe);
				earlyReturn = true;
			}

			if (earlyReturn)
			{
				return new BulkProcessResult(handlingError, errors, token.IsCancellationRequested);
			}

			var info = (errorContext ?? new ProcessingErrorContext()).ToProcessingErrorInfo();
			var curError = handlingError;

			foreach (var processor in _errorProcessors)
			{
				var result = await ProcessOneAsync(processor, info, curError, token, configAwait).ConfigureAwait(configAwait);
				if (result.Exception != null)
				{
					errors.Add(result.Exception);
					if (result.Exception.ErrorStatus == ProcessStatus.Canceled)
					{
						return new BulkProcessResult(handlingError, errors);
					}
				}

				if (token.IsCancellationRequested)
				{
					var oe = new ErrorProcessorException(new OperationCanceledException(token), processor, ProcessStatus.Canceled);
					errors.Add(oe);
					return new BulkProcessResult(handlingError, errors, isCanceledBetweenProcessors: true);
				}

				curError = result.Error;
			}

			return new BulkProcessResult(handlingError, errors);
		}

		private static async Task<ProcessorResult> ProcessOneAsync(IErrorProcessor processor, ProcessingErrorInfo info, Exception curError, CancellationToken token, bool configAwait)
		{
			try
			{
				return new ProcessorResult(await processor.ProcessAsync(curError, info, configAwait, token).ConfigureAwait(configAwait));
			}
			catch (OperationCanceledException ex) when (token.IsCancellationRequested)
			{
				return new ProcessorResult(new ErrorProcessorException(ex, processor, ProcessStatus.Canceled), curError);
			}
			catch (Exception ex)
			{
				return new ProcessorResult(new ErrorProcessorException(ex, processor, ProcessStatus.Faulted), curError);
			}
		}

		/// <inheritdoc/>
		public IEnumerator<IErrorProcessor> GetEnumerator() => _errorProcessors.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Represents the result of processing a single error processor.
		/// </summary>
		private readonly struct ProcessorResult
		{
			public ProcessorResult(Exception error)
			{
				Exception = null;
				Error = error;
			}

			public ProcessorResult(ErrorProcessorException exception, Exception error)
			{
				Exception = exception;
				Error = error;
			}

			public ErrorProcessorException Exception { get; }
			public Exception Error { get; }
		}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This enum is obsolete")]
#pragma warning restore S1133 // Deprecated code should be removed
		public enum BulkProcessStatus
		{
			None = 0,
			Canceled,
			ProcessorException,
			Success
		}

		/// <summary>
		/// Represents the status of an individual processor's execution.
		/// </summary>
		public enum ProcessStatus
		{
			/// <summary>
			/// The status is not set.
			/// </summary>
			None = 0,
			/// <summary>
			/// The processor's operation was canceled.
			/// </summary>
			Canceled,
			/// <summary>
			/// The processor's operation faulted with an exception.
			/// </summary>
			Faulted
		}

		/// <summary>
		/// Represents an exception that occurs during the execution of an <see cref="IErrorProcessor"/>.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.", Justification = "<Pending>")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3925:\"ISerializable\" should be implemented correctly", Justification = "<Pending>")]
		public class ErrorProcessorException : Exception
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ErrorProcessorException"/> class.
			/// </summary>
			/// <param name="processException">The inner exception thrown by the processor.</param>
			/// <param name="processor">The processor that threw the exception.</param>
			/// <param name="processStatus">The status of the processor's execution.</param>
			public ErrorProcessorException(Exception processException, IErrorProcessor processor, ProcessStatus processStatus)
				: this("Exception in error processing", processException, processor, processStatus)
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ErrorProcessorException"/> class with a custom message.
			/// </summary>
			/// <param name="msg">The exception message.</param>
			/// <param name="processException">The inner exception thrown by the processor.</param>
			/// <param name="processor">The processor that threw the exception.</param>
			/// <param name="processStatus">The status of the processor's execution.</param>
			public ErrorProcessorException(string msg, Exception processException, IErrorProcessor processor, ProcessStatus processStatus)
				: base(msg, processException)
			{
				ErrorProcessor = processor;
				ErrorStatus = processStatus;
			}

			/// <summary>
			/// Gets the processor that threw the exception.
			/// </summary>
			public IErrorProcessor ErrorProcessor { get; }

			/// <summary>
			/// Gets the status of the processor's execution when the exception was thrown.
			/// </summary>
			public ProcessStatus ErrorStatus { get; }
		}
	}
}
