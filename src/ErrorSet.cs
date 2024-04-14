using System;
using System.Collections.Generic;
using static PoliNorError.ErrorSet;

namespace PoliNorError
{
	public interface IErrorSet
	{
		IEnumerable<ErrorSetItem> Items { get; }
	}

	public sealed class ErrorSet : IErrorSet
	{
		private readonly HashSet<ErrorSetItem> _set;

		public static ErrorSet FromError<TException>() where TException : Exception
		{
			return new ErrorSet().WithError<TException>();
		}

		public ErrorSet WithError<TException>() where TException : Exception
		{
			_set.Add(new ErrorSetItem(typeof(TException), ErrorSetItem.ItemType.Error));
			return this;
		}

		private ErrorSet()
		{
			_set = new HashSet<ErrorSetItem>();
		}

		public IEnumerable<ErrorSetItem> Items => _set;

		public sealed class ErrorSetItem : IEquatable<ErrorSetItem>
		{
			public ErrorSetItem(Type errorType, ItemType errorKind)
			{
				ErrorType = errorType;
				ErrorKind = errorKind;
			}

			public Type ErrorType { get; }

			public ItemType ErrorKind { get; }

			public override bool Equals(object obj) => obj is ErrorSetItem item && Equals(item);

			public bool Equals(ErrorSetItem other) => ErrorType == other.ErrorType && ErrorKind == other.ErrorKind;

			public override int GetHashCode() => (ErrorKind, ErrorType).GetHashCode();

			public enum ItemType
			{
				None,
				Error
			}
		}
	}
}
