using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Paramaniacs.GrasshopperPlugin
{
    /// <summary>Describes the PARAMANIACS Grasshopper plug-in (shown in Grasshopper's plug-in list).</summary>
    public sealed class ParamaniacsInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get { return "PARAMANIACS"; }
        }

        public override Bitmap Icon
        {
            get { return null; }
        }

        public override string Description
        {
            get { return "Fun, quirky and playful components for Grasshopper."; }
        }

        public override Guid Id
        {
            get { return new Guid("192A7659-E7B2-4339-8916-8569390F7DB2"); }
        }

        public override string AuthorName
        {
            get { return "PARAMANIACS contributors"; }
        }

        public override string AuthorContact
        {
            get { return "https://github.com/paramaniacs/paramaniacs"; }
        }
    }
}
