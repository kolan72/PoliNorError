using System;

namespace PoliNorError
{
	public static class PolicyProcessorErrorExtensions
	{
		/// <summary>
		/// Add a <see cref="NonEmptyCatchBlockFilter"/> to the policy processor’s error filters.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor.</typeparam>
		/// <param name="policyProcessor">Policy processor.</param>
		/// <param name="filterFactory">Factory to create the <see cref="NonEmptyCatchBlockFilter"/>.</param>
		/// <returns></returns>
		public static T AddErrorFilter<T>(this T policyProcessor, Func<IEmptyCatchBlockFilter, NonEmptyCatchBlockFilter> filterFactory) where T : IPolicyProcessor
		{
			policyProcessor.AddNonEmptyCatchBlockFilter(filterFactory);
			return policyProcessor;
		}

		/// <summary>
		/// Add a <see cref="NonEmptyCatchBlockFilter"/> to the policy processor’s error filters.
		/// </summary>
		/// <typeparam name="T">The type of the policy processor.</typeparam>
		/// <param name="policyProcessor">Policy processor.</param>
		/// <param name="filter"><see cref="NonEmptyCatchBlockFilter"/></param>
		/// <returns></returns>
		public static T AddErrorFilter<T>(this T policyProcessor, NonEmptyCatchBlockFilter filter) where T : IPolicyProcessor
		{
			policyProcessor.AddNonEmptyCatchBlockFilter(filter);
			return policyProcessor;
		}
	}
}
