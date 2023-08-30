using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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
		[Obsolete("Switch to CatchBlockHandlers")]
		protected void HandleCatchBlockAndChangeResult(Exception ex, PolicyResult result, CancellationToken token, ProcessingErrorContext  errorContext = null)
		{
			result.ChangeByHandleCatchBlockResult(CanHandleCatchBlock());

			HandleCatchBlockResult CanHandleCatchBlock()
			{
				if (token.IsCancellationRequested)
				{
					return HandleCatchBlockResult.Canceled;
				}
				var checkFallbackResult = CanHandle(ex);
				if (checkFallbackResult == HandleCatchBlockResult.Success)
				{
					var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext, token);
					result.AddBulkProcessorErrors(bulkProcessResult);
					return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : checkFallbackResult;
				}
				else
				{
					return checkFallbackResult;
				}
			}
		}
		[Obsolete("Switch to CatchBlockHandlers")]
		protected async Task HandleCatchBlockAndChangeResultAsync(Exception ex, PolicyResult result, CancellationToken token, bool configAwait, ProcessingErrorContext errorContext = null)
		{
			result.ChangeByHandleCatchBlockResult(await CanHandleCatchBlockAsync().ConfigureAwait(configAwait));

			async Task<HandleCatchBlockResult> CanHandleCatchBlockAsync()
			{
				if (token.IsCancellationRequested)
				{
					return HandleCatchBlockResult.Canceled;
				}
				var checkFallbackResult = CanHandle(ex);
				if (checkFallbackResult == HandleCatchBlockResult.Success)
				{
					var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ex, errorContext, configAwait, token).ConfigureAwait(configAwait);
					result.AddBulkProcessorErrors(bulkProcessResult);
					return bulkProcessResult.IsCanceled ? HandleCatchBlockResult.Canceled : checkFallbackResult;
				}
				else
				{
					return checkFallbackResult;
				}
			}
		}

		internal HandleCatchBlockResult CanHandle(Exception exception)
		{
			if (!GetCanHandle()(exception))
				return HandleCatchBlockResult.FailedByErrorFilter;
			else
				return HandleCatchBlockResult.Success;
		}

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

		protected Func<Exception, bool> GetCanHandle()
		{
			return ErrorFilter.GetCanHandle();
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
