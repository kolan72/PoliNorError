using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public partial class BulkErrorProcessor
	{
		/// <summary>
		/// Represents the result of a bulk processing operation.
		/// </summary>
		public class BulkProcessResult
		{
			private readonly IReadOnlyList<ErrorProcessorException> _processErrors;

			/// <summary>
			/// Initializes a new instance of the <see cref="BulkProcessResult"/> class.
			/// </summary>
			/// <param name="handlingError">The original exception that was handled.</param>
			/// <param name="processErrors">A collection of exceptions that occurred within the error processors.</param>
			/// <param name="isCanceledBetweenProcessors">A flag indicating if cancellation was requested between processor executions.</param>
			public BulkProcessResult(Exception handlingError, IEnumerable<ErrorProcessorException> processErrors, bool isCanceledBetweenProcessors = false)
			{
				HandlingError = handlingError;
				_processErrors = (processErrors ?? Array.Empty<ErrorProcessorException>()).ToList();
				IsCanceledBetweenProcessors = isCanceledBetweenProcessors;
			}

			/// <summary>
			/// Gets the original exception that was handled.
			/// </summary>
			public Exception HandlingError { get; }

			/// <summary>
			/// Gets a collection of exceptions that occurred within the error processors.
			/// </summary>
			public IEnumerable<ErrorProcessorException> ProcessErrors => _processErrors;

			/// <summary>
			/// Gets a value indicating whether any processing errors occurred.
			/// </summary>
			public bool HasProcessErrors => _processErrors.Count > 0;

			/// <summary>
			/// Gets a value indicating whether the bulk processing operation was canceled.
			/// </summary>
			public bool IsCanceled => _processErrors.Any(e => e.ErrorStatus == ProcessStatus.Canceled) || IsCanceledBetweenProcessors;

			/// <summary>
			/// Gets a value indicating whether cancellation was requested between processor executions.
			/// </summary>
			public bool IsCanceledBetweenProcessors { get; }

			/// <summary>
			/// Gets the <see cref="OperationCanceledException"/> that caused the cancellation, if any.
			/// </summary>
			public OperationCanceledException CancellationException =>
				_processErrors.FirstOrDefault(e => e.ErrorStatus == ProcessStatus.Canceled)?.InnerException as OperationCanceledException;

			/// <summary>
			/// Converts the processing errors into a collection of <see cref="CatchBlockException"/>.
			/// </summary>
			/// <returns>An enumerable of <see cref="CatchBlockException"/>.</returns>
			public IEnumerable<CatchBlockException> ToCatchBlockExceptions()
			{
				foreach (var pe in _processErrors)
				{
					yield return new CatchBlockException(
						pe,
						HandlingError,
						CatchBlockExceptionSource.ErrorProcessor);
				}
			}
		}
	}
}
