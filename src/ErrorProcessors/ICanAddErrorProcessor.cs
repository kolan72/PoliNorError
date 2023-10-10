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

		internal static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor, Action<ICanAddErrorProcessor, IErrorProcessor> action) where T : ICanAddErrorProcessor
		{
			if (errorProcessor == null)
				throw new ArgumentNullException(nameof(errorProcessor));

			action(policyProcessor, errorProcessor);
			return policyProcessor;
		}
	}
}
