namespace PoliNorError
{
	internal enum HandleCatchBlockResult
	{
		FailedByPolicyRules = 1,
		FailedByErrorFilter,
		Canceled,
		Success
	}
}
