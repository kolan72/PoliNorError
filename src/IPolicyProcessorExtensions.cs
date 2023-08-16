using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IPolicyProcessorExtensions
	{
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
						=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> actionProcessor) where T : IPolicyProcessor
						=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> actionProcessor, CancellationType cancellationType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(actionProcessor, cancellationType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor, convertToCancelableFuncType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor, actionProcessor, convertToCancelableFuncType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor, actionProcessor));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> funcProcessor, Action<Exception> actionProcessor, CancellationType convertToCancelableFuncType) where T : IPolicyProcessor
					=> WithErrorProcessor(policyProcessor, new DefaultErrorProcessorV2(funcProcessor, actionProcessor, convertToCancelableFuncType));

		public static T WithErrorProcessor<T>(this T policyProcessor, IErrorProcessor errorProcessor) where T : IPolicyProcessor
		{
			if (errorProcessor == null)
				throw new ArgumentNullException(nameof(errorProcessor));

			policyProcessor.AddErrorProcessor(errorProcessor);
			return policyProcessor;
		}

		internal static T IncludeError<T, TException>(this T policyProcessor, Func<TException, bool> func = null) where T : IPolicyProcessor where TException : Exception
		{
			policyProcessor.AddIncludedErrorFilter(func);
			return policyProcessor;
		}

		internal static T IncludeError<T>(this T policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyProcessor
		{
			policyProcessor.AddIncludedErrorFilter(handledErrorFilter);
			return policyProcessor;
		}

		internal static T ExcludeError<T, TException>(this T policyProcessor, Func<TException, bool> func = null) where T : IPolicyProcessor where TException : Exception
		{
			policyProcessor.AddExcludedErrorFilter(func);
			return policyProcessor;
		}

		internal static T ExcludeError<T>(this T policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter) where T : IPolicyProcessor
		{
			policyProcessor.AddExcludedErrorFilter(handledErrorFilter);
			return policyProcessor;
		}

		internal static void AddIncludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(handledErrorFilter);
		}

		internal static void AddIncludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}

		internal static void AddExcludedErrorFilter(this IPolicyProcessor policyProcessor, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(handledErrorFilter);
		}

		internal static void AddExcludedErrorFilter<TException>(this IPolicyProcessor policyProcessor, Func<TException, bool> func = null) where TException : Exception
		{
			policyProcessor.ErrorFilter.AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
		}
	}
}
