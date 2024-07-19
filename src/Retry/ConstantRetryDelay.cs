using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public class ConstantRetryDelay : RetryDelay
	{
		private readonly TimeSpan _baseDelay;

		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions) : this(retryDelayOptions.BaseDelay){}

		internal ConstantRetryDelay(TimeSpan baseDelay)
		{
			_baseDelay = baseDelay;
		}

		public override TimeSpan GetDelay(int attempt)
		{
			return _baseDelay;
		}
	}

	/// <summary>
	/// Represents options for the <see cref="ConstantRetryDelay"/>.
	/// </summary>
	public class ConstantRetryDelayOptions : RetryDelayOptions
	{
		public override RetryDelayType DelayType => RetryDelayType.Constant;
	}
}
