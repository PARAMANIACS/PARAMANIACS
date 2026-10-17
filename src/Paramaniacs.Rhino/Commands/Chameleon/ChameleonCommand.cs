using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;

namespace Paramaniacs.RhinoPlugin.Commands.Chameleon
{
    /// <summary>
    /// ParamaniacsChameleon: gives every object in the model its own random color.
    /// Press Enter (or choose Shuffle) for new colors, choose Restore to go back.
    /// </summary>
    public sealed class ChameleonCommand : Command
    {
        public ChameleonCommand()
        {
            Instance = this;
        }

        public static ChameleonCommand Instance { get; private set; }

        public override string EnglishName
        {
            get { return "ParamaniacsChameleon"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var go = new GetOption();
            go.SetCommandPrompt("Press Enter to turn every object into a chameleon");
            go.AcceptNothing(true);
            go.AddOption("Shuffle");
            int restoreIndex = go.AddOption("Restore");

            GetResult result = go.Get();
            if (result == GetResult.Cancel)
                return Result.Cancel;

            bool restore = result == GetResult.Option && go.OptionIndex() == restoreIndex;

            if (restore)
            {
                if (RhinoChameleonPainter.IsActive(doc))
                {
                    int count = RhinoChameleonPainter.Restore(doc);
                    RhinoApp.WriteLine("PARAMANIACS: {0} object(s) have their original colors back.", count);
                }
                else
                {
                    RhinoApp.WriteLine("PARAMANIACS: nothing to restore, no object colors were changed.");
                }
            }
            else
            {
                int count = RhinoChameleonPainter.Shuffle(doc);
                if (count == 0)
                    RhinoApp.WriteLine("PARAMANIACS: there are no objects to color.");
                else
                    RhinoApp.WriteLine("PARAMANIACS: {0} object(s) painted. Run the command again for new colors, or choose Restore.", count);
            }

            return Result.Success;
        }
    }
}
