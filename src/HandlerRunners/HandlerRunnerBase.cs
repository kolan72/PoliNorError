namespace PoliNorError
{
	internal abstract class HandlerRunnerBase : IHandlerRunnerBase
	{
		protected HandlerRunnerBase(int num)
		{
			Num = num;
		}

		public int Num { get; protected set; }

		public abstract bool UseSync { get; }
	}
}
