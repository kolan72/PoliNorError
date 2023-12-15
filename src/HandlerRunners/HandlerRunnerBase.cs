namespace PoliNorError
{
	internal abstract class HandlerRunnerBase : IHandlerRunnerBase
	{
		protected HandlerRunnerBase(int num)
		{
			CollectionIndex = num;
		}

		protected HandlerRunnerBase(){}

		public int CollectionIndex { get; set; }

		public abstract bool SyncRun { get; }
	}
}
