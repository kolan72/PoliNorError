using System;
using System.Linq;
using System.Linq.Expressions;

namespace PoliNorError
{
	public static partial class PolicyDelegateCollectionRegistrar
	{
		public static IPolicyDelegateCollection IncludeErrorForAll(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.Select(pd => pd.Policy).AddIncludedErrorFilter(handledErrorFilter);
			return policyDelegateCollection;
		}

		public static IPolicyDelegateCollection ExcludeErrorForAll(this IPolicyDelegateCollection policyDelegateCollection, Expression<Func<Exception, bool>> handledErrorFilter)
		{
			policyDelegateCollection.Select(pd => pd.Policy).AddExcludedErrorFilter(handledErrorFilter);
			return policyDelegateCollection;
		}
	}
}
