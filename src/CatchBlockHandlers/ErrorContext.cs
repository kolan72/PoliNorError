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

	internal class EmptyErrorContext : ErrorContext<Unit>
	{
		public static EmptyErrorContext Default { get; } = new EmptyErrorContext();

		public static EmptyErrorContext DefaultFallback { get; } = new EmptyErrorContext() { PolicyKind = PolicyAlias.Fallback };

		public static EmptyErrorContext DefaultSimple { get; } = new EmptyErrorContext() { PolicyKind = PolicyAlias.Simple };

		protected EmptyErrorContext() : base(Unit.Default){}

		public PolicyAlias PolicyKind { get; private set; } = PolicyAlias.NotSet;

		public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext(PolicyKind);
	}

	internal class EmptyErrorContext<TParam> : EmptyErrorContext
	{
		public EmptyErrorContext(TParam param)
		{
			Param = param;
		}

		public TParam Param { get; private set; }

		public override ProcessingErrorContext ToProcessingErrorContext() => new ProcessingErrorContext<TParam>(PolicyKind, Param);
	}
}
