using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	public abstract partial class PolicyProcessor : IPolicyProcessor
	{
		protected IBulkErrorProcessor _bulkErrorProcessor;

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This field is obsolete.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected bool _isPolicyAliasSet;

		protected PolicyProcessor(IBulkErrorProcessor bulkErrorProcessor = null) : this(new ExceptionFilter(), bulkErrorProcessor)
		{ }

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete.  Use constructors without the PolicyAlias parameter instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
		protected PolicyProcessor(PolicyAlias policyAlias, IBulkErrorProcessor bulkErrorProcessor = null) : this(policyAlias, new ExceptionFilter(), bulkErrorProcessor)
		{ }

		protected PolicyProcessor(ExceptionFilter exceptionFilter, IBulkErrorProcessor bulkErrorProcessor = null)
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor();
			ErrorFilter = exceptionFilter;
		}

#pragma warning disable S1133 // Deprecated code should be removed
		[Obsolete("This constructor is obsolete.  Use constructors without the PolicyAlias parameter instead.")]
#pragma warning restore S1133 // Deprecated code should be removed
#pragma warning disable RCS1163 // Unused parameter.
		protected PolicyProcessor(PolicyAlias policyAlias, ExceptionFilter exceptionFilter, IBulkErrorProcessor bulkErrorProcessor = null)
#pragma warning restore RCS1163 // Unused parameter.
		{
			_bulkErrorProcessor = bulkErrorProcessor ?? new BulkErrorProcessor();
			_isPolicyAliasSet = bulkErrorProcessor == null;
			ErrorFilter = exceptionFilter;
		}

		/// <summary>
		/// Adds an error processor to the bulk error processor.
		/// </summary>
		/// <param name="newErrorProcessor">The error processor to add.</param>
		/// <exception cref="ArgumentNullException">Thrown when newErrorProcessor is null.</exception>
		public void AddErrorProcessor(IErrorProcessor newErrorProcessor)
		{
			_bulkErrorProcessor.AddProcessor(newErrorProcessor);
		}

		/// <summary>
		/// Gets the collection of included error filters that determine which exceptions should be handled by this policy processor.
		/// </summary>
		/// <returns>An enumerable of expressions that define included error filters.</returns>
		public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => ErrorFilter.IncludedErrorFilters;

		/// <summary>
		/// Gets the collection of excluded error filters that determine which exceptions should not be handled by this policy processor.
		/// </summary>
		/// <returns>An enumerable of expressions that define excluded error filters.</returns>
		public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => ErrorFilter.ExcludedErrorFilters;

		/// <summary>
		/// Gets the exception filter used to determine which exceptions should be handled by this policy processor.
		/// </summary>
		/// <returns>The exception filter instance.</returns>
		public ExceptionFilter ErrorFilter { get; }

		/// <summary>
		/// Creates a synchronous catch block handler for the specified error context type.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <param name="policyResult">The policy result to update during error handling.</param>
		/// <param name="token">The cancellation token.</param>
		/// <param name="policyRuleFunc">Optional policy rule function to determine if the exception should be handled.</param>
		/// <returns>A new instance of PolicyProcessorCatchBlockSyncHandler.</returns>
		internal PolicyProcessorCatchBlockSyncHandler<T> GetCatchBlockSyncHandler<T>(PolicyResult policyResult, CancellationToken token, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			return new PolicyProcessorCatchBlockSyncHandler<T>(policyResult,
																_bulkErrorProcessor,
																token,
																ErrorFilter.GetCanHandle(),
																policyRuleFunc);
		}

		internal PolicyProcessorCatchBlockAsyncHandler<T> GetCatchBlockAsyncHandler<T>(PolicyResult policyResult, bool configAwait, CancellationToken token, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			return new PolicyProcessorCatchBlockAsyncHandler<T>(policyResult,
																_bulkErrorProcessor,
																configAwait,
																token,
																ErrorFilter.GetCanHandle(),
																policyRuleFunc);
		}

		protected internal async Task<ExceptionHandlingResult> HandleExceptionAsync<T>(
				Exception ex,
				PolicyResult policyResult,
				ErrorContext<T> errorContext,
				Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> errorSaver,
				Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc,
				ExceptionHandlingBehavior handlingBehavior,
				ProcessingOrder processingOrder,
				ErrorProcessingCancellationEffect cancellationEffect,
				bool configureAwait,
				CancellationToken token)
		{
			var saver = errorSaver ?? CreateDefaultAsyncErrorSaver<T>();
			if (handlingBehavior != ExceptionHandlingBehavior.ConditionalRethrow)
			{
				await saver(policyResult, ex, errorContext, configureAwait, token).ConfigureAwait(configureAwait);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
					return ExceptionHandlingResult.Handled;
				}
			}

			var (result, error) = EvaluateExceptionFilter(policyResult, ex, handlingBehavior);

			if (!(error is null) && handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
			{
				await saver(policyResult, ex, errorContext, configureAwait, token).ConfigureAwait(configureAwait);
			}

			if (result != ExceptionHandlingResult.Accepted)
			{
				return result;
			}

			if (handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
			{
				await saver(policyResult, ex, errorContext, configureAwait, token).ConfigureAwait(configureAwait);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
					return ExceptionHandlingResult.Handled;
				}
			}

			if (processingOrder == ProcessingOrder.EvaluateThenProcess)
			{
				var ruleResult = await EvaluatePolicyRuleAsync(ex, policyResult, errorContext, policyRuleFunc, configureAwait, token).ConfigureAwait(configureAwait);
				if (ruleResult != ExceptionHandlingResult.Accepted)
				{
					return ruleResult;
				}

				var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), configureAwait, token).ConfigureAwait(configureAwait);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);
				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
				}
				return ExceptionHandlingResult.Handled;
			}
			else
			{
				var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), configureAwait, token).ConfigureAwait(configureAwait);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);
				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
					return ExceptionHandlingResult.Handled;
				}
				return await EvaluatePolicyRuleAsync(ex, policyResult, errorContext, policyRuleFunc, configureAwait, token).ConfigureAwait(configureAwait);
			}
		}

		protected internal ExceptionHandlingResult HandleException<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> errorSaver,
			Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
			ExceptionHandlingBehavior handlingBehavior,
			ProcessingOrder processingOrder,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			var saver = errorSaver ?? CreateDefaultErrorSaver<T>();
			if (handlingBehavior != ExceptionHandlingBehavior.ConditionalRethrow)
			{
				saver(policyResult, ex, errorContext, token);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
					return ExceptionHandlingResult.Handled;
				}
			}

			var (result, error) = EvaluateExceptionFilter(policyResult, ex, handlingBehavior);

			if (!(error is null) && handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
			{
				saver(policyResult, ex, errorContext, token);
			}

			if (result != ExceptionHandlingResult.Accepted)
			{
				return result;
			}

			if (handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
			{
				saver(policyResult, ex, errorContext, token);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled(new OperationCanceledException(token));
					return ExceptionHandlingResult.Handled;
				}
			}

			if (processingOrder == ProcessingOrder.EvaluateThenProcess)
			{
				var ruleResult = EvaluatePolicyRule(ex, policyResult, errorContext, policyRuleFunc, token);
				if (ruleResult != ExceptionHandlingResult.Accepted)
				{
					return ruleResult;
				}

				var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);
				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
				}

				return ExceptionHandlingResult.Handled;
			}
			else
			{
				var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);

				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled(bulkProcessResult.CancellationException);
					return ExceptionHandlingResult.Handled;
				}

				return EvaluatePolicyRule(ex, policyResult, errorContext, policyRuleFunc, token);
			}
		}

		/// <summary>
		/// Gets the default error saver for policy results with Unit error context.
		/// </summary>
		internal static Action<PolicyResult, Exception, ErrorContext<Unit>, CancellationToken> DefaultErrorSaver { get; } = CreateDefaultErrorSaver<Unit>();

		/// <summary>
		/// Creates a default error saver that adds the exception to the policy result.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <returns>A default error saver action.</returns>
		internal static Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> CreateDefaultErrorSaver<T>() =>
			 (pr, e, _, __) => pr.AddError(e);

		/// <summary>
		/// Gets the default async error saver for policy results with Unit error context.
		/// </summary>
		internal static Func<PolicyResult, Exception, ErrorContext<Unit>, bool, CancellationToken, Task> DefaultAsyncErrorSaver { get; } = CreateDefaultAsyncErrorSaver<Unit>();

		/// <summary>
		/// Creates a default async error saver that adds the exception to the policy result.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <returns>A default async error saver function.</returns>
		internal static Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> CreateDefaultAsyncErrorSaver<T>() =>
			 (pr, e, _, __, ___) => { pr.AddError(e); return Task.CompletedTask; };

		/// <summary>
		/// Gets the default async policy rule that always returns true for Unit error context.
		/// </summary>
		internal static Func<ErrorContext<Unit>, CancellationToken, Task<bool>> DefaultAsyncPolicyRule { get; }= (_, __) => Task.FromResult(true);

		/// <summary>
		/// Gets the default policy rule for Unit error context.
		/// </summary>
		internal static Func<ErrorContext<Unit>, CancellationToken, bool> DefaultPolicyRule { get; } = CreateDefaultPolicyRule<Unit>();

		/// <summary>
		/// Creates a default policy rule that always returns true.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <returns>A default policy rule function.</returns>
		internal static Func<ErrorContext<T>, CancellationToken, bool> CreateDefaultPolicyRule<T>() =>
			(_, __) => true;

		/// <summary>
		/// Evaluates the exception filter against the given exception and determines the handling result.
		/// </summary>
		/// <param name="policyResult">The policy result to update based on the filter evaluation.</param>
		/// <param name="ex">The exception to evaluate.</param>
		/// <param name="handlingBehavior">The exception handling behavior to apply.</param>
		/// <returns>A tuple containing the handling result and any error encountered during evaluation.</returns>
		private (ExceptionHandlingResult, Exception) EvaluateExceptionFilter(PolicyResult policyResult, Exception ex, ExceptionHandlingBehavior handlingBehavior)
		{
			var (filterPassed, error) = RunErrorFilterFunc();
			return (FailPolicyResultIfRequired(), error);
			ExceptionHandlingResult FailPolicyResultIfRequired()
			{
				if (!filterPassed)
				{
					switch (handlingBehavior)
					{
						case ExceptionHandlingBehavior.ConditionalRethrow when !(error is null):
							policyResult.AddCatchBlockError(new CatchBlockException(error, ex, CatchBlockExceptionSource.ErrorFilter, true));
							policyResult.SetFailedAndFilterUnsatisfied();
							return ExceptionHandlingResult.Handled;
						case ExceptionHandlingBehavior.ConditionalRethrow:
							return ExceptionHandlingResult.Rethrow;
						case ExceptionHandlingBehavior.Handle:
							if (!(error is null))
							{
								policyResult.AddCatchBlockError(new CatchBlockException(error, ex, CatchBlockExceptionSource.ErrorFilter, true));
							}
							policyResult.SetFailedAndFilterUnsatisfied();
							return ExceptionHandlingResult.Handled;
						default:
							policyResult.AddCatchBlockError(new CatchBlockException(new NotSupportedException(), ex, CatchBlockExceptionSource.ErrorFilter, true));
							policyResult.SetFailedAndFilterUnsatisfied();
							return ExceptionHandlingResult.Handled;
					}
				}
				return ExceptionHandlingResult.Accepted;
			}

			(bool, Exception) RunErrorFilterFunc()
			{
				try
				{
					return (ErrorFilter.GetCanHandle()(ex), null);
				}
				catch (Exception exIn)
				{
					return (false, exIn);
				}
			}
		}

		/// <summary>
		/// Evaluates a policy rule synchronously to determine if an exception should be handled.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <param name="ex">The exception to evaluate.</param>
		/// <param name="policyResult">The policy result to update based on the rule evaluation.</param>
		/// <param name="errorContext">The error context containing additional information about the exception.</param>
		/// <param name="policyRuleFunc">The policy rule function to evaluate.</param>
		/// <param name="token">The cancellation token.</param>
		/// <returns>The exception handling result based on the policy rule evaluation.</returns>
		private static ExceptionHandlingResult EvaluatePolicyRule<T>(Exception ex, PolicyResult policyResult, ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc, CancellationToken token)
		{
			return HandlePolicyRuleFuncResult(RunPolicyRuleFunc(), ex, policyResult);

			(bool Result, bool IsCanceled, Exception error) RunPolicyRuleFunc()
			{
				try
				{
					var result = policyRuleFunc?.Invoke(errorContext, token);
					return (result != false, false, null);
				}
				catch (OperationCanceledException tce) when (token.IsCancellationRequested)
				{
					return (false, true, tce);
				}
				catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
				{
					return (false, true, ae.GetCancellationException());
				}
				catch (Exception cex)
				{
					return (false, false, cex);
				}
			}
		}

		/// <summary>
		/// Evaluates a policy rule asynchronously to determine if an exception should be handled.
		/// </summary>
		/// <typeparam name="T">The type of the error context.</typeparam>
		/// <param name="ex">The exception to evaluate.</param>
		/// <param name="policyResult">The policy result to update based on the rule evaluation.</param>
		/// <param name="errorContext">The error context containing additional information about the exception.</param>
		/// <param name="policyRuleFunc">The policy rule function to evaluate.</param>
		/// <param name="configureAwait">Whether to configure await for the async operations.</param>
		/// <param name="token">The cancellation token.</param>
		/// <returns>A task representing the exception handling result based on the policy rule evaluation.</returns>
		private static async Task<ExceptionHandlingResult> EvaluatePolicyRuleAsync<T>(Exception ex, PolicyResult policyResult, ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, Task<bool>> policyRuleFunc, bool configureAwait, CancellationToken token)
		{
			return HandlePolicyRuleFuncResult(await RunPolicyRuleFunc().ConfigureAwait(configureAwait), ex, policyResult);

			async Task<(bool Result, bool IsCanceled, Exception error)> RunPolicyRuleFunc()
			{
				try
				{
					if (policyRuleFunc is null)
						return (true, false, null);
					var result = await policyRuleFunc(errorContext, token).ConfigureAwait(configureAwait);
					return (result, false, null);
				}
				catch (OperationCanceledException tce) when (token.IsCancellationRequested)
				{
					return (false, true, tce);
				}
				catch (AggregateException ae) when (ae.IsOperationCanceledWithRequestedToken(token))
				{
					return (false, true, ae.GetCancellationException());
				}
				catch (Exception cex)
				{
					return (false, false, cex);
				}
			}
		}

		/// <summary>
		/// Handles the result of a policy rule function evaluation and updates the policy result accordingly.
		/// </summary>
		/// <param name="result">The result from the policy rule function evaluation.</param>
		/// <param name="ex">The original exception that was evaluated.</param>
		/// <param name="policyResult">The policy result to update based on the rule evaluation.</param>
		/// <returns>The exception handling result based on the policy rule function evaluation.</returns>
		private static ExceptionHandlingResult HandlePolicyRuleFuncResult((bool accepted, bool canceled, Exception error) result, Exception ex, PolicyResult policyResult)
		{
			if (result.accepted)
			{
				return ExceptionHandlingResult.Accepted;
			}
			else
			{
				if (!(result.error is null))
				{
					if (result.canceled)
					{
						policyResult.SetFailedAndCanceled((OperationCanceledException)result.error);
						policyResult.AddCatchBlockError(new CatchBlockException(result.error, ex, CatchBlockExceptionSource.PolicyRule));
					}
					else
					{
						policyResult.SetFailedWithCatchBlockError(result.error, ex, CatchBlockExceptionSource.PolicyRule);
					}
				}
				else
				{
					policyResult.SetFailedInner();
				}

				return ExceptionHandlingResult.Handled;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection of error processors.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<IErrorProcessor> GetEnumerator() => _bulkErrorProcessor.GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through the collection of error processors.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Represents the outcome of an exception handling.
	/// </summary>
	public enum ExceptionHandlingResult
	{
		/// <summary>
		/// The exception matched the configured filter and rule and should be processed.
		/// </summary>
		Accepted,

		/// <summary>
		/// The exception was handled and will not be rethrown.
		/// </summary>
		Handled,

		/// <summary>
		/// The exception was not handled and should be rethrown to the caller.
		/// </summary>
		Rethrow
	}

	/// <summary>
	/// Defines the desired behavior for an exception handling mechanism.
	/// </summary>
	public enum ExceptionHandlingBehavior
	{
		/// <summary>
		/// Handle the exception and do not rethrow it.
		/// <para>Outcome: <see cref="ExceptionHandlingResult.Handled"/>
		/// or <see cref="ExceptionHandlingResult.Accepted"/></para>
		/// </summary>
		Handle,

		/// <summary>
		/// Handle the exception and rethrow ONLY if an error filter condition is NOT satisfied.
		/// <para>Outcome: <see cref="ExceptionHandlingResult.Handled"/> (if filtered)
		/// or <see cref="ExceptionHandlingResult.Rethrow"/> (if unfiltered)</para>
		/// </summary>
		ConditionalRethrow
	}

	/// <summary>
	/// Describes whether a cancellation that occurred during <see cref="BulkErrorProcessor"/> should be propagated to <see cref="PolicyResult.IsCanceled"/>.
	/// </summary>
	public enum ErrorProcessingCancellationEffect
	{
		/// <summary>
		/// Cancellation during error processing does not influence
		/// the policy execution result.
		/// </summary>
		/// <remarks>
		/// When this value is specified, cancellations occurring inside
		/// error processors are treated as an internal execution concern
		/// and <see cref="PolicyResult.IsCanceled"/> remains <c>false</c>.
		/// </remarks>
		Ignore,

		/// <summary>
		/// Cancellation during error processing is propagated
		/// to the policy execution result.
		/// </summary>
		/// <remarks>
		/// When this value is specified, a cancellation raised inside
		/// an error processor causes <see cref="PolicyResult.IsCanceled"/>
		/// to be set to <c>true</c>.
		/// </remarks>
		Propagate
	}

	/// <summary>
	/// Defines the execution order for policy rule evaluation and bulk error processing.
	/// </summary>
	public enum ProcessingOrder
	{
		/// <summary>
		/// Evaluates the policy rule before bulk error processing.
		/// </summary>
		EvaluateThenProcess,

		/// <summary>
		/// Performs bulk error processing before evaluating the policy rule.
		/// </summary>
		ProcessThenEvaluate
	}
}
