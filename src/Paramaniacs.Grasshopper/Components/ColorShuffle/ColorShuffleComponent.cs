using System;
using Grasshopper.Kernel;
using Paramaniacs.Core.Colors;

namespace Paramaniacs.GrasshopperPlugin.Components.ColorShuffle
{
    /// <summary>
    /// Color Shuffle: gives the Grasshopper canvas a random new color theme
    /// every time Run switches from false to true (connect a Button).
    /// Connect true to Restore, or delete the component, to get your colors back.
    /// </summary>
    public sealed class ColorShuffleComponent : ParamaniacsComponent
    {
        private bool _lastRun;
        private string _lastMessage = "Waiting for a button press.";

        public ColorShuffleComponent()
            : base(
                "Color Shuffle",
                "Shuffle",
                "Gives the Grasshopper canvas a random new color theme. Connect a Button to Run. Connect true to Restore (or delete this component) to get your original colors back.",
                SubCategories.Colors)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ECA64728-D1B5-4C15-81C4-23CDEFDB19D4"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Press to shuffle the canvas colors.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Restore", "X", "Set to true to restore the original canvas colors.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Theme", "T", "The name of the current theme.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            bool run = false;
            bool restore = false;
            da.GetData(0, ref run);
            da.GetData(1, ref restore);

            if (restore)
            {
                _lastMessage = GrasshopperColorSwapper.Restore()
                    ? "Original colors restored."
                    : "Nothing to restore, your colors were never changed.";
            }
            else if (run && !_lastRun)
            {
                ColorTheme theme = GrasshopperColorSwapper.Shuffle();
                _lastMessage = theme.Name + " (background " + theme.Background.ToHex() + ")";
            }

            _lastRun = run;
            da.SetData(0, _lastMessage);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            // Deleting the component undoes the prank.
            GrasshopperColorSwapper.Restore();
            base.RemovedFromDocument(document);
        }
    }
}
