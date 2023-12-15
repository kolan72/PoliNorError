namespace PoliNorError
{
	internal interface IHandlerRunnerBase
	{
		int CollectionIndex { get; set; }
		bool SyncRun { get; }
	}
}
