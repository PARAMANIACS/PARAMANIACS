using System;

namespace Paramaniacs.Core.Colors
{
    /// <summary>
    /// Generates random but good-looking color themes (random hue, dark or light,
    /// plus a contrasting accent). Pass a seed for repeatable results.
    /// </summary>
    public sealed class ThemeGenerator
    {
        private static readonly string[] Adjectives =
        {
            "Electric", "Sleepy", "Spicy", "Wobbly", "Cosmic", "Moody",
            "Fluffy", "Nocturnal", "Sneaky", "Radiant", "Grumpy", "Quantum"
        };

        private static readonly string[] Nouns =
        {
            "Pickle", "Flamingo", "Concrete", "Espresso", "Mango", "Voxel",
            "Parabola", "Banana", "Staircase", "Hologram", "Cactus", "Lasagna"
        };

        private readonly Random _random;

        public ThemeGenerator() : this(new Random())
        {
        }

        public ThemeGenerator(int seed) : this(new Random(seed))
        {
        }

        private ThemeGenerator(Random random)
        {
            _random = random;
        }

        public ColorTheme Next()
        {
            double hue = _random.NextDouble() * 360.0;
            bool dark = _random.Next(2) == 0;
            double saturation = 0.35 + _random.NextDouble() * 0.45;

            double backgroundLightness = dark
                ? 0.10 + _random.NextDouble() * 0.10
                : 0.86 + _random.NextDouble() * 0.09;
            double step = dark ? 0.08 : -0.08;

            Rgb background = Rgb.FromHsl(hue, saturation * 0.7, backgroundLightness);
            Rgb gridMinor = Rgb.FromHsl(hue, saturation * 0.6, backgroundLightness + step);
            Rgb gridMajor = Rgb.FromHsl(hue, saturation * 0.6, backgroundLightness + step * 2.0);
            Rgb accent = Rgb.FromHsl(hue + 150.0 + _random.NextDouble() * 60.0, 0.85, dark ? 0.62 : 0.42);
            Rgb text = Rgb.FromHsl(hue, 0.25, dark ? 0.92 : 0.12);

            string name = Adjectives[_random.Next(Adjectives.Length)] + " " + Nouns[_random.Next(Nouns.Length)];
            return new ColorTheme(name, dark, background, gridMinor, gridMajor, accent, text);
        }
    }
}
