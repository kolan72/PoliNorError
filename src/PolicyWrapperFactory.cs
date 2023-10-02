using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperFactory
	{
		private readonly IPolicyBase _wrappedPolicy;
		protected readonly IEnumerable<IPolicyBase> _polices;

		internal PolicyWrapperFactory(IPolicyBase wrappedPolicy)
		{
			_wrappedPolicy = wrappedPolicy;
			WrapSinglePolicy = true;
		}

		internal PolicyWrapper CreateWrapper(Action action, CancellationToken token)
		{
			if (WrapSinglePolicy)
			{
				return new PolicyWrapperSingle(_wrappedPolicy, action, token);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		internal PolicyWrapper CreateWrapper(Func<CancellationToken, Task> func, CancellationToken token, bool configureAwait)
		{
			if (WrapSinglePolicy)
			{
				return new PolicyWrapperSingle(_wrappedPolicy, func, token, configureAwait);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		protected bool WrapSinglePolicy { get; }
	}
}
