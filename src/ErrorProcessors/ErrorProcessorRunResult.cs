namespace PoliNorError
{
	internal enum ErrorProcessorRunResult
	{
		CancelableActionNoToken,
		CancelableActionTokenExists,
		NotCancelableActionNoToken,
		NotCancelableActionTokenExists,
		CancelableFuncTokenExists,
		CancelableFuncNoToken,
		NotCancelableFuncNoToken,
		NotCancelableFuncTokenExists
	}
}
