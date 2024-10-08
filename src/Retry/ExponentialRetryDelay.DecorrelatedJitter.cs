﻿using System;

namespace PoliNorError
{
	public partial class ExponentialRetryDelay
	{
        internal class DecorrelatedJitter
        {
            // A factor used within the formula to help smooth the first calculated delay.
            private const double PFactor = 4.0;

            // A factor used to scale the median values of the retry times generated by the formula to be _near_ whole seconds.
            // This factor allows the median values to fall approximately at 1, 2, 4 etc seconds, instead of 1.4, 2.8, 5.6, 11.2.
            private const double RpScalingFactor = 1 / 1.4d;

            private readonly TimeSpan _baseDelay;
            private readonly double _exponentialFactor;
            private readonly double _maxDelayTicks;
            private readonly TimeSpan _maxTimeSpanValue;

            private double _prev;

            private static readonly Func<double> randomizer = StaticRandom.RandDouble;

            public DecorrelatedJitter(TimeSpan baseDelay, double exponentialFactor, TimeSpan maxDelay)
			{
                _baseDelay = baseDelay;
                _exponentialFactor = exponentialFactor;
                if (maxDelay.Ticks > RetryDelayConstants.MaxTimeSpanTicks)
                {
                    _maxDelayTicks = RetryDelayConstants.MaxTimeSpanTicks;
                    _maxTimeSpanValue = RetryDelayConstants.MaxTimeSpanFromTicks;
                }
                else
                {
                    _maxDelayTicks = maxDelay.Ticks;
                    _maxTimeSpanValue = maxDelay;
                }
            }

            /// <summary>
            /// Generates sleep durations in an exponentially backing-off, jittered manner, making sure to mitigate any correlations.
            /// For example: 850ms, 1455ms, 3060ms.
            /// Per discussion in Polly issue https://github.com/App-vNext/Polly/issues/530, the jitter of this implementation exhibits fewer spikes and a smoother distribution than the AWS jitter formula.
            /// </summary>
            /// <param name="attempt">The current attempt.</param>
            /// <remarks>
            /// This code was adapted from https://github.com/App-vNext/Polly/blob/main/src/Polly.Core/Retry/RetryHelper.cs.
            /// </remarks>
            public TimeSpan DecorrelatedJitterBackoffV2(int attempt)
            {
                // The original author/credit for this jitter formula is @george-polevoy .
                // Jitter formula used with permission as described at https://github.com/App-vNext/Polly/issues/530#issuecomment-526555979
                // Minor adaptations (pFactor = 4.0 and rpScalingFactor = 1 / 1.4d) by @reisenberger, to scale the formula output for easier parameterization to users.

                long targetTicksFirstDelay = _baseDelay.Ticks;

                double t = attempt + randomizer();
                double next = Math.Pow(_exponentialFactor, t) * Math.Tanh(Math.Sqrt(PFactor * t));

                // At t >=1024, the above will tend to infinity which would otherwise cause the
                // ticks to go negative. See https://github.com/App-vNext/Polly/issues/2163.
                if (double.IsInfinity(next))
                {
                    _prev = next;
                    return _maxTimeSpanValue;
                }

                double formulaIntrinsicValue = next - _prev;
                _prev = next;

                long ticks = (long)Math.Min(Math.Abs(formulaIntrinsicValue * RpScalingFactor * targetTicksFirstDelay), _maxDelayTicks);

                return TimeSpan.FromTicks(ticks);
            }
        }
    }
}
