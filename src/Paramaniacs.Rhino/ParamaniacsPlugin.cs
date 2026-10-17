using Paramaniacs.RhinoPlugin.Commands.ColorShuffle;
using Rhino.PlugIns;

namespace Paramaniacs.RhinoPlugin
{
    /// <summary>
    /// The PARAMANIACS plug-in for Rhino. Commands are discovered automatically,
    /// so this class only handles plug-in wide lifetime events.
    /// </summary>
    public sealed class ParamaniacsPlugin : PlugIn
    {
        public ParamaniacsPlugin()
        {
            Instance = this;
        }

        public static ParamaniacsPlugin Instance { get; private set; }

        /// <summary>
        /// Rhino stores color settings on disk. Put the user's original colors back
        /// when Rhino closes so a shuffled theme never becomes permanent.
        /// </summary>
        protected override void OnShutdown()
        {
            RhinoColorSwapper.Restore();
            base.OnShutdown();
        }
    }
}
