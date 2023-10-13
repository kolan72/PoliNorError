namespace PoliNorError
{
	internal abstract class ErrorContext<T>
	{
		protected ErrorContext(T t)
		{
			Context = t;
		}

		public T Context { get; }

		public abstract ProcessingErrorContext ToProcessingErrorContext();
	}

	internal sealed class EmptyErrorContext : ErrorContext<Unit>
	{
		private static EmptyErrorContext _defaultContext = new EmptyErrorContext();

		public static EmptyErrorContext Default => _defaultContext;

		private EmptyErrorContext() : base(Unit.Default){}

		public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext();
	}
}
