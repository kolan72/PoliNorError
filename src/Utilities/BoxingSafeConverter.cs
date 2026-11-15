using System;
using System.Linq.Expressions;

namespace PoliNorError
{
#pragma warning disable S1133 // Deprecated code should be removed
	[Obsolete("This class is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
	internal sealed class BoxingSafeConverter<TIn, TOut>
	{
		public static readonly BoxingSafeConverter<TIn, TOut> Instance = new BoxingSafeConverter<TIn, TOut>();

		public Func<TIn, TOut> Convert { get; }

		private BoxingSafeConverter()
		{
			if (typeof(TIn) != typeof(TOut))
			{
				throw new InvalidOperationException("Both generic type parameters must represent the same type.");
			}
			var paramExpr = Expression.Parameter(typeof(TIn));
			Convert =
				Expression.Lambda<Func<TIn, TOut>>(paramExpr, // this conversion is legal as typeof(TIn) = typeof(TOut)
					paramExpr)
					.Compile();
		}
	}
}
