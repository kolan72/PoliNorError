namespace PoliNorError
{
	internal abstract class HandlerRunnerBase : IHandlerRunnerBase
	{
		protected HandlerRunnerBase(int num)
		{
			CollectionIndex = num;
		}

		public int CollectionIndex { get; protected set; }

		public abstract bool SyncRun { get; }
	}
}
