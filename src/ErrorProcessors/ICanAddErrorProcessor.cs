using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public interface ICanAddErrorProcessor { };

	internal static class ICanAddErrorProcessorExtensions
	{
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(actionProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new BasicErrorProcessor(funcProcessor, actionProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo, CancellationToken> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(actionProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor, cancellationType), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor), action);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, ProcessingErrorInfo, Task> funcProcessor, Action<Exception, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessor(funcProcessor, actionProcessor, cancellationType), action);

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(actionProcessor), action);
		}

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(actionProcessor, cancellationType), action);
		}

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Action<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(actionProcessor), action);
		}

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(funcProcessor), action);
		}

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, Task> funcProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(funcProcessor, cancellationType), action);
		}

		public static T WithErrorContextProcessorOf<T, TErrorContext>(this T policyProcessor, Func<Exception, ProcessingErrorInfo<TErrorContext>, CancellationToken, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorContextProcessor(policyProcessor, new DefaultErrorProcessor<TErrorContext>(funcProcessor), action);
		}

		public static T WithErrorContextProcessor<T, TErrorContext>(this T policyProcessor, DefaultErrorProcessor<TErrorContext> errorProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			return WithErrorProcessor(policyProcessor, errorProcessor, action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, CancellationToken> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), cancellationType, action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, Task> funcProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), cancellationType, action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, CancellationToken, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo, CancellationToken> actionProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Action<TException, ProcessingErrorInfo> actionProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(actionProcessor.ToActionForInnerException(), cancellationType, action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, Task> funcProcessor, CancellationType cancellationType, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), cancellationType, action);
		}

		public static T WithInnerErrorProcessorOf<T, TException>(this T policyProcessor, Func<TException, ProcessingErrorInfo, CancellationToken, Task> funcProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor where TException : Exception
		{
			return policyProcessor.WithErrorProcessorOf(funcProcessor.ToFuncForInnerException(), action);
		}

		internal static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			if (errorProcessor == null)
				throw new ArgumentNullException(nameof(errorProcessor));

			action(policyProcessor, errorProcessor);
			return policyProcessor;
		}
	}
}
