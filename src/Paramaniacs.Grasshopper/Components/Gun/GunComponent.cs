using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Paramaniacs.GrasshopperPlugin.Components.Gun
{
    /// <summary>
    /// Gun: every time Fire switches from false to true (connect a Button), an arrow
    /// flies out of the component's only output and deletes the first component it hits.
    /// Switch on Pierce to let the arrow fly through and delete everything in its path.
    /// Deleted components come back with Ctrl+Z.
    /// </summary>
    public sealed class GunComponent : ParamaniacsComponent
    {
        private ArrowShooter _shooter;
        private bool _lastFire;
        private int _shots;
        public GunComponent()
            : base(
                "Gun",
                "Gun",
                "Shoots an arrow out of its output when Fire is pressed (connect a Button). Any component the arrow hits is deleted. Ctrl+Z brings it back. Angle: 0 = right, 90 = up, 180 = left, 270 = down. Pierce: the arrow flies through everything.",
                SubCategories.Surprises)
        {
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("09B6EA38-43FA-4D20-B4A0-E2AE5B1B1CCB"); }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Fire", "F", "Press to shoot an arrow.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Angle", "A", "Shooting direction in degrees. 0 = right, 90 = up, 180 = left, 270 = down.", GH_ParamAccess.item, 0.0);
            pManager.AddBooleanParameter("Pierce", "P", "If true, the arrow flies through everything and deletes every component in its path.", GH_ParamAccess.item, false);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Shots", "S", "How many arrows were fired so far.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess da)
        {
            bool fire = false;
            double angle = 0.0;
            bool pierce = false;
            da.GetData(0, ref fire);
            da.GetData(1, ref angle);
            da.GetData(2, ref pierce);
            GH_Document document = OnPingDocument();
            if (fire && !_lastFire && document != null)
            {
                if (_shooter == null)
                    _shooter = new ArrowShooter(InstanceGuid);
                // The arrow leaves from the output pin of this component.
                PointF start = Params.Output[0].Attributes.OutputGrip;
                _shooter.Fire(document, start, angle, pierce);
                _shots++;
            }
            _lastFire = fire;
            da.SetData(0, _shots);
        }
        public override void RemovedFromDocument(GH_Document document)
        {
            if (_shooter != null)
            {
                _shooter.Dispose();
                _shooter = null;
            }
            base.RemovedFromDocument(document);
        }
    }
}