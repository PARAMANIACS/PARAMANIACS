using System.Drawing;
using Rhino.Display;
using Rhino.Geometry;

namespace Paramaniacs.RhinoPlugin.Commands.Gun
{
    /// <summary>Draws a flying arrow in the Rhino viewports. Nothing is added to the document.</summary>
    internal sealed class ArrowConduit : DisplayConduit
    {
        private static readonly Color ArrowColor = Color.OrangeRed;
        public Line Arrow { get; set; }
        public bool Visible { get; set; }
        protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
        {
            if (Visible && Arrow.IsValid)
                e.IncludeBoundingBox(Arrow.BoundingBox);

            base.CalculateBoundingBox(e);
        }
        protected override void PostDrawObjects(DrawEventArgs e)
        {
            if (!Visible || !Arrow.IsValid)
                return;
            e.Display.DrawLine(Arrow, ArrowColor, 3);
            e.Display.DrawArrowHead(Arrow.To, Arrow.Direction, ArrowColor, 18.0, 0.0);
        }
    }
}