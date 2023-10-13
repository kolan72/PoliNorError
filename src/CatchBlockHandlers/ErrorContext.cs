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
		public static EmptyErrorContext Default { get; } = new EmptyErrorContext();

		public static EmptyErrorContext DefaultFallback { get; } = new EmptyErrorContext() { PolicyKind = PolicyAlias.Fallback };

		public static EmptyErrorContext DefaultSimple { get; } = new EmptyErrorContext() { PolicyKind = PolicyAlias.Simple };

		private EmptyErrorContext() : base(Unit.Default){}

		public PolicyAlias PolicyKind { get; private set; } = PolicyAlias.NotSet;

		public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext(PolicyKind);
	}
}
