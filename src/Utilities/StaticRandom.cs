using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PoliNorError
{
    internal static class StaticRandom
    {
        private static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RandDouble()
        {
            return random.Value.NextDouble();
        }
    }
}
