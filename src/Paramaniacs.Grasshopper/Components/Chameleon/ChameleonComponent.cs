using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Paramaniacs.Core.Colors;

namespace Paramaniacs.GrasshopperPlugin.Components.Chameleon
{
    /// <summary>
    /// Chameleon: gives every component, parameter, slider and panel on the canvas
    /// its own random color. Press Run again for a whole new set of colors.
    ///
    /// The colors are only painted on top of the canvas while the component is active.
    /// Nothing in the document is changed or saved. Connect true to Restore,
    /// or delete the component, to get the normal colors back.
    /// </summary>
    public sealed class ChameleonComponent : ParamaniacsComponent
    {
        private readonly RandomColorGenerator _generator = new RandomColorGenerator();
        private readonly Dictionary<Guid, Color> _colors = new Dictionary<Guid, Color>();

        private GH_Canvas _canvas;
        private GH_Document _document;
        private bool _active;
        private bool _lastRun;
        private int _alpha = 115;

        public ChameleonComponent()
            : base(
                "Chameleon",
                "Cham",
                "Gives every component on the canvas its own random color. Connect a Button to Run. Connect true to Restore (or delete this component) to get the normal colors back. Nothing in your definition is changed.",
                SubCategories.Colors)
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("96E6EC4F-3429-4C8B-9F63-8DF954FB8D10"); }
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "R", "Press to give every component a new random color.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Restore", "X", "Set to true to bring back the normal colors.", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("Opacity", "O", "How strong the colors are, from 0.05 to 1.", GH_ParamAccess.item, 0.45);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Count", "C", "How many objects are painted right now.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess da)
        {
            bool run = false;
            bool restore = false;
            double opacity = 0.45;
            da.GetData(0, ref run);
            da.GetData(1, ref restore);
            da.GetData(2, ref opacity);

            _alpha = (int)Math.Round(Math.Max(0.05, Math.Min(1.0, opacity)) * 255.0);

            if (restore)
            {
                Deactivate();
            }
            else if (run && !_lastRun)
            {
                Activate();
            }

            _lastRun = run;

            if (_active && _canvas != null)
                _canvas.Invalidate();

            da.SetData(0, _active ? _colors.Count : 0);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            // Deleting the component undoes the prank.
            Deactivate();
            base.RemovedFromDocument(document);
        }

        private void Activate()
        {
            GH_Document document = OnPingDocument();
            GH_Canvas canvas = Instances.ActiveCanvas;
            if (document == null || canvas == null)
                return;

            if (_canvas != null && !ReferenceEquals(_canvas, canvas))
                _canvas.CanvasPostPaintObjects -= OnPostPaintObjects;

            _document = document;
            if (!_active || !ReferenceEquals(_canvas, canvas))
            {
                _canvas = canvas;
                _canvas.CanvasPostPaintObjects += OnPostPaintObjects;
            }

            _colors.Clear();
            foreach (IGH_DocumentObject obj in document.Objects)
            {
                if (IsTintable(obj))
                    _colors[obj.InstanceGuid] = ToColor(_generator.Next());
            }

            _active = true;
        }

        private void Deactivate()
        {
            if (_canvas != null)
            {
                _canvas.CanvasPostPaintObjects -= OnPostPaintObjects;
                _canvas.Invalidate();
            }

            _canvas = null;
            _document = null;
            _active = false;
            _colors.Clear();
        }

        /// <summary>Components, parameters, sliders and panels. Groups and scribbles are left alone.</summary>
        private static bool IsTintable(IGH_DocumentObject obj)
        {
            return (obj is IGH_Component || obj is IGH_Param) && obj.Attributes != null;
        }

        private void OnPostPaintObjects(GH_Canvas sender)
        {
            GH_Document document = sender.Document;
            if (!_active || document == null || !ReferenceEquals(document, _document))
                return;

            Graphics graphics = sender.Graphics;
            if (graphics == null)
                return;

            foreach (IGH_DocumentObject obj in document.Objects)
            {
                if (!IsTintable(obj))
                    continue;

                Color color;
                if (!_colors.TryGetValue(obj.InstanceGuid, out color))
                {
                    // Objects added after pressing Run get a color too.
                    color = ToColor(_generator.Next());
                    _colors[obj.InstanceGuid] = color;
                }

                RectangleF bounds = obj.Attributes.Bounds;
                using (var brush = new SolidBrush(Color.FromArgb(_alpha, color)))
                using (GraphicsPath path = RoundedRectangle(bounds, 4f))
                {
                    graphics.FillPath(brush, path);
                }
            }
        }

        private static Color ToColor(Rgb rgb)
        {
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        private static GraphicsPath RoundedRectangle(RectangleF rect, float radius)
        {
            float d = Math.Min(radius * 2f, Math.Min(rect.Width, rect.Height));
            var path = new GraphicsPath();
            if (d <= 0f)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(rect.Left, rect.Top, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Top, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.Left, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
