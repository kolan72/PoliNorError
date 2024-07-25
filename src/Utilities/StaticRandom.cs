using System;
using System.Threading;

namespace PoliNorError
{
    internal static class StaticRandom
    {
        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random());

        public static double RandDouble()
        {
            return random.Value.NextDouble();
        }
    }
}
