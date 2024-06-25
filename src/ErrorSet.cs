using System;
using System.Collections.Generic;
using static PoliNorError.ErrorSet;

namespace PoliNorError
{
	/// <summary>
	/// Represents a set of <see cref="ErrorSetItem"/> items.
	/// </summary>
	public interface IErrorSet
	{
		/// <summary>
		/// Gets <see cref="ErrorSetItem"/> items
		/// </summary>
		IEnumerable<ErrorSetItem> Items { get; }
	}

	///<inheritdoc cref = "IErrorSet"/>
	public sealed class ErrorSet : IErrorSet
	{
		private readonly HashSet<ErrorSetItem> _set;

		/// <summary>
		/// Creates  the <see cref="ErrorSet"/> set and adds the <see cref="ErrorSetItem"/> item with the <typeparamref name="TException"/> type and <see cref="ErrorSetItem.ItemType.Error"/> kind to the new set.
		/// </summary>
		/// <typeparam name="TException">The type of exception.</typeparam>
		/// <returns></returns>
		public static ErrorSet FromError<TException>() where TException : Exception
		{
			return new ErrorSet().WithError<TException>();
		}

		/// <summary>
		/// Creates  the <see cref="ErrorSet"/> set and adds the <see cref="ErrorSetItem"/> item with the <typeparamref name="TInnerException"/> type and <see cref="ErrorSetItem.ItemType.InnerError"/> kind to the new set.
		/// </summary>
		/// <typeparam name="TInnerException">The type of inner exception.</typeparam>
		/// <returns></returns>
		public static ErrorSet FromInnerError<TInnerException>() where TInnerException : Exception
		{
			return new ErrorSet().WithInnerError<TInnerException>();
		}

		/// <summary>
		/// Adds the <see cref="ErrorSetItem"/> item with the <typeparamref name="TException"/> type and <see cref="ErrorSetItem.ItemType.Error"/> kind.
		/// </summary>
		/// <typeparam name="TException">The type of exception.</typeparam>
		/// <returns></returns>
		public ErrorSet WithError<TException>() where TException : Exception
		{
			_set.Add(new ErrorSetItem(typeof(TException), ErrorSetItem.ItemType.Error));
			return this;
		}

		/// <summary>
		/// Adds the <see cref="ErrorSetItem"/> item with the <typeparamref name="TInnerException"/> type and <see cref="ErrorSetItem.ItemType.InnerError"/> kind.
		/// </summary>
		/// <typeparam name="TInnerException">The type of inner exception.</typeparam>
		/// <returns></returns>
		public ErrorSet WithInnerError<TInnerException>() where TInnerException : Exception
		{
			_set.Add(new ErrorSetItem(typeof(TInnerException), ErrorSetItem.ItemType.InnerError));
			return this;
		}

		private ErrorSet()
		{
			_set = new HashSet<ErrorSetItem>();
		}

		///<inheritdoc cref = "IErrorSet.Items"/>
		public IEnumerable<ErrorSetItem> Items => _set;

		/// <summary>
		/// Represents the type and kind of an exception.
		/// </summary>
		public sealed class ErrorSetItem : IEquatable<ErrorSetItem>
		{
			public ErrorSetItem(Type errorType, ItemType errorKind)
			{
				ErrorType = errorType;
				ErrorKind = errorKind;
			}

			/// <summary>
			/// Type of exception
			/// </summary>
			public Type ErrorType { get; }

			/// <summary>
			///  Kind of exception
			/// </summary>
			public ItemType ErrorKind { get; }

			public override bool Equals(object obj) => obj is ErrorSetItem item && Equals(item);

			public bool Equals(ErrorSetItem other) => ErrorType == other.ErrorType && ErrorKind == other.ErrorKind;

			public override int GetHashCode() => (ErrorKind, ErrorType).GetHashCode();

			/// <summary>
			/// Represents what kind of exception the error type is for.
			/// </summary>
			public enum ItemType
			{
				None,
				/// <summary>
				/// Represents the exception type for an exception itself
				/// </summary>
				Error,
				/// <summary>
				/// Represents the exception type for an inner exception
				/// </summary>
				InnerError
			}
		}
	}
}
