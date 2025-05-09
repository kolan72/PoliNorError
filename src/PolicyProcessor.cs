﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace PoliNorError
{
	public abstract class PolicyProcessor : IPolicyProcessor
	{
		protected IBulkErrorProcessor _bulkErrorProcessor;

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This field is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected bool _isPolicyAliasSet;

		protected PolicyProcessor(IBulkErrorProcessor bulkErrorProcessor = null): this(new ExceptionFilter(), bulkErrorProcessor)
		{}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete.  Use constructors without the PolicyAlias parameter instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected PolicyProcessor(PolicyAlias policyAlias, IBulkErrorProcessor bulkErrorProcessor = null) : this(policyAlias, new ExceptionFilter(), bulkErrorProcessor)
		{}

		protected PolicyProcessor(ExceptionFilter exceptionFilter, IBulkErrorProcessor bulkErrorProcessor = null)
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor();
			ErrorFilter = exceptionFilter;
		}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete.  Use constructors without the PolicyAlias parameter instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
#pragma warning disable RCS1163 // Unused parameter.
		protected PolicyProcessor(PolicyAlias policyAlias, ExceptionFilter exceptionFilter, IBulkErrorProcessor bulkErrorProcessor = null)
#pragma warning restore RCS1163 // Unused parameter.
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor();
			_isPolicyAliasSet = bulkErrorProcessor == null;
			ErrorFilter = exceptionFilter;
		}

		public void AddErrorProcessor(IErrorProcessor newErrorProcessor)
		{
			_bulkErrorProcessor.AddProcessor(newErrorProcessor);
		}

		public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => ErrorFilter.IncludedErrorFilters;

		public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => ErrorFilter.ExcludedErrorFilters;

		public ExceptionFilter ErrorFilter { get; }

		internal PolicyProcessorCatchBlockSyncHandler<T> GetCatchBlockSyncHandler<T>(PolicyResult policyResult, CancellationToken token, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			return new PolicyProcessorCatchBlockSyncHandler<T> (policyResult,
																_bulkErrorProcessor,
																token,
																ErrorFilter.GetCanHandle(),
																policyRuleFunc);
		}

		internal PolicyProcessorCatchBlockAsyncHandler<T> GetCatchBlockAsyncHandler<T>(PolicyResult policyResult, bool configAwait, CancellationToken token, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			return new PolicyProcessorCatchBlockAsyncHandler<T>(policyResult,
																_bulkErrorProcessor,
																configAwait,
																token,
																ErrorFilter.GetCanHandle(),
																policyRuleFunc);
		}

		public IEnumerator<IErrorProcessor> GetEnumerator() => _bulkErrorProcessor.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public class ExceptionFilter
		{
			internal readonly List<Expression<Func<Exception, bool>>> _includedErrorFilters = new List<Expression<Func<Exception, bool>>>();
			internal readonly List<Expression<Func<Exception, bool>>> _excludedErrorFilters = new List<Expression<Func<Exception, bool>>>();

			internal void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				_includedErrorFilters.Add(handledErrorFilter);
			}

			internal void AddIncludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddIncludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				_excludedErrorFilters.Add(handledErrorFilter);
			}

			internal void AddExcludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			internal void AddExcludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			internal void AppendFilter(ExceptionFilter exceptionFilter)
			{
				foreach (var filter in exceptionFilter.IncludedErrorFilters)
				{
					_includedErrorFilters.Add(filter);
				}
				foreach (var filter in exceptionFilter.ExcludedErrorFilters)
				{
					_excludedErrorFilters.Add(filter);
				}
			}

			internal Func<Exception, bool> GetCanHandle()
			{
				var (includeMode, includeExpression) = GetIncludedErrorFilterPredicateTuple();
				var (excludeMode, excludeExpression) = GetExcludedErrorFilterPredicateTuple();

				var resMode = includeMode | excludeMode;

				if ((resMode & ErrorFilterModes.Include) != 0)
				{
					return (resMode & ErrorFilterModes.Exclude) != 0 ? includeExpression.And(excludeExpression).Compile() : includeExpression.Compile();
				}
				else if ((resMode & ErrorFilterModes.Exclude) != 0)
				{
					return excludeExpression.Compile();
				}
				else
				{
					return (_) => true;
				}
			}

			public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => _includedErrorFilters;

			public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => _excludedErrorFilters;

			private (ErrorFilterModes mode, Expression<Func<Exception, bool>> expression) GetIncludedErrorFilterPredicateTuple()
			{
				if (_includedErrorFilters.Count == 0)
					return (ErrorFilterModes.None, null);

				return (ErrorFilterModes.Include, _includedErrorFilters.GetOrCombined());
			}

			private (ErrorFilterModes mode, Expression<Func<Exception, bool>> expression) GetExcludedErrorFilterPredicateTuple()
			{
				if (_excludedErrorFilters.Count == 0)
					return (ErrorFilterModes.None, null);

				var res = _excludedErrorFilters.GetOrCombined().Not();

				return (ErrorFilterModes.Exclude, res);
			}

			[Flags]
			private enum ErrorFilterModes
			{
				None = 0,
				Include = 1,
				Exclude = 2,
				IncludeExclude = Include | Exclude
			}
		}
	}
}
