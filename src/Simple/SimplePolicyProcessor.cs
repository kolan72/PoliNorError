using System;
using System.Threading;
using System.Threading.Tasks;

namespace PoliNorError
{
	/// <summary>
	/// A SimplePolicy processor that can handle delegates.
	/// </summary>
	public sealed class SimplePolicyProcessor : PolicyProcessor, ISimplePolicyProcessor
	{
		private readonly EmptyErrorContext _emptyErrorContext;

		private readonly bool _rethrowIfErrorFilterUnsatisfied;

		///<inheritdoc cref = "SimplePolicyProcessor(IBulkErrorProcessor, bool)"/>
		public SimplePolicyProcessor(bool rethrowIfErrorFilterUnsatisfied = false) : this(null, rethrowIfErrorFilterUnsatisfied) { }

		/// <summary>
		/// Initializes a new instance of the SimplePolicyProcessor
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		public SimplePolicyProcessor(IBulkErrorProcessor bulkErrorProcessor, bool rethrowIfErrorFilterUnsatisfied = false) : base(PolicyAlias.Simple, bulkErrorProcessor)
		{
			_emptyErrorContext = _isPolicyAliasSet ? EmptyErrorContext.Default: EmptyErrorContext.DefaultSimple;
			_rethrowIfErrorFilterUnsatisfied = rethrowIfErrorFilterUnsatisfied;
		}

		///<inheritdoc cref = "CreateDefault(IBulkErrorProcessor, bool)"/>
		public static ISimplePolicyProcessor CreateDefault(bool rethrowIfErrorFilterUnsatisfied = false) => new SimplePolicyProcessor(rethrowIfErrorFilterUnsatisfied);

		/// <summary>
		/// Creates and returns the <see cref="SimplePolicyProcessor"/> class, which represents the default implementation of the <see cref="ISimplePolicyProcessor"/> interface.
		/// </summary>
		/// <param name="bulkErrorProcessor"><see cref="IBulkErrorProcessor"/></param>
		/// <param name="rethrowIfErrorFilterUnsatisfied">Specifies whether an exception is rethrown if the error filter is unsatisfied.</param>
		/// <returns></returns>
		public static ISimplePolicyProcessor CreateDefault(IBulkErrorProcessor bulkErrorProcessor, bool rethrowIfErrorFilterUnsatisfied = false) => new SimplePolicyProcessor(bulkErrorProcessor, rethrowIfErrorFilterUnsatisfied);

		///<inheritdoc cref = "ISimplePolicyProcessor.Execute"/>
		public PolicyResult Execute(Action action, CancellationToken token = default)
		{
			if (action == null)
				return new PolicyResult().WithNoDelegateException();

			PolicyResult result = PolicyResult.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				action();
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					(bool? filterUnsatisfied, Exception filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													 .Handle(ex, _emptyErrorContext));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.Execute{T}"/>
		public PolicyResult<T> Execute<T>(Func<T> func, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.ForSync();

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = func();
				result.SetOk();
				result.SetResult(resAction);
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (AggregateException ae) when (ae.HasCanceledException(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					(bool? filterUnsatisfied, Exception filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(GetCatchBlockSyncHandler<Unit>(result, token)
													 .Handle(ex, _emptyErrorContext));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.ExecuteAsync"/>
		public async Task<PolicyResult> ExecuteAsync(Func<CancellationToken, Task> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult().WithNoDelegateException();

			PolicyResult result = PolicyResult.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				await func(token).ConfigureAwait(configureAwait);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					(bool? filterUnsatisfied, Exception filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
															.HandleAsync(ex, _emptyErrorContext).ConfigureAwait(configureAwait));
			}
			return result;
		}

		///<inheritdoc cref = "ISimplePolicyProcessor.ExecuteAsync{T}"/>
		public async Task<PolicyResult<T>> ExecuteAsync<T>(Func<CancellationToken, Task<T>> func, bool configureAwait = false, CancellationToken token = default)
		{
			if (func == null)
				return new PolicyResult<T>().WithNoDelegateException();

			var result = PolicyResult<T>.InitByConfigureAwait(configureAwait);

			if (token.IsCancellationRequested)
			{
				result.SetCanceled();
				return result;
			}

			try
			{
				var resAction = await func(token).ConfigureAwait(configureAwait);
				result.SetResult(resAction);
				result.SetOk();
			}
			catch (OperationCanceledException oe) when (oe.CancellationToken.Equals(token))
			{
				result.SetFailedAndCanceled();
			}
			catch (Exception ex)
			{
				if (_rethrowIfErrorFilterUnsatisfied)
				{
					(bool? filterUnsatisfied, Exception filterException) = GetFilterUnsatisfiedOrFilterException(ex);
					if (filterUnsatisfied == true)
					{
						throw;
					}
					else if (!(filterException is null))
					{
						AddErrorAndCatchBlockFilterError(result, ex, filterException);
						return result;
					}
				}

				result.AddError(ex);
				result.ChangeByHandleCatchBlockResult(await GetCatchBlockAsyncHandler<Unit>(result, configureAwait, token)
															.HandleAsync(ex, _emptyErrorContext).ConfigureAwait(configureAwait));
			}
			return result;
		}

		private (bool? FilterUnsatisfied, Exception exception) GetFilterUnsatisfiedOrFilterException(Exception ex)
		{
			try
			{
				return (!ErrorFilter.GetCanHandle()(ex), null);
			}
			catch (Exception filterEx)
			{
				return (null, filterEx);
			}
		}

		private static void AddErrorAndCatchBlockFilterError(PolicyResult result, Exception ex, Exception filterException)
		{
			result.AddError(ex);
			result.SetFailedWithCatchBlockError(filterException, ex, CatchBlockExceptionSource.ErrorFilter);
		}
	}
}
