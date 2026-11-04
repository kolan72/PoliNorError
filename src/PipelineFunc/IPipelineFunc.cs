namespace PoliNorError
{
	/// <summary>
	/// Non-generic marker interface for a registered pipeline function.
	/// Exposes only the pipeline name so that registrations of different
	/// input/output type pairs can be enumerated uniformly.
	/// </summary>
	public interface IPipelineFunc
	{
		/// <summary>
		/// Gets the name of this pipeline, derived from the underlying policy's <see cref="IPolicyBase.PolicyName"/>.
		/// </summary>
		string Name { get; }
	}
}