using Paramaniacs.Core.Colors;
using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Paramaniacs.RhinoPlugin.Commands.ColorShuffle
{
    /// <summary>
    /// ParamaniacsColors: gives Rhino a random new color theme.
    /// Press Enter (or choose Shuffle) for a new theme, choose Restore to go back.
    /// </summary>
    public sealed class ColorShuffleCommand : Command
    {
        public ColorShuffleCommand()
        {
            Instance = this;
        }

        public static ColorShuffleCommand Instance { get; private set; }

        public override string EnglishName
        {
            get { return "ParamaniacsColors"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var go = new GetOption();
            go.SetCommandPrompt("Press Enter for a surprise");
            go.AcceptNothing(true);
            int shuffleIndex = go.AddOption("Shuffle");
            int restoreIndex = go.AddOption("Restore");

            GetResult result = go.Get();
            if (result == GetResult.Cancel)
                return Result.Cancel;

            bool restore = result == GetResult.Option && go.OptionIndex() == restoreIndex;

            if (restore)
            {
                if (RhinoColorSwapper.Restore())
                    RhinoApp.WriteLine("PARAMANIACS: your original colors are back.");
                else
                    RhinoApp.WriteLine("PARAMANIACS: nothing to restore, your colors were never changed.");
            }
            else
            {
                ColorTheme theme = RhinoColorSwapper.Shuffle();
                RhinoApp.WriteLine(
                    "PARAMANIACS: new theme \"{0}\" (background {1}). Run the command again for another one, or choose Restore.",
                    theme.Name,
                    theme.Background.ToHex());
            }

            doc.Views.Redraw();
            return Result.Success;
        }
    }
}
