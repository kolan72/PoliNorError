using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace PoliNorError
{
	/// <summary>
	/// Thread-safe, in-memory registry for <see cref="PipelineFunc{TIn,TOut}"/> instances,
	/// keyed by input type, output type and pipeline name.
	/// </summary>
	/// <remarks>
	/// Parameterless operations (<see cref="Get{TIn,TOut}()"/>, <see cref="Contains{TIn,TOut}()"/> and
	/// <see cref="Unregister{TIn,TOut}()"/>) use the default key: the name derived from the input type
	/// (see <see cref="GetDefaultName(Type)"/>). This is the key under which pipelines registered
	/// without an explicit name are stored, so parameterless lookup always round-trips.
	/// </remarks>
	public sealed class PipelineFuncRegistry : IPipelineFuncRegistrationService
	{
		private sealed class RegistryEntry
		{
			public RegistryEntry(IPipelineFunc untypedPipelineFunc, object typedPipelineFunc, string name, Type outType)
			{
				UntypedPipelineFunc = untypedPipelineFunc;
				TypedPipelineFunc = typedPipelineFunc;
				Name = name;
				OutType = outType;
			}

			public IPipelineFunc UntypedPipelineFunc { get; }
			public object TypedPipelineFunc { get; }
			public string Name { get; }
			public Type OutType { get; }
		}

		private readonly ConcurrentDictionary<(Type InType, Type OutType, string Name), RegistryEntry> _registrations
			= new ConcurrentDictionary<(Type, Type, string), RegistryEntry>();

		/// <summary>
		/// Registers a <see cref="PipelineFunc{TIn,TOut}"/> under the default key,
		/// which is the name derived from the input type.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFunc">The pipeline to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFunc"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">A pipeline with the same key is already registered.</exception>
		public void Register<TIn, TOut>(PipelineFunc<TIn, TOut> pipelineFunc)
		{
			Register(pipelineFunc, GetDefaultName(typeof(TIn)));
		}

		/// <summary>
		/// Registers a <see cref="PipelineFunc{TIn,TOut}"/> under the specified name.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFunc">The pipeline to register.</param>
		/// <param name="name">The key to register under. When <c>null</c>, the default key
		/// (the name derived from the input type) is used.</param>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFunc"/> is <c>null</c>.</exception>
		/// <exception cref="InvalidOperationException">A pipeline with the same key is already registered.</exception>
		public void Register<TIn, TOut>(PipelineFunc<TIn, TOut> pipelineFunc, string name)
		{
			if (pipelineFunc is null)
			{
				throw new ArgumentNullException(nameof(pipelineFunc));
			}

			var key = (typeof(TIn), typeof(TOut), name ?? GetDefaultName(typeof(TIn)));

			var entry = new RegistryEntry(pipelineFunc, pipelineFunc, key.Item3, typeof(TOut));

			var added = _registrations.TryAdd(key, entry);
			if (!added)
			{
				var registeredNames = string.Join(", ", GetNamesForTypes(typeof(TIn), typeof(TOut)).Select(n => $"'{n}'"));
				throw new InvalidOperationException(
					$"A pipeline with the key (input type: {GetTypeName(typeof(TIn))}, output type: {GetTypeName(typeof(TOut))}, name: '{key.Item3}') is already registered. " +
					$"Registered names for this type pair: [{registeredNames}].");
			}
		}

		/// <summary>
		/// Gets the registered <see cref="PipelineFunc{TIn,TOut}"/> under the default key,
		/// which is the name derived from the input type.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <returns>The registered pipeline.</returns>
		/// <exception cref="InvalidOperationException">No pipeline with the given type pair is registered under the default key.</exception>
		public PipelineFunc<TIn, TOut> Get<TIn, TOut>()
		{
			return Get<TIn, TOut>(GetDefaultName(typeof(TIn)));
		}

		/// <summary>
		/// Gets the registered <see cref="PipelineFunc{TIn,TOut}"/> with the specified name.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="name">The name of the pipeline to retrieve. When <c>null</c>, the default key is used.</param>
		/// <returns>The registered pipeline.</returns>
		/// <exception cref="InvalidOperationException">
		/// No pipeline with the given type pair and name is registered; the message lists the registered names.
		/// </exception>
		public PipelineFunc<TIn, TOut> Get<TIn, TOut>(string name)
		{
			var key = (typeof(TIn), typeof(TOut), name ?? GetDefaultName(typeof(TIn)));

			if (!_registrations.TryGetValue(key, out var entry))
			{
				var registeredNames = GetNamesForTypes(typeof(TIn), typeof(TOut)).Select(n => $"'{n}'").ToArray();
				var namesPart = registeredNames.Length == 0
					? "the type pair has no registrations at all."
					: $"registered names for this type pair: [{string.Join(", ", registeredNames)}].";
				throw new InvalidOperationException(
					$"No pipeline with the key (input type: {GetTypeName(typeof(TIn))}, output type: {GetTypeName(typeof(TOut))}, name: '{key.Item3}') is registered; {namesPart}");
			}

			return (PipelineFunc<TIn, TOut>)entry.TypedPipelineFunc;
		}

		/// <summary>
		/// Gets all registered pipelines in the registry, regardless of their type pairs.
		/// </summary>
		/// <returns>A list of all registered pipelines; empty when nothing is registered.</returns>
		public List<IPipelineFunc> GetAllPipelineFuncs()
		{
			return _registrations.Values.Select(e => e.UntypedPipelineFunc).ToList();
		}

		/// <summary>
		/// Determines whether a pipeline is registered for the type pair under the default key
		/// (the name derived from the input type), i.e. whether <see cref="Get{TIn,TOut}()"/> would succeed.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <returns><c>true</c> if a pipeline is registered; otherwise <c>false</c>.</returns>
		public bool Contains<TIn, TOut>()
		{
			return Contains<TIn, TOut>(GetDefaultName(typeof(TIn)));
		}

		/// <summary>
		/// Determines whether a pipeline with the given type pair and name is registered.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="name">The name of the pipeline to check. When <c>null</c>, the default key is used.</param>
		/// <returns><c>true</c> if a pipeline with the given name is registered; otherwise <c>false</c>.</returns>
		public bool Contains<TIn, TOut>(string name)
		{
			var key = (typeof(TIn), typeof(TOut), name ?? GetDefaultName(typeof(TIn)));
			return _registrations.ContainsKey(key);
		}

		/// <summary>
		/// Unregisters the <see cref="PipelineFunc{TIn,TOut}"/> under the default key,
		/// which is the name derived from the input type.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		public void Unregister<TIn, TOut>()
		{
			Unregister<TIn, TOut>(GetDefaultName(typeof(TIn)));
		}

		/// <summary>
		/// Unregisters the <see cref="PipelineFunc{TIn,TOut}"/> with the specified name.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="name">The name of the pipeline to unregister. When <c>null</c>, the default key is used.</param>
		public void Unregister<TIn, TOut>(string name)
		{
			var key = (typeof(TIn), typeof(TOut), name ?? GetDefaultName(typeof(TIn)));
			_registrations.TryRemove(key, out _);
		}

		/// <summary>
		/// Gets the default registry key for the given input type: the type name without the generic arity suffix
		/// (for example <c>Int32</c> or <c>List</c>).
		/// </summary>
		/// <param name="type">The input type of the pipeline.</param>
		/// <returns>The default key.</returns>
		internal static string GetDefaultName(Type type)
		{
			var name = type.Name;
			var backtickIndex = name.IndexOf('`');
			return backtickIndex < 0 ? name : name.Substring(0, backtickIndex);
		}

		private IEnumerable<string> GetNamesForTypes(Type inType, Type outType)
		{
			return _registrations
				.Where(kv => kv.Key.InType == inType && kv.Key.OutType == outType)
				.Select(kv => kv.Key.Item3)
				.Distinct()
				.OrderBy(n => n, StringComparer.Ordinal);
		}

		private static string GetTypeName(Type type)
		{
			var name = type.FullName ?? type.Name;
			var backtickIndex = name.IndexOf('`');
			return backtickIndex < 0 ? name : name.Substring(0, backtickIndex);
		}
	}
}