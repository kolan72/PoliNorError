using System;
using static PoliNorError.PolicyProcessor;

namespace PoliNorError
{
	internal interface ICanHandleChecker<T>
	{
		HandleCatchBlockResult CanHandle(Exception exception, ErrorContext<T> errorContext);
	}

	internal class DefalutCanHandleChecker : ICanHandleChecker<Unit>
	{
		private readonly ExceptionFilter _exceptionFilter;
		public DefalutCanHandleChecker(ExceptionFilter exceptionFilter)
		{
			_exceptionFilter = exceptionFilter;
		}

		public HandleCatchBlockResult CanHandle(Exception exception, ErrorContext<Unit> errorContext)
		{
			if (!GetCanHandle()(exception))
				return HandleCatchBlockResult.FailedByErrorFilter;
			else
				return HandleCatchBlockResult.Success;
		}

		protected Func<Exception, bool> GetCanHandle()
		{
			return _exceptionFilter.GetCanHandle();
		}
	}
}
