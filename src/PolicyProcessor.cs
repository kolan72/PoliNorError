using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PoliNorError
{
	public abstract partial class PolicyProcessor : IPolicyProcessor
	{
		private readonly List<Expression<Func<Exception, bool>>> _includedErrorFilters = new List<Expression<Func<Exception, bool>>>();
		private readonly List<Expression<Func<Exception, bool>>> _excludedErrorFilters = new List<Expression<Func<Exception, bool>>>();

		protected IBulkErrorProcessor _bulkErrorProcessor;

		protected PolicyProcessor(IBulkErrorProcessor bulkErrorProcessor = null)
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor();
		}

		public void WithErrorProcessor(IErrorProcessor newErrorProcessor)
		{
			_bulkErrorProcessor.AddProcessor(newErrorProcessor);
		}

		public void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			_includedErrorFilters.Add(handledErrorFilter);
		}

		public void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			_excludedErrorFilters.Add(handledErrorFilter);
		}

		public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => _includedErrorFilters;

		public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => _excludedErrorFilters;

		internal HandleCatchBlockResult CanHandle(Exception exception)
		{
			if (!GetCanHandle()(exception))
				return HandleCatchBlockResult.FailedByErrorFilter;
			else
				return HandleCatchBlockResult.Success;
		}

		protected Func<Exception, bool> GetCanHandle()
		{
			return CanHandleHolder.Create(_includedErrorFilters, _excludedErrorFilters).GetCanHandle();
		}

		public IEnumerator<IErrorProcessor> GetEnumerator() => _bulkErrorProcessor.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public sealed class CanHandleHolder
		{
			private readonly IEnumerable<Expression<Func<Exception, bool>>> _includedErrorFilters;
			private readonly IEnumerable<Expression<Func<Exception, bool>>> _excludedErrorFilters;

			private CanHandleHolder(IEnumerable<Expression<Func<Exception, bool>>> includedErrorFilters, IEnumerable<Expression<Func<Exception, bool>>> excludedErrorFilters)
			{
				_includedErrorFilters = includedErrorFilters;
				_excludedErrorFilters = excludedErrorFilters;
			}

			public static CanHandleHolder Create(IEnumerable<Expression<Func<Exception, bool>>> includedErrorFilters = null, IEnumerable<Expression<Func<Exception, bool>>> excludedErrorFilters = null)
			{
				if (!(includedErrorFilters == null ^ excludedErrorFilters == null))
				{
					return new CanHandleHolder(includedErrorFilters, excludedErrorFilters);
				}
				else if (includedErrorFilters != null)
				{
					return FromIncludedErrorFilters(includedErrorFilters);
				}
				else
				{
					return FromExcludedErrorFilters(excludedErrorFilters);
				}
			}

			public static CanHandleHolder FromIncludedErrorFilters(IEnumerable<Expression<Func<Exception, bool>>> includedErrorFilters)
			{
				return new CanHandleHolder(includedErrorFilters, null);
			}

			public static CanHandleHolder FromExcludedErrorFilters(IEnumerable<Expression<Func<Exception, bool>>> excludedErrorFilters)
			{
				return new CanHandleHolder(null, excludedErrorFilters);
			}

			public Func<Exception, bool> GetCanHandle()
			{
				var includedFilterPredicate = GetIncludedErrorFilterPredicate();
				var excludedFilterPredicate = GetExcludedErrorFilterPredicate();
				bool res(Exception e) => includedFilterPredicate(e) && !excludedFilterPredicate(e);
				return res;
			}

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
