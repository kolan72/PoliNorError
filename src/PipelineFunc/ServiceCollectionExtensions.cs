using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PoliNorError
{
	/// <summary>
	/// Extension methods for registering and resolving <see cref="PipelineFunc{TIn,TOut}"/>
	/// pipelines through Microsoft.Extensions.DependencyInjection.
	/// </summary>
	/// <remarks>
	/// Every <c>AddPipelineFunc</c> overload registers a singleton <see cref="PipelineFuncRegistry"/>,
	/// a singleton <c>IPipelineFunc{TIn,TOut}</c> and a resolution trigger, so the registry
	/// does not have to be wired up manually. Registration keys are exclusive: registering two
	/// pipelines with the same input type, output type and name throws when they are materialized.
	/// Pipelines are functionally immutable after materialization; any
	/// <c>ConfigureErrorProcessors</c> calls must be completed before the pipeline is built.
	/// </remarks>
	public static class PipelineFuncServiceCollectionExtensions
	{
		/// <summary>
		/// Registers a single-step pipeline pipeline built with the default <see cref="SimplePolicy"/>.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="func">The function executed under the policy.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.
		/// When not supplied, the key is the name derived from <typeparamref name="TIn"/>.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFunc<TIn, TOut>(this IServiceCollection services, Func<TIn, TOut> func, string pipelineName = null)
		{
			if (func is null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			return services.AddPipelineFunc<TIn, TOut>(_ => PipelineFuncBuilder.StartWith<TIn, TOut>(func), pipelineName);
		}

		/// <summary>
		/// Registers a single-step pipeline built with the specified policy.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="func">The function executed under the policy.</param>
		/// <param name="policy">The policy controlling execution of <paramref name="func"/>.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.
		/// When not supplied, the key is the name derived from <typeparamref name="TIn"/>.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFunc<TIn, TOut>(this IServiceCollection services, Func<TIn, TOut> func, IPolicyBase policy, string pipelineName = null)
		{
			if (func is null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			if (policy is null)
			{
				throw new ArgumentNullException(nameof(policy));
			}

			return services.AddPipelineFunc<TIn, TOut>(_ => PipelineFuncBuilder.StartWith<TIn, TOut>(func, policy), pipelineName);
		}

		/// <summary>
		/// Registers a single-step pipeline built with a retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="func">The function executed under the retry policy.</param>
		/// <param name="retryCount">The number of retry attempts.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFuncWithRetry<TIn, TOut>(this IServiceCollection services, Func<TIn, TOut> func, int retryCount, RetryDelay retryDelay = null, string pipelineName = null)
		{
			if (func is null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			return services.AddPipelineFunc<TIn, TOut>(_ => PipelineFuncBuilder.StartWithRetry<TIn, TOut>(func, retryCount, retryDelay), pipelineName);
		}

		/// <summary>
		/// Registers a single-step pipeline built with an infinite-retry policy.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="func">The function executed under the retry policy.</param>
		/// <param name="retryDelay">Optional retry delay configuration.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFuncWithInfiniteRetry<TIn, TOut>(this IServiceCollection services, Func<TIn, TOut> func, RetryDelay retryDelay = null, string pipelineName = null)
		{
			if (func is null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			return services.AddPipelineFunc<TIn, TOut>(_ => PipelineFuncBuilder.StartWithInfiniteRetry<TIn, TOut>(func, retryDelay), pipelineName);
		}

		/// <summary>
		/// Registers a single-step pipeline built with a fallback policy.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="func">The function executed under the fallback policy.</param>
		/// <param name="fallbackFunc">The fallback executed when <paramref name="func"/> fails.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFuncWithFallback<TIn, TOut>(this IServiceCollection services, Func<TIn, TOut> func, Func<TOut> fallbackFunc, string pipelineName = null)
		{
			if (func is null)
			{
				throw new ArgumentNullException(nameof(func));
			}

			if (fallbackFunc is null)
			{
				throw new ArgumentNullException(nameof(fallbackFunc));
			}

			return services.AddPipelineFunc<TIn, TOut>(_ => PipelineFuncBuilder.StartWithFallback<TIn, TOut>(func, fallbackFunc), pipelineName);
		}

		/// <summary>
		/// Registers a pipeline whose stages are defined by the builder factory. The factory runs
		/// against the scope's <see cref="IServiceProvider"/> and the pipeline is materialized once
		/// (as a singleton <c>IPipelineFunc{TIn,TOut}</c>) and registered in the
		/// <see cref="PipelineFuncRegistry"/> when first resolved.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="builderFactory">Factory producing the <see cref="IPipelineFuncBuilder{TIn,TOut}"/>
		/// for this pipeline. All error-processor configuration must be completed inside it.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.
		/// When not supplied, the key is the name derived from <typeparamref name="TIn"/>.</param>
		/// <returns>The same service collection.</returns>
		public static IServiceCollection AddPipelineFunc<TIn, TOut>(
			this IServiceCollection services,
			Func<IServiceProvider, IPipelineFuncBuilder<TIn, TOut>> builderFactory,
			string pipelineName = null)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (builderFactory is null)
			{
				throw new ArgumentNullException(nameof(builderFactory));
			}

			return services.RegisterPipeline<TIn, TOut>(provider =>
			{
				var builder = builderFactory(provider);
				if (builder is null)
				{
					throw new InvalidOperationException(
						$"The pipeline builder factory returned null for {typeof(TIn)} -> {typeof(TOut)}.");
				}

				return builder.ToPipelineFunc(pipelineName);
			}, pipelineName);
		}

		/// <summary>
		/// Registers a pipeline defined by the builder factory, optionally applying error processors
		/// to the pipeline's final policy before it is materialized.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="builderFactory">Factory producing the <see cref="IPipelineFuncBuilder{TIn,TOut}"/>
		/// for this pipeline. All further <c>ConfigureErrorProcessors</c> calls must be completed inside it.</param>
		/// <param name="configureErrorProcessors">Optional action configuring the error processors of the
		/// pipeline's final policy, applied before the pipeline is built.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.</param>
		/// <returns>The same service collection.</returns>
		/// <exception cref="InvalidOperationException">
		/// The builder does not support configuration (a foreign <see cref="IPipelineFuncBuilder{TIn,TOut}"/> implementation).
		/// </exception>
		public static IServiceCollection AddPipelineFuncChain<TIn, TOut>(
			this IServiceCollection services,
			Func<IServiceProvider, IPipelineFuncBuilder<TIn, TOut>> builderFactory,
			Action<BulkErrorProcessor> configureErrorProcessors = null,
			string pipelineName = null)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (builderFactory is null)
			{
				throw new ArgumentNullException(nameof(builderFactory));
			}

			return services.RegisterPipeline<TIn, TOut>(provider =>
			{
				var builder = builderFactory(provider);
				if (builder is null)
				{
					throw new InvalidOperationException(
						$"The pipeline builder factory returned null for {typeof(TIn)} -> {typeof(TOut)}.");
				}

				if (configureErrorProcessors != null)
				{
					if (!(builder is IPipelineFuncBuilderInternals internals))
					{
						throw new InvalidOperationException(
							$"Error processor configuration is only supported for builders created by {nameof(PipelineFuncBuilder)}. " +
							$"The factory returned {builder.GetType().Name}.");
					}

					internals.SetConfigure(configureErrorProcessors);
				}

				return builder.ToPipelineFunc(pipelineName);
			}, pipelineName);
		}

		/// <summary>
		/// Registers a pre-built <see cref="PipelineFunc{TIn,TOut}"/> instance as a singleton.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="services">The service collection.</param>
		/// <param name="pipelineFunc">The pre-built pipeline.</param>
		/// <param name="pipelineName">An optional key to register the pipeline under.
		/// When not supplied, the key is the name derived from <typeparamref name="TIn"/>.</param>
		/// <returns>The same service collection.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFunc"/> is <c>null</c>.</exception>
		public static IServiceCollection AddPipelineFunc<TIn, TOut>(this IServiceCollection services, PipelineFunc<TIn, TOut> pipelineFunc, string pipelineName = null)
		{
			if (services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if (pipelineFunc is null)
			{
				throw new ArgumentNullException(nameof(pipelineFunc));
			}

			return services.RegisterPipeline<TIn, TOut>(_ => pipelineFunc, pipelineName);
		}

		/// <summary>
		/// Resolves the pipeline registered under the default key (the name derived from
		/// <typeparamref name="TIn"/>).
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="serviceProvider">The service provider built from the augmented service collection.</param>
		/// <returns>The registered pipeline function.</returns>
		/// <exception cref="InvalidOperationException">No pipeline registry or no matching pipeline is registered.</exception>
		public static IPipelineFunc<TIn, TOut> GetPipelineFunc<TIn, TOut>(this IServiceProvider serviceProvider)
		{
			return GetPipelineFunc<TIn, TOut>(serviceProvider, null);
		}

		/// <summary>
		/// Resolves the pipeline registered under the specified key.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="serviceProvider">The service provider built from the augmented service collection.</param>
		/// <param name="pipelineName">The key the pipeline was registered under. When <c>null</c>,
		/// the default key (the name derived from <typeparamref name="TIn"/>) is used.</param>
		/// <returns>The registered pipeline function.</returns>
		/// <exception cref="InvalidOperationException">No pipeline registry or no matching pipeline is registered;
		/// the message lists the registered names.</exception>
		public static IPipelineFunc<TIn, TOut> GetPipelineFunc<TIn, TOut>(this IServiceProvider serviceProvider, string pipelineName)
		{
			if (serviceProvider is null)
			{
				throw new ArgumentNullException(nameof(serviceProvider));
			}

			var registry = serviceProvider.GetService<PipelineFuncRegistry>();
			if (registry is null)
			{
				throw new InvalidOperationException(
					$"No {nameof(PipelineFuncRegistry)} is registered in the service provider. " +
					$"Call {nameof(PipelineFuncServiceCollectionExtensions)}.{nameof(AddPipelineFunc)} first.");
			}

			MaterializePipelines(serviceProvider);
			return registry.Get<TIn, TOut>(pipelineName);
		}

		/// <summary>
		/// Gets every pipeline function registered in the container, regardless of its type pair.
		/// </summary>
		/// <param name="serviceProvider">The service provider built from the augmented service collection.</param>
		/// <returns>All registered pipelines; empty when nothing is registered.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <c>null</c>.</exception>
		public static List<IPipelineFunc> GetAllPipelineFuncs(this IServiceProvider serviceProvider)
		{
			if (serviceProvider is null)
			{
				throw new ArgumentNullException(nameof(serviceProvider));
			}

			MaterializePipelines(serviceProvider);

			var registry = serviceProvider.GetService<PipelineFuncRegistry>();
			return registry?.GetAllPipelineFuncs() ?? new List<IPipelineFunc>();
		}

		/// <summary>
		/// Synchronously invokes the pipeline delegate with the specified input.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFunc">The pipeline function.</param>
		/// <param name="input">The pipeline input.</param>
		/// <param name="token">A cancellation token; defaults to <see cref="CancellationToken.None"/>.</param>
		/// <returns>The pipeline result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFunc"/> is <c>null</c>.</exception>
		public static PipelineResult<TOut> Invoke<TIn, TOut>(this IPipelineFunc<TIn, TOut> pipelineFunc, TIn input, CancellationToken token = default)
		{
			if (pipelineFunc is null)
			{
				throw new ArgumentNullException(nameof(pipelineFunc));
			}

			return pipelineFunc.GetPipelineDelegate()(input, token);
		}

		/// <summary>
		/// Invokes the pipeline delegate with the specified input.
		/// The pipeline delegates are synchronous, so this is a thin adapter that wraps the
		/// already-known result in a completed task.
		/// </summary>
		/// <typeparam name="TIn">The input type of the pipeline.</typeparam>
		/// <typeparam name="TOut">The output type of the pipeline.</typeparam>
		/// <param name="pipelineFunc">The pipeline function.</param>
		/// <param name="input">The pipeline input.</param>
		/// <param name="token">A cancellation token; defaults to <see cref="CancellationToken.None"/>.</param>
		/// <returns>A completed task carrying the pipeline result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="pipelineFunc"/> is <c>null</c>.</exception>
		public static Task<PipelineResult<TOut>> InvokeAsync<TIn, TOut>(this IPipelineFunc<TIn, TOut> pipelineFunc, TIn input, CancellationToken token = default)
		{
			return Task.FromResult(pipelineFunc.Invoke(input, token));
		}

		private static IServiceCollection RegisterPipeline<TIn, TOut>(
			this IServiceCollection services,
			Func<IServiceProvider, PipelineFunc<TIn, TOut>> pipelineFactory,
			string pipelineName)
		{
			services.TryAddSingleton<PipelineFuncRegistry>();

			services.AddSingleton<IPipelineFunc<TIn, TOut>>(provider =>
			{
				var pipelineFunc = pipelineFactory(provider);
				provider.GetRequiredService<PipelineFuncRegistry>().Register(pipelineFunc, pipelineName);
				return pipelineFunc;
			});

			services.AddSingleton(new PipelineFuncResolution(provider =>
				provider.GetServices<IPipelineFunc<TIn, TOut>>()));

			return services;
		}

		private static void MaterializePipelines(IServiceProvider serviceProvider)
		{
			foreach (var resolution in serviceProvider.GetServices<PipelineFuncResolution>())
			{
				resolution.Materialize(serviceProvider);
			}
		}
	}
}