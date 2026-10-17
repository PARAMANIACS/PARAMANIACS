using System;

namespace Paramaniacs.Core.Colors
{
    /// <summary>
    /// A tiny RGB color. Core has no dependency on System.Drawing or Rhino,
    /// so each host (Rhino / Grasshopper) converts it to its own color type.
    /// </summary>
    public readonly struct Rgb
    {
        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        /// <summary>Hue 0-360, saturation 0-1, lightness 0-1.</summary>
        public static Rgb FromHsl(double hue, double saturation, double lightness)
        {
            hue = ((hue % 360.0) + 360.0) % 360.0;
            saturation = Clamp01(saturation);
            lightness = Clamp01(lightness);

            double c = (1.0 - Math.Abs(2.0 * lightness - 1.0)) * saturation;
            double x = c * (1.0 - Math.Abs((hue / 60.0) % 2.0 - 1.0));
            double m = lightness - c / 2.0;

            double r, g, b;
            if (hue < 60.0) { r = c; g = x; b = 0; }
            else if (hue < 120.0) { r = x; g = c; b = 0; }
            else if (hue < 180.0) { r = 0; g = c; b = x; }
            else if (hue < 240.0) { r = 0; g = x; b = c; }
            else if (hue < 300.0) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return new Rgb(ToByte(r + m), ToByte(g + m), ToByte(b + m));
        }

        public string ToHex()
        {
            return "#" + R.ToString("X2") + G.ToString("X2") + B.ToString("X2");
        }

        private static double Clamp01(double value)
        {
            return value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value);
        }

        private static byte ToByte(double channel)
        {
            return (byte)Math.Round(Clamp01(channel) * 255.0);
        }
    }
}
