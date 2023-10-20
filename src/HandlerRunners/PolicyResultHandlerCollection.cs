using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	internal class PolicyResultHandlerCollection
	{
		public PolicyResultHandlerCollection()
		{
			Handlers = new List<IHandlerRunner>();
			GenericHandlers = new List<IHandlerRunnerT>();
		}

		internal void AddHandler(Func<PolicyResult, Task> func)
		{
			AddHandler((pr, _) => func(pr));
		}

		internal void AddHandler(Func<PolicyResult, CancellationToken, Task> func)
		{
			var handler = new ASyncHandlerRunner(func, Handlers.Count);
			Handlers.Add(handler);
		}

		internal void AddHandler<T>(Func<PolicyResult<T>, Task> func)
		{
			AddHandler<T>((pr, _) => func(pr));
		}

		internal void AddHandler<T>(Func<PolicyResult<T>, CancellationToken, Task> func)
		{
			var handler = ASyncHandlerRunnerT.Create(func, Handlers.Count);
			GenericHandlers.Add(handler);
		}

		internal void AddHandler(Action<PolicyResult> act)
		{
			AddHandler((pr, _) => act(pr));
		}

		internal void AddHandler(Action<PolicyResult, CancellationToken> act)
		{
			var handler = new SyncHandlerRunner(act, Handlers.Count);
			Handlers.Add(handler);
		}

		internal void AddHandler<T>(Action<PolicyResult<T>> act)
		{
			AddHandler<T>((pr, _) => act(pr));
		}

		internal void AddHandler<T>(Action<PolicyResult<T>, CancellationToken> act)
		{
			var handler = SyncHandlerRunnerT.Create(act, Handlers.Count);
			GenericHandlers.Add(handler);
		}

		internal List<IHandlerRunner> Handlers { get; }
		internal List<IHandlerRunnerT> GenericHandlers { get; }
	}
}
