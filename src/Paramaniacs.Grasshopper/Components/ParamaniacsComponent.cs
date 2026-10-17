using Grasshopper.Kernel;

namespace Paramaniacs.GrasshopperPlugin.Components
{
    /// <summary>
    /// Base class for every PARAMANIACS component.
    /// It puts the component in the "PARAMANIACS" tab and gives all components
    /// the same defaults. Inherit from this class instead of GH_Component.
    /// </summary>
    public abstract class ParamaniacsComponent : GH_Component
    {
        /// <summary>The name of the Grasshopper tab that holds all our components.</summary>
        public const string TabName = "PARAMANIACS";

        protected ParamaniacsComponent(string name, string nickname, string description, string subCategory)
            : base(name, nickname, description, TabName, subCategory)
        {
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>Shows a friendly (non-error) note on the component bubble.</summary>
        protected void Remark(string message)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, message);
        }
    }
}
