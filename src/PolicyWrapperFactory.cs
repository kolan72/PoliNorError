using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyWrapperFactory
	{
		private readonly IPolicyBase _wrappedPolicy;

		private readonly IEnumerable<IPolicyBase> _wrappePolices;
		private readonly ThrowOnWrappedCollectionFailed _throwOnWrappedCollectionFailed;

		internal PolicyWrapperFactory(IPolicyBase wrappedPolicy)
		{
			_wrappedPolicy = wrappedPolicy;
			WrapSinglePolicy = true;
		}

		internal PolicyWrapperFactory(IEnumerable<IPolicyBase> wrappePolices, ThrowOnWrappedCollectionFailed throwOnWrappedCollectionFailed)
		{
			_wrappePolices = wrappePolices;
			_throwOnWrappedCollectionFailed = throwOnWrappedCollectionFailed;
			WrapSinglePolicy = false;
		}

		internal PolicyWrapper CreateWrapper(Action action, CancellationToken token)
		{
			if (WrapSinglePolicy)
			{
				return new PolicyWrapperSingle(_wrappedPolicy, action, token);
			}
			else
			{
				return new PolicyWrapperCollection(_wrappePolices, action, token, _throwOnWrappedCollectionFailed);
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
				return new PolicyWrapperCollection(_wrappePolices, func, token, _throwOnWrappedCollectionFailed, configureAwait);
			}
		}

		protected bool WrapSinglePolicy { get; }
	}
}
