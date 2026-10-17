using System;

namespace Paramaniacs.Core.Colors
{
    /// <summary>
    /// Generates random, lively colors (random hue, strong saturation, medium lightness).
    /// Pass a seed for repeatable results.
    /// </summary>
    public sealed class RandomColorGenerator
    {
        private readonly Random _random;

        public RandomColorGenerator() : this(new Random())
        {
        }

        public RandomColorGenerator(int seed) : this(new Random(seed))
        {
        }

        private RandomColorGenerator(Random random)
        {
            _random = random;
        }

        public Rgb Next()
        {
            double hue = _random.NextDouble() * 360.0;
            double saturation = 0.60 + _random.NextDouble() * 0.30;
            double lightness = 0.50 + _random.NextDouble() * 0.15;
            return Rgb.FromHsl(hue, saturation, lightness);
        }
    }
}
