﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public static class IPolicyProcessorExtensions
	{
		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception> onBeforeProcessError, ConvertToCancelableFuncType convertToCancelableFuncType = ConvertToCancelableFuncType.Precancelable) where T : IPolicyProcessor => WithErrorProcessorOf(policyProcessor, onBeforeProcessError.ToCancelableAction(convertToCancelableFuncType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> onBeforeProcessError) where T : IPolicyProcessor => WithErrorProcessorOf(policyProcessor, onBeforeProcessError, null);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, Task> onBeforeProcessErrorAsync, ConvertToCancelableFuncType convertToCancelableFuncType = ConvertToCancelableFuncType.Precancelable) where T : IPolicyProcessor => WithErrorProcessorOf(policyProcessor, onBeforeProcessErrorAsync.ToCancelableFunc(convertToCancelableFuncType));

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) where T : IPolicyProcessor => WithErrorProcessorOf(policyProcessor, null, onBeforeProcessErrorAsync);

		public static T WithErrorProcessorOf<T>(this T policyProcessor, Action<Exception, CancellationToken> onBeforeProcessError, Func<Exception, CancellationToken, Task> onBeforeProcessErrorAsync) where T : IPolicyProcessor
		{
			policyProcessor.WithErrorProcessor(new DefaultErrorProcessor(onBeforeProcessError, onBeforeProcessErrorAsync));
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
