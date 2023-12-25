using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal static class ErrorProcessorFuncExtensions
	{
	 	public static Action<Exception> ToActionForInnerException<TException>(this Action<TException> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Action<Exception, CancellationToken> ToActionForInnerException<TException>(this Action<TException, CancellationToken> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Action<Exception, ProcessingErrorInfo> ToActionForInnerException<TException>(this Action<TException, ProcessingErrorInfo> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Action<Exception, ProcessingErrorInfo, CancellationToken> ToActionForInnerException<TException>(this Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Func<Exception, Task> ToFuncForInnerException<TException>(this Func<TException, Task> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Func<Exception, CancellationToken, Task> ToFuncForInnerException<TException>(this Func<TException, CancellationToken, Task> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Func<Exception, ProcessingErrorInfo, Task> ToFuncForInnerException<TException>(this Func<TException, ProcessingErrorInfo, Task> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}

		public static Func<Exception, ProcessingErrorInfo, CancellationToken, Task> ToFuncForInnerException<TException>(this Func<TException, ProcessingErrorInfo, CancellationToken, Task> actionProcessor) where TException : Exception
		{
			return ErrorProcessorFuncConverter.Convert(actionProcessor, ConvertExceptionDelegates.ToInnerException);
		}
	}

	internal static class ErrorProcessorFuncConverter
	{
		internal static Action<Exception> Convert<TException>(Action<TException> actionProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return ex =>
			{
				var resConvert = convertException(ex, out TException exception);
				if (resConvert)
				{
					actionProcessor(exception);
				}
			};
		}

		internal static Action<Exception, CancellationToken> Convert<TException>(Action<TException, CancellationToken> actionProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, token) =>
			{
				var resConvert = convertException(ex, out TException exception);
				if (resConvert)
				{
					actionProcessor(exception, token);
				}
			};
		}

		internal static Func<Exception, Task> Convert<TException>(Func<TException, Task> funcProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return ex =>
			{
				var resConvert = convertException(ex, out TException exception);
				return resConvert ? funcProcessor(exception) : Task.CompletedTask;
			};
		}

		internal static Func<Exception, CancellationToken, Task> Convert<TException>(Func<TException, CancellationToken, Task> funcProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, token) =>
			{
				var resConvert = convertException(ex, out TException exception);
				return resConvert ? funcProcessor(exception, token) : Task.CompletedTask;
			};
		}

		internal static Action<Exception, ProcessingErrorInfo> Convert<TException>(Action<TException, ProcessingErrorInfo> actionProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, pei)=>
			{
				var resConvert = convertException(ex, out TException exception);
				if (resConvert)
				{
					actionProcessor(exception, pei);
				}
			};
		}

		internal static Action<Exception, ProcessingErrorInfo, CancellationToken> Convert<TException>(Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, pei, token) =>
			{
				var resConvert = convertException(ex, out TException exception);
				if (resConvert)
				{
					actionProcessor(exception, pei, token);
				}
			};
		}

		internal static Func<Exception, ProcessingErrorInfo, Task> Convert<TException>(Func<TException, ProcessingErrorInfo, Task> funcProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, pei) =>
			{
				var resConvert = convertException(ex, out TException exception);
				return resConvert ? funcProcessor(exception, pei) : Task.CompletedTask;
			};
		}

		internal static Func<Exception, ProcessingErrorInfo, CancellationToken, Task> Convert<TException>(Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, ConvertExceptionPredicate<TException> convertException)
		{
			return (ex, pei, token) =>
			{
				var resConvert = convertException(ex, out TException exception);
				return resConvert ? funcProcessor(exception, pei, token) : Task.CompletedTask;
			};
		}
	}
}
