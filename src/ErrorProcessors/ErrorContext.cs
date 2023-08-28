namespace PoliNorError
{
	public abstract class ErrorContext<T>
	{
		protected ErrorContext(T t)
		{
			Context = t;
		}

		public T Context { get; }

		public abstract ProcessingErrorContext ToProcessingErrorContext();
	}
}
