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

		protected internal ExceptionHandlingResult HandleException<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> saveError,
			Func<ErrorContext<T>, bool> policyRuleFunc,
			ExceptionHandlingBehavior handlingBehavior,
			ErrorProcessingCancellationEffect cancellationEffect,
			CancellationToken token)
		{
			saveError = saveError ?? CreateDefaultErrorSaver<T>();
			if (handlingBehavior != ExceptionHandlingBehavior.ConditionalRethrow)
			{
				saveError(policyResult, ex, errorContext, token);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled();
					return ExceptionHandlingResult.Handled;
				}
			}

			var result = DetermineExceptionHandlingResult(ex, policyResult, errorContext, saveError, policyRuleFunc, handlingBehavior, token);
			if (result != ExceptionHandlingResult.Accepted)
			{
				return result;
			}

			if (handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
			{
				saveError(policyResult, ex, errorContext, token);
				if (token.IsCancellationRequested)
				{
					policyResult.SetFailedAndCanceled();
					return ExceptionHandlingResult.Handled;
				}
			}

			var bulkProcessResult = _bulkErrorProcessor.Process(ex, errorContext.ToProcessingErrorContext(), token);
			policyResult.AddBulkProcessorErrors(bulkProcessResult);
			if (cancellationEffect == ErrorProcessingCancellationEffect.Propagate && bulkProcessResult.IsCanceled)
			{
				policyResult.SetFailedAndCanceled();
			}
			return ExceptionHandlingResult.Handled;
		}

		internal static Action<PolicyResult, Exception, ErrorContext<Unit>, CancellationToken> DefaultErrorSaver { get; } = CreateDefaultErrorSaver<Unit>();

		internal static Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> CreateDefaultErrorSaver<T>() =>
			 (pr, e, _, __) => pr.AddError(e);

		internal static Func<PolicyResult, Exception, ErrorContext<T>, bool, CancellationToken, Task> CreateDefaultAsyncErrorSaver<T>() =>
			 (pr, e, _, __, ___) => { pr.AddError(e); return Task.CompletedTask; };

		internal static Func<ErrorContext<Unit>, bool> DefaultPolicyRule { get; } = CreateDefaultPolicyRule<Unit>();

		internal static Func<ErrorContext<T>, bool> CreateDefaultPolicyRule<T>() =>
			(_) => true;

		private ExceptionHandlingResult DetermineExceptionHandlingResult<T>(
			Exception ex,
			PolicyResult policyResult,
			ErrorContext<T> errorContext,
			Action<PolicyResult, Exception, ErrorContext<T>, CancellationToken> saveError,
			Func<ErrorContext<T>, bool> policyRuleFunc,
			ExceptionHandlingBehavior handlingBehavior,
			CancellationToken token)
		{
			var (result, error) = ShouldHandleException(ex, errorContext, policyRuleFunc);
			if (result == HandleCatchBlockResult.FailedByErrorFilter)
			{
				if (!(error is null))
				{
					if (handlingBehavior == ExceptionHandlingBehavior.ConditionalRethrow)
					{
						saveError(policyResult, ex, errorContext, token);
					}
					policyResult.AddCatchBlockError(new CatchBlockException(error, ex, CatchBlockExceptionSource.ErrorFilter, true));
					policyResult.SetFailedAndFilterUnsatisfied();
					return ExceptionHandlingResult.Handled;
				}

				switch (handlingBehavior)
				{
					case ExceptionHandlingBehavior.ConditionalRethrow:
						return ExceptionHandlingResult.Rethrow;
					case ExceptionHandlingBehavior.Handle:
						policyResult.SetFailedAndFilterUnsatisfied();
						return ExceptionHandlingResult.Handled;
					default:
						policyResult.AddCatchBlockError(new CatchBlockException(new ArgumentOutOfRangeException(nameof(handlingBehavior), handlingBehavior, null), ex, CatchBlockExceptionSource.ErrorFilter, true));
						policyResult.SetFailedAndFilterUnsatisfied();
						return ExceptionHandlingResult.Handled;
				}
			}
			if (result == HandleCatchBlockResult.FailedByPolicyRules)
			{
				policyResult.SetFailedInner();
				return ExceptionHandlingResult.Handled;
			}
			return ExceptionHandlingResult.Accepted;
		}

		private (HandleCatchBlockResult, Exception) ShouldHandleException<T>(Exception ex, ErrorContext<T> errorContext, Func<ErrorContext<T>, bool> policyRuleFunc = null)
		{
			var (filterPassed, error) = RunErrorFilterFunc();

			if (!filterPassed)
				return (HandleCatchBlockResult.FailedByErrorFilter, error);

			if (policyRuleFunc?.Invoke(errorContext) == false)
				return (HandleCatchBlockResult.FailedByPolicyRules, null);

			return (HandleCatchBlockResult.Success, null);

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
}
