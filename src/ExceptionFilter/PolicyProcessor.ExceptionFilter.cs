using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static PoliNorError.CatchBlockFilter;

namespace PoliNorError
{
	public abstract partial class PolicyProcessor
	{
		/// <summary>
		/// Represents a filter for handling exceptions based on inclusion and exclusion criteria.
		/// </summary>
		public class ExceptionFilter
		{
			/// <summary>
			/// Gets the collection of included error filters used to determine which exceptions should be handled.
			/// </summary>
			/// <value>An enumerable collection of expressions that evaluate to <see langword="true"/> if the exception should be handled.</value>
			public IEnumerable<Expression<Func<Exception, bool>>> IncludedErrorFilters => FilterSet.IncludedErrorFilters;

			/// <summary>
			/// Gets the collection of excluded error filters used to determine which exceptions should not be handled.
			/// </summary>
			/// <value>An enumerable collection of expressions that evaluate to <see langword="true"/> if the exception should be excluded from handling.</value>
			public IEnumerable<Expression<Func<Exception, bool>>> ExcludedErrorFilters => FilterSet.ExcludedErrorFilters;

			/// <summary>
			/// Adds an error set to be included in the filter.
			/// </summary>
			/// <param name="errorSet">The error set containing exception types to include.</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter IncludeErrorSet(IErrorSet errorSet)
			{
				this.AddIncludedErrorSet(errorSet);
				return this;
			}

			/// <summary>
			/// Adds an error set to be excluded from the filter.
			/// </summary>
			/// <param name="errorSet">The error set containing exception types to exclude.</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter ExcludeErrorSet(IErrorSet errorSet)
			{
				this.AddExcludedErrorSet(errorSet);
				return this;
			}

			/// <summary>
			/// Includes a specific exception type in the filter.
			/// </summary>
			/// <typeparam name="TException">The type of exception to include.</typeparam>
			/// <param name="errorType">The type of error to filter (error or inner error).</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter IncludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				return IncludeError<TException>(null, errorType);
			}

			/// <summary>
			/// Includes a specific exception type in the filter with a custom predicate.
			/// </summary>
			/// <typeparam name="TException">The type of exception to include.</typeparam>
			/// <param name="func">A predicate function to further filter the exception. Can be <see langword="null"/> to include all exceptions of type <typeparamref name="TException"/>.</param>
			/// <param name="errorType">The type of error to filter (error or inner error).</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter IncludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				switch (errorType)
				{
					case ErrorType.Error:
						AddIncludedErrorFilter(func);
						return this;
					case ErrorType.InnerError:
						AddIncludedInnerErrorFilter(func);
						return this;
					default:
						throw new NotImplementedException();
				}
			}

			/// <summary>
			/// Includes an exception using a lambda expression filter.
			/// </summary>
			/// <param name="expression">An expression that evaluates to <see langword="true"/> if the exception should be handled.</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter IncludeError(Expression<Func<Exception, bool>> expression)
			{
				AddIncludedErrorFilter(expression);
				return this;
			}

			/// <summary>
			/// Excludes a specific exception type from the filter.
			/// </summary>
			/// <typeparam name="TException">The type of exception to exclude.</typeparam>
			/// <param name="errorType">The type of error to filter (error or inner error).</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter ExcludeError<TException>(ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				return ExcludeError<TException>(null, errorType);
			}

			/// <summary>
			/// Excludes a specific exception type from the filter with a custom predicate.
			/// </summary>
			/// <typeparam name="TException">The type of exception to exclude.</typeparam>
			/// <param name="func">A predicate function to further filter the exception. Can be <see langword="null"/> to exclude all exceptions of type <typeparamref name="TException"/>.</param>
			/// <param name="errorType">The type of error to filter (error or inner error).</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter ExcludeError<TException>(Func<TException, bool> func, ErrorType errorType = ErrorType.Error) where TException : Exception
			{
				switch (errorType)
				{
					case ErrorType.Error:
						AddExcludedErrorFilter(func);
						return this;
					case ErrorType.InnerError:
						AddExcludedInnerErrorFilter(func);
						return this;
					default:
						throw new NotImplementedException();
				}
			}

			/// <summary>
			/// Excludes an exception using a lambda expression filter.
			/// </summary>
			/// <param name="expression">An expression that evaluates to <see langword="true"/> if the exception should be excluded.</param>
			/// <returns>The current <see cref="ExceptionFilter"/> instance for method chaining.</returns>
			public ExceptionFilter ExcludeError(Expression<Func<Exception, bool>> expression)
			{
				AddExcludedErrorFilter(expression);
				return this;
			}

			/// <summary>
			/// Gets the underlying filter set containing all included and excluded error filters.
			/// </summary>
			internal ExceptionFilterSet FilterSet { get; } = new ExceptionFilterSet();

			/// <summary>
			/// Adds an included error filter expression to the filter set.
			/// </summary>
			/// <param name="handledErrorFilter">The expression to evaluate against exceptions.</param>
			internal void AddIncludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.IncludeFilter(handledErrorFilter);
			}

			/// <summary>
			/// Adds an included error filter with a typed predicate function.
			/// </summary>
			/// <typeparam name="TException">The type of exception to filter.</typeparam>
			/// <param name="func">The predicate function. Can be <see langword="null"/>.</param>
			internal void AddIncludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			/// <summary>
			/// Adds an included inner error filter with a typed predicate function.
			/// </summary>
			/// <typeparam name="TException">The type of inner exception to filter.</typeparam>
			/// <param name="func">The predicate function. Can be <see langword="null"/>.</param>
			internal void AddIncludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddIncludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			/// <summary>
			/// Adds an excluded error filter expression to the filter set.
			/// </summary>
			/// <param name="handledErrorFilter">The expression to evaluate against exceptions.</param>
			internal void AddExcludedErrorFilter(Expression<Func<Exception, bool>> handledErrorFilter)
			{
				FilterSet.ExcludeFilter(handledErrorFilter);
			}

			/// <summary>
			/// Adds an excluded error filter with a typed predicate function.
			/// </summary>
			/// <typeparam name="TException">The type of exception to filter.</typeparam>
			/// <param name="func">The predicate function. Can be <see langword="null"/>.</param>
			internal void AddExcludedErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedErrorFilter(func));
			}

			/// <summary>
			/// Adds an excluded inner error filter with a typed predicate function.
			/// </summary>
			/// <typeparam name="TException">The type of inner exception to filter.</typeparam>
			/// <param name="func">The predicate function. Can be <see langword="null"/>.</param>
			internal void AddExcludedInnerErrorFilter<TException>(Func<TException, bool> func = null) where TException : Exception
			{
				AddExcludedErrorFilter(ExpressionHelper.GetTypedInnerErrorFilter(func));
			}

			/// <summary>
			/// Appends all filters from another <see cref="ExceptionFilter"/> to this filter.
			/// </summary>
			/// <param name="exceptionFilter">The <see cref="ExceptionFilter"/> whose filters should be appended.</param>
			internal void AppendFilter(ExceptionFilter exceptionFilter)
			{
				FilterSet.AppendFilter(exceptionFilter.FilterSet);
			}

			/// <summary>
			/// Compiles all filters into a single predicate function that determines if an exception can be handled.
			/// </summary>
			/// <returns>A function that returns <see langword="true"/> if the exception can be handled by the filter.</returns>
			internal Func<Exception, bool> GetCanHandle()
			{
				return FilterSet.CompilePredicate();
			}

			/// <summary>
			/// Determines whether a filter should propagate an exception when the filter is not satisfied.
			/// </summary>
			/// <param name="predicate">The filter predicate to evaluate.</param>
			/// <param name="originalEx">The original exception to test against the predicate.</param>
			/// <param name="rethrowIfErrorFilterUnsatisfied">If <see langword="true"/>, rethrows the exception when the filter is not satisfied.</param>
			/// <param name="filterAccepted">When the method returns, contains <see langword="true"/> if the filter accepted the exception; otherwise, <see langword="false"/>.</param>
			/// <param name="filterException">When the method returns, contains any exception thrown during filter evaluation; otherwise, <see langword="null"/>.</param>
			/// <returns><see langword="true"/> if the exception should be propagated; otherwise, <see langword="false"/>.</returns>
			internal static bool ShouldPropagateFilterUnsatisfied(Func<Exception, bool> predicate, Exception originalEx, bool rethrowIfErrorFilterUnsatisfied, out bool filterAccepted, out Exception filterException)
			{
				filterAccepted = false;
				filterException = null;
				try
				{
					var filterResult = predicate(originalEx);
					if (!filterResult)
					{
						if (rethrowIfErrorFilterUnsatisfied)
						{
							originalEx.Data[PolinorErrorConsts.EXCEPTION_DATA_ERRORFILTERUNSATISFIED_KEY] = true;
							return true;
						}
						else
						{
							return false;
						}
					}
					else
					{
						filterAccepted = true;
						return false;
					}
				}
				catch (Exception fe)
				{
					filterException = fe;
					return false;
				}
			}
		}
	}
}
