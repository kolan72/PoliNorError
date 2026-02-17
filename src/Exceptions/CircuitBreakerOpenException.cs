using System;

namespace PoliNorError
{
	public class CircuitBreakerOpenException : InvalidOperationException
	{
		public CircuitBreakerOpenException()
			: base("The circuit breaker is open and is rejecting executions.")
		{
		}
	}
}
