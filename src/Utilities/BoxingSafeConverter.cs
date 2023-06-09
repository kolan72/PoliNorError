﻿using System;
using System.Linq.Expressions;

namespace PoliNorError
{
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
