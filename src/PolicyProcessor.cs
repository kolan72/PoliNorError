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

		public void AddErrorProcessor(IErrorProcessor newErrorProcessor)
		{
			_bulkErrorProcessor.AddProcessor(newErrorProcessor);
		}

		public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => ErrorFilter.IncludedErrorFilters;

		public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => ErrorFilter.ExcludedErrorFilters;

		public ExceptionFilter ErrorFilter { get; }

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
				Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc,
				ExceptionHandlingBehavior handlingBehavior,
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
					policyResult.SetFailedAndCanceled();
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
					policyResult.SetFailedAndCanceled();
					return ExceptionHandlingResult.Handled;
				}
			}

			var ruleResult = EvaluatePolicyRule(policyResult, errorContext, policyRuleFunc, token);
			if (ruleResult != ExceptionHandlingResult.Accepted)
			{
				return ruleResult;
			}

			var bulkProcessResult = await _bulkErrorProcessor.ProcessAsync(ex, errorContext.ToProcessingErrorContext(), configureAwait, token).ConfigureAwait(configureAwait);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled();
			}
			return ExceptionHandlingResult.Handled;
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
					policyResult.SetFailedAndCanceled();
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
					policyResult.SetFailedAndCanceled();
					return ExceptionHandlingResult.Handled;
				}
			}

			if (processingOrder == ProcessingOrder.EvaluateThenProcess)
			{
				var ruleResult = EvaluatePolicyRule(policyResult, errorContext, policyRuleFunc, token);
				if (ruleResult != ExceptionHandlingResult.Accepted)
				{
					return ruleResult;
				}

				var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);
				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled();
				}

				return ExceptionHandlingResult.Handled;
			}
			else
			{
				var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
				policyResult.AddBulkProcessorErrors(bulkProcessResult);

				if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
				{
					policyResult.SetFailedAndCanceled();
					return ExceptionHandlingResult.Handled;
				}

				return EvaluatePolicyRule(policyResult, errorContext, policyRuleFunc, token);
			}
		}

		internal static Action<PolicyResult, Exception, ErrorContext<Unit>, CancellationToken> DefaultErrorSaver { get; } = CreateDefaultErrorSaver<Unit>();

		internal static Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> CreateDefaultErrorSaver<T>() =>
			 (pr, e, _, __) => pr.AddError(e);

		internal static Func<PolicyResult, Exception, ErrorContext<Unit>, bool, CancellationToken, Task> DefaultAsyncErrorSaver { get; } = CreateDefaultAsyncErrorSaver<Unit>();

		internal static Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> CreateDefaultAsyncErrorSaver<T>() =>
			 (pr, e, _, __, ___) => { pr.AddError(e); return Task.CompletedTask; };

		internal static Func<ErrorContext<Unit>, CancellationToken, bool> DefaultPolicyRule { get; } = CreateDefaultPolicyRule<Unit>();

		internal static Func<ErrorContext<T>, CancellationToken, bool> CreateDefaultPolicyRule<T>() =>
			(_, __) => true;

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

		private ExceptionHandlingResult EvaluatePolicyRule<T>(PolicyResult policyResult, ErrorContext<T> errorContext, Func<ErrorContext<T>, CancellationToken, bool> policyRuleFunc, CancellationToken token)
		{
			var ruleApplyResult = policyRuleFunc?.Invoke(errorContext, token);
			if (ruleApplyResult == false)
			{
				policyResult.SetFailedInner();
				return ExceptionHandlingResult.Handled;
			}
			return ExceptionHandlingResult.Accepted;
		}

		public IEnumerator<IErrorProcessor> GetEnumerator() => _bulkErrorProcessor.GetEnumerator();
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
