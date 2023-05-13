using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	public interface IErrorsAggregator<out T>
	{
		T Aggregate(IEnumerable<Exception> exceptions);
	}

	public interface IErrorsToStringAggregator : IErrorsAggregator<string> { }

	public class DefaultErrorsToStringAggregator : IErrorsToStringAggregator
	{
		private readonly string _delimiter;

		public DefaultErrorsToStringAggregator(string delimiter = ";")
		{
			_delimiter = delimiter;
		}

		public string Aggregate(IEnumerable<Exception> exceptions) => string.Join(_delimiter, exceptions.Select(ie => ie.Message));
	}
}
