using System;

namespace PoliNorError
{
	public interface ICircuitBreakerPolicy : IPolicyBase
	{
		CircuitBreakerState State { get; }

		ICircuitBreakerPolicy WithOptions(Action<CircuitBreakerOptions> configure);

		ICircuitBreakerPolicy OnBreak(Action<PolicyResult> onBreak);

		ICircuitBreakerPolicy OnReset(Action onReset);

		ICircuitBreakerPolicy OnHalfOpen(Action onHalfOpen);
	}
}
