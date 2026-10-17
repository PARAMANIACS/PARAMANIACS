namespace Paramaniacs.Core.Colors
{
    /// <summary>A small set of matching colors with a silly name.</summary>
    public sealed class ColorTheme
    {
        public ColorTheme(string name, bool isDark, Rgb background, Rgb gridMinor, Rgb gridMajor, Rgb accent, Rgb text)
        {
            Name = name;
            IsDark = isDark;
            Background = background;
            GridMinor = gridMinor;
            GridMajor = gridMajor;
            Accent = accent;
            Text = text;
        }

        public string Name { get; }
        public bool IsDark { get; }
        public Rgb Background { get; }
        public Rgb GridMinor { get; }
        public Rgb GridMajor { get; }
        public Rgb Accent { get; }
        public Rgb Text { get; }
    }
}
