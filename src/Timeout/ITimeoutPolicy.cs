using System;

namespace PoliNorError
{
	public interface ITimeoutPolicy : IPolicyBase
	{
		ITimeoutPolicy WithTimeout(TimeSpan timeout);

		ITimeoutPolicy WithStrategy(TimeoutStrategy strategy);

		ITimeoutPolicy WithOptions(Action<TimeoutPolicyOptions> configure);
	}
}
