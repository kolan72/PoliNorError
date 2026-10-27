using System;

namespace PoliNorError
{
	internal sealed class RetryStepOutcome
	{
		private static readonly RetryStepOutcome _terminal = new RetryStepOutcome(true, null, null);

		private RetryStepOutcome(bool isTerminal, TimeSpan? delay, Exception lastError)
		{
			IsTerminal = isTerminal;
			Delay = delay;
			LastError = lastError;
		}

		internal static RetryStepOutcome Terminal => _terminal;

		internal static RetryStepOutcome WaitAndContinue(TimeSpan? delay, Exception lastError)
			=> new RetryStepOutcome(false, delay, lastError);

		internal bool IsTerminal { get; }

		internal TimeSpan? Delay { get; }

		internal Exception LastError { get; }
	}
}