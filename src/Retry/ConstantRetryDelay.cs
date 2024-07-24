using System;

namespace PoliNorError
{
	/// <summary>
	/// Class to get the constant delay value.
	/// </summary>
	public class ConstantRetryDelay : RetryDelay
	{
		private readonly ConstantRetryDelayOptions _retryDelayOptions;

		/// <summary>
		/// Initializes a new instance of <see cref="ConstantRetryDelay"/>.
		/// </summary>
		/// <param name="retryDelayOptions"><see cref="ConstantRetryDelayOptions"/></param>
		public ConstantRetryDelay(ConstantRetryDelayOptions retryDelayOptions)
		{
			InnerDelay = this;
			_retryDelayOptions = retryDelayOptions;
		}

		internal ConstantRetryDelay(TimeSpan baseDelay) : this(new ConstantRetryDelayOptions() { BaseDelay = baseDelay }){}

		protected override TimeSpan GetInnerDelay(int attempt)
		{
			return _retryDelayOptions.BaseDelay;
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
