using System.Collections.Generic;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	public interface IPolicyProcessor : IEnumerable<IErrorProcessor>, ICanAddErrorProcessor
	{
		ExceptionFilter ErrorFilter { get; }
		void AddErrorProcessor(IErrorProcessor newErrorProcessor);
	}
}
