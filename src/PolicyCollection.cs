using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PoliNorError
{
	internal class PolicyCollection : IEnumerable<IPolicyBase>, IWithPolicy<PolicyCollection>
	{
		protected readonly List<IPolicyBase> _policies = new List<IPolicyBase>();

		public PolicyCollection WithPolicy(Func<IPolicyBase> func) => WithPolicy(func());

		public PolicyCollection WithPolicy(IPolicyBase policyBase)
		{
			_policies.Add(policyBase);
			return this;
		}

		public PolicyCollection IncludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddIncludedErrorFilter(func);
			return this;
		}

		public PolicyCollection IncludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddIncludedErrorFilter(handledErrorFilter);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll<TException>(Func<TException, bool> func = null) where TException : Exception
		{
			this.AddExcludedErrorFilter(func);
			return this;
		}

		public PolicyCollection ExcludeErrorForAll(Expression<Func<Exception, bool>> handledErrorFilter)
		{
			this.AddExcludedErrorFilter(handledErrorFilter);
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<IPolicyBase> GetEnumerator() => _policies.GetEnumerator();
	}
}
