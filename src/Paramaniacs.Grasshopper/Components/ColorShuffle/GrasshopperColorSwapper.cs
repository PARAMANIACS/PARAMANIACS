using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Paramaniacs.Core.Colors;

namespace Paramaniacs.GrasshopperPlugin.Components.ColorShuffle
{
    /// <summary>
    /// Applies a random color theme to the Grasshopper canvas and can put the
    /// original colors back. Changes live in memory only (nothing is saved to disk).
    /// </summary>
    internal static class GrasshopperColorSwapper
    {
        private static readonly ThemeGenerator Generator = new ThemeGenerator();
        private static Snapshot _original;

        static GrasshopperColorSwapper()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => Restore();
        }

        public static ColorTheme Shuffle()
        {
            if (_original == null)
                _original = Snapshot.Capture();

            ColorTheme theme = Generator.Next();
            GH_Skin.canvas_back = ToColor(theme.Background);
            GH_Skin.canvas_mono_color = ToColor(theme.Background);
            GH_Skin.canvas_grid = ToColor(theme.GridMajor);
            GH_Skin.canvas_edge = ToColor(theme.Accent);
            GH_Skin.canvas_shade = ToColor(theme.GridMinor);

            Redraw();
            return theme;
        }

        public static bool Restore()
        {
            if (_original == null)
                return false;

            _original.Apply();
            _original = null;
            Redraw();
            return true;
        }

        private static void Redraw()
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            if (canvas != null)
                canvas.Refresh();
        }

        private static Color ToColor(Rgb rgb)
        {
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        /// <summary>The user's canvas colors before the first shuffle.</summary>
        private sealed class Snapshot
        {
            private Color _back;
            private Color _monoColor;
            private Color _grid;
            private Color _edge;
            private Color _shade;

            public static Snapshot Capture()
            {
                return new Snapshot
                {
                    _back = GH_Skin.canvas_back,
                    _monoColor = GH_Skin.canvas_mono_color,
                    _grid = GH_Skin.canvas_grid,
                    _edge = GH_Skin.canvas_edge,
                    _shade = GH_Skin.canvas_shade
                };
            }

            public void Apply()
            {
                GH_Skin.canvas_back = _back;
                GH_Skin.canvas_mono_color = _monoColor;
                GH_Skin.canvas_grid = _grid;
                GH_Skin.canvas_edge = _edge;
                GH_Skin.canvas_shade = _shade;
            }
        }
    }
}
