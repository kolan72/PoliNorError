using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public class BulkErrorProcessor : IBulkErrorProcessor
	{
		private readonly List<IErrorProcessor> _errorProcessors = new List<IErrorProcessor>();

		public BulkErrorProcessor() {}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete. Use parameterless constructor instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
#pragma warning disable RCS1163 // Unused parameter.
		public BulkErrorProcessor(PolicyAlias policyAlias){}
#pragma warning restore RCS1163 // Unused parameter.

		public void AddProcessor(IErrorProcessor errorProcessor)
		{
			_errorProcessors.Add(errorProcessor);
		}

		public BulkProcessResult Process(Exception handlingError, ProcessingErrorContext errorContext = null, CancellationToken token = default)
		{
			List<ErrorProcessorException> errorProcessorExceptions = new List<ErrorProcessorException>();
			if (_errorProcessors.Count == 0)
			{
				return new BulkProcessResult(handlingError, errorProcessorExceptions);
			}

			var catchBlockProcessErrorInfo = (errorContext ?? new ProcessingErrorContext()).ToProcessingErrorInfo();
			var curError = handlingError;
			var isCanceledBetweenProcessOne = false;
			foreach (var errorProcessor in _errorProcessors)
			{
				var resProcess = ProcessOne(errorProcessor, catchBlockProcessErrorInfo, curError, token);
				if (resProcess.Item1 != null)
				{
					errorProcessorExceptions.Add(resProcess.Item1);
				}
				if (token.IsCancellationRequested)
				{
					isCanceledBetweenProcessOne = true;
					break;
				}
				curError = resProcess.Item2;
			}
			return new BulkProcessResult(handlingError, errorProcessorExceptions, isCanceledBetweenProcessOne);
		}

		private (ErrorProcessorException, Exception) ProcessOne(IErrorProcessor errorProcessor, ProcessingErrorInfo catchBlockProcessErrorInfo, Exception curError, CancellationToken token)
		{
			if (token.IsCancellationRequested)
			{
				return (new ErrorProcessorException(new OperationCanceledException(token), errorProcessor, ProcessStatus.Canceled), curError);
			}

			try
			{
				curError = errorProcessor.Process(curError, catchBlockProcessErrorInfo, token);
				return (null, curError);
			}
			catch (OperationCanceledException oe) when (token.IsCancellationRequested)
			{
				return (new ErrorProcessorException(oe, errorProcessor, ProcessStatus.Canceled), curError);
			}
			catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
			{
				return (new ErrorProcessorException(ae.GetCancellationException(), errorProcessor, ProcessStatus.Canceled), curError);
			}
			catch (Exception ex)
			{
				return (new ErrorProcessorException(ex, errorProcessor, ProcessStatus.Faulted), curError);
			}
		}

		public async Task<BulkProcessResult> ProcessAsync(Exception handlingError, ProcessingErrorContext errorContext = null, bool configAwait = false, CancellationToken token = default)
		{
			var errorProcessorExceptions = new FlexSyncEnumerable<ErrorProcessorException>(!configAwait);
			if (_errorProcessors.Count == 0)
			{
				return new BulkProcessResult(handlingError, errorProcessorExceptions);
			}

			var catchBlockProcessErrorInfo = (errorContext ?? new ProcessingErrorContext()).ToProcessingErrorInfo();
			var curError = handlingError;
			var isCanceledBetweenProcessOne = false;
			foreach (var errorProcessor in _errorProcessors)
			{
				var resProcess = await ProcessOneAsync(errorProcessor, catchBlockProcessErrorInfo, curError, token, configAwait).ConfigureAwait(configAwait);
				if (resProcess.Item1 != null)
				{
					errorProcessorExceptions.Add(resProcess.Item1);
					if(resProcess.Item1.ErrorStatus == ProcessStatus.Canceled)
					{
						isCanceledBetweenProcessOne = true;
						break;
					}
				}
				if (token.IsCancellationRequested)
				{
					isCanceledBetweenProcessOne = true;
					break;
				}
				curError = resProcess.Item2;
			}
			return new BulkProcessResult(handlingError, errorProcessorExceptions, isCanceledBetweenProcessOne);
		}

		private  async Task<(ErrorProcessorException, Exception)> ProcessOneAsync(IErrorProcessor errorProcessor, ProcessingErrorInfo catchBlockProcessErrorInfo, Exception curError, CancellationToken token, bool configAwait)
		{
			if (token.IsCancellationRequested)
			{
				return (new ErrorProcessorException(new OperationCanceledException(token), errorProcessor, ProcessStatus.Canceled), curError);
			}
			try
			{
				curError = await errorProcessor.ProcessAsync(curError, catchBlockProcessErrorInfo, configAwait, token).ConfigureAwait(configAwait);
				return (null, curError);
			}
			catch (OperationCanceledException oe) when (token.IsCancellationRequested)
			{
				return (new ErrorProcessorException(oe, errorProcessor, ProcessStatus.Canceled), curError);
			}
			catch (Exception ex)
			{
				return (new ErrorProcessorException(ex, errorProcessor, ProcessStatus.Faulted), curError);
			}
		}

		public IEnumerator<IErrorProcessor> GetEnumerator() => _errorProcessors.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
			public ErrorProcessorException(Exception processException, IErrorProcessor processor, ProcessStatus processStatus) : this("Exception in error processing", processException, processor, processStatus)
			{
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="ErrorProcessorException"/> class with a custom message.
			/// </summary>
			/// <param name="msg">The exception message.</param>
			/// <param name="processException">The inner exception thrown by the processor.</param>
			/// <param name="processor">The processor that threw the exception.</param>
			/// <param name="processStatus">The status of the processor's execution.</param>
			public ErrorProcessorException(string msg, Exception processException, IErrorProcessor processor, ProcessStatus processStatus) : base(msg, processException)
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

		/// <summary>
		/// Represents the result of a bulk processing operation.
		/// </summary>
		public class BulkProcessResult
		{
			private readonly bool _isCanceledBetweenProcessOne;

			/// <summary>
			/// Initializes a new instance of the <see cref="BulkProcessResult"/> class.
			/// </summary>
			/// <param name="handlingError">The original exception that was handled.</param>
			/// <param name="processErrors">A collection of exceptions that occurred within the error processors.</param>
			/// <param name="isCanceledBetweenProcessOne">A flag indicating if cancellation was requested between processor executions.</param>
			public BulkProcessResult(Exception handlingError, IEnumerable<ErrorProcessorException> processErrors, bool isCanceledBetweenProcessOne = false)
			{
				HandlingError = handlingError;
				ProcessErrors = processErrors;
				_isCanceledBetweenProcessOne = isCanceledBetweenProcessOne;
			}

			/// <summary>
			/// Gets the original exception that was handled.
			/// </summary>
			public Exception HandlingError { get; }

			/// <summary>
			/// Gets a collection of exceptions that occurred within the error processors.
			/// </summary>
			public IEnumerable<ErrorProcessorException> ProcessErrors { get; }

			/// <summary>
			/// Gets a value indicating whether the bulk processing operation was canceled.
			/// </summary>
			public bool IsCanceled => ProcessErrors.Any(e => e.ErrorStatus == ProcessStatus.Canceled) || _isCanceledBetweenProcessOne;

			/// <summary>
			/// Converts the processing errors into a collection of <see cref="CatchBlockException"/>.
			/// </summary>
			/// <returns>An enumerable of <see cref="CatchBlockException"/>.</returns>
			public IEnumerable<CatchBlockException> ToCatchBlockExceptions()
			{
				return ProcessErrors?.Any() != true
					? Array.Empty<CatchBlockException>()
					: ProcessErrors.Select(pe => new CatchBlockException(pe, HandlingError, CatchBlockExceptionSource.ErrorProcessor));
			}
		}
	}
}
