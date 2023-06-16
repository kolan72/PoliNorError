using System.Collections.Generic;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	public interface IPolicyProcessor : IEnumerable<IErrorProcessor>
	{
		ExceptionFilter ErrorFilter { get; }
		void WithErrorProcessor(IErrorProcessor newErrorProcessor);
	}
}
