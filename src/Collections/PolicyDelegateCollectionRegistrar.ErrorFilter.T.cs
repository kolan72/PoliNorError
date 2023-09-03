using System;
using System.Linq.Expressions;

namespace PoliNorError
{
	public static partial class PolicyDelegateCollectionRegistrar
	{
		public static  IPolicyDelegateCollection<T> IncludeErrorForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection,  Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddIncludedErrorFilter(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection<T> ExcludeErrorForAll<T>(this IPolicyDelegateCollection<T> policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.AddExcludedErrorFilter(handledErrorFilter);
			return policyDelegateCollection;
		}
	}
}
