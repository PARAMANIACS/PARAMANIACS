using System;
using System.Drawing;
using Paramaniacs.Core.Colors;
using Rhino.ApplicationSettings;

namespace Paramaniacs.RhinoPlugin.Commands.ColorShuffle
{
    /// <summary>
    /// Applies a random color theme to Rhino and can put the original colors back.
    /// The original colors are remembered the first time a theme is applied.
    /// </summary>
    internal static class RhinoColorSwapper
    {
        private static readonly ThemeGenerator Generator = new ThemeGenerator();
        private static Snapshot _original;

        static RhinoColorSwapper()
        {
            // Safety net: restore even if Rhino closes without the plug-in shutdown event.
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Restore();
        }

        public static bool IsShuffled
        {
            get { return _original != null; }
        }

        public static ColorTheme Shuffle()
        {
            if (_original == null)
                _original = Snapshot.Capture();

            ColorTheme theme = Generator.Next();
            Apply(theme);
            return theme;
        }

        public static bool Restore()
        {
            if (_original == null)
                return false;

            _original.Apply();
            _original = null;
            return true;
        }

        private static void Apply(ColorTheme theme)
        {
            AppearanceSettings.ViewportBackgroundColor = ToColor(theme.Background);
            AppearanceSettings.GridThinLineColor = ToColor(theme.GridMinor);
            AppearanceSettings.GridThickLineColor = ToColor(theme.GridMajor);
            AppearanceSettings.CommandPromptBackgroundColor = ToColor(theme.Background);
            AppearanceSettings.CommandPromptTextColor = ToColor(theme.Text);
            AppearanceSettings.CrosshairColor = ToColor(theme.Accent);
        }

        private static Color ToColor(Rgb rgb)
        {
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        /// <summary>The user's colors before the first shuffle.</summary>
        private sealed class Snapshot
        {
            private Color _viewportBackground;
            private Color _gridThin;
            private Color _gridThick;
            private Color _promptBackground;
            private Color _promptText;
            private Color _crosshair;

            public static Snapshot Capture()
            {
                return new Snapshot
                {
                    _viewportBackground = AppearanceSettings.ViewportBackgroundColor,
                    _gridThin = AppearanceSettings.GridThinLineColor,
                    _gridThick = AppearanceSettings.GridThickLineColor,
                    _promptBackground = AppearanceSettings.CommandPromptBackgroundColor,
                    _promptText = AppearanceSettings.CommandPromptTextColor,
                    _crosshair = AppearanceSettings.CrosshairColor
                };
            }

            public void Apply()
            {
                AppearanceSettings.ViewportBackgroundColor = _viewportBackground;
                AppearanceSettings.GridThinLineColor = _gridThin;
                AppearanceSettings.GridThickLineColor = _gridThick;
                AppearanceSettings.CommandPromptBackgroundColor = _promptBackground;
                AppearanceSettings.CommandPromptTextColor = _promptText;
                AppearanceSettings.CrosshairColor = _crosshair;
            }
        }
    }
}
