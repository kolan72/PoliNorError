using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace PoliNorError
{
	public abstract class PolicyProcessor : IPolicyProcessor
	{
		protected IBulkErrorProcessor _bulkErrorProcessor;

		protected PolicyProcessor(PolicyAlias policyAlias, IBulkErrorProcessor bulkErrorProcessor = null)
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor(policyAlias);
		}

		public void AddErrorProcessor(IErrorProcessor newErrorProcessor)
		{
			_bulkErrorProcessor.AddProcessor(newErrorProcessor);
		}

		public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => ErrorFilter.IncludedErrorFilters;

		public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => ErrorFilter.ExcludedErrorFilters;

		public ExceptionFilter ErrorFilter { get; } = new ExceptionFilter();

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

		public sealed class ExceptionFilter
		{
			private readonly List<Expression<Func<Exception, bool>>> _includedErrorFilters = new List<Expression<Func<Exception, bool>>>();
			private readonly List<Expression<Func<Exception, bool>>> _excludedErrorFilters = new List<Expression<Func<Exception, bool>>>();

			internal void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				_includedErrorFilters.Add(handledErrorFilter);
			}

			internal void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				_excludedErrorFilters.Add(handledErrorFilter);
			}

			internal Func<Exception, bool> GetCanHandle()
			{
				var includedFilterPredicate = GetIncludedErrorFilterPredicate();
				var excludedFilterPredicate = GetExcludedErrorFilterPredicate();
				bool res(Exception e) => includedFilterPredicate(e) && !excludedFilterPredicate(e);
				return res;
			}

			public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => _includedErrorFilters;

			public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => _excludedErrorFilters;

			private Func<Exception, bool> GetIncludedErrorFilterPredicate()
			{
				if (_includedErrorFilters?.Any() != true)
					return (_) => true;

				var res = GetOrCombinedFilter(_includedErrorFilters);

				return res.Compile();
			}

			private Func<Exception, bool> GetExcludedErrorFilterPredicate()
			{
				if (_excludedErrorFilters?.Any() != true)
					return (_) => false;

				var res = GetOrCombinedFilter(_excludedErrorFilters);

				return res.Compile();
			}

			private static Expression<Func<Exception, bool>> GetOrCombinedFilter(IEnumerable<Expression<Func<Exception, bool>>> filterExpressCollection)
			{
				return filterExpressCollection.GetOrCombined();
			}
		}
	}
}
