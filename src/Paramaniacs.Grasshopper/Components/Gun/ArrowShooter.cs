using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace Paramaniacs.GrasshopperPlugin.Components.Gun
{
    /// <summary>
    /// Flies arrows across the Grasshopper canvas and deletes the components they hit.
    ///
    /// Every deletion is recorded in Grasshopper's undo stack (Ctrl+Z brings the
    /// component back). The arrows are only painted on top of the canvas.
    /// </summary>
    internal sealed class ArrowShooter : IDisposable
    {
        private const float StepLength = 8f;        // canvas units checked for hits per step
        private const float SpeedPerTick = 40f;     // canvas units flown per timer tick
        private const float MaxDistance = 6000f;    // an arrow gives up after this distance
        private const float ArrowLength = 40f;
        private const int LingerTicks = 30;         // how long a landed arrow stays visible
        private readonly Guid _gunId;
        private readonly List<Arrow> _arrows = new List<Arrow>();
        private readonly System.Windows.Forms.Timer _timer;
        private GH_Canvas _canvas;
        private GH_Document _document;
        public ArrowShooter(Guid gunId)
        {
            _gunId = gunId;
            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += OnTick;
        }
        /// <summary>Shoots one arrow from <paramref name="start"/> (canvas coordinates).</summary>
        /// <param name="angleDegrees">0 = right, 90 = up, 180 = left, 270 = down.</param>
        public void Fire(GH_Document document, PointF start, double angleDegrees, bool pierce)
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            if (document == null || canvas == null)return;
            if (!ReferenceEquals(_canvas, canvas))
            {
                if (_canvas != null)
                    _canvas.CanvasPostPaintObjects -= OnPostPaintObjects;
                _canvas = canvas;
                _canvas.CanvasPostPaintObjects += OnPostPaintObjects;
            }
            _document = document;
            double radians = angleDegrees * Math.PI / 180.0;
            _arrows.Add(new Arrow
            {
                Tip = start,
                Dx = (float)Math.Cos(radians),
                Dy = -(float)Math.Sin(radians), // canvas Y points down
                Pierce = pierce,
             Flying = true
            });
            _timer.Start();
        }
        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= OnTick;
            _timer.Dispose();
            if (_canvas != null)
            {
                _canvas.CanvasPostPaintObjects -= OnPostPaintObjects;
                _canvas.Invalidate();
            }
            _arrows.Clear();
            _canvas = null;
            _document = null;
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (_document == null || _canvas == null)
            {
                StopFlying();
                return;
            }
            foreach (Arrow arrow in new List<Arrow>(_arrows))
            {
                if (arrow.Flying)
                    Advance(arrow);
                else
                    arrow.Linger--;
            }
            _arrows.RemoveAll(a => !a.Flying && a.Linger <= 0);
            if (_arrows.Count == 0)
                StopFlying();
            _canvas.Invalidate();
        }
        private void StopFlying()
        {
            _timer.Stop();
            if (_canvas != null)
            {
                _canvas.CanvasPostPaintObjects -= OnPostPaintObjects;
                _canvas.Invalidate();
                _canvas = null;
           }
        }
        private void Advance(Arrow arrow)
        {
            float travelled = 0f;
            while (arrow.Flying && travelled < SpeedPerTick)
            {
                arrow.Tip = new PointF(arrow.Tip.X + arrow.Dx * StepLength, arrow.Tip.Y + arrow.Dy * StepLength);
                arrow.Distance += StepLength;
                travelled += StepLength;
                IGH_DocumentObject target = FindTarget(arrow.Tip);
                if (target != null)
                {
                    Remove(target);
                    if (!arrow.Pierce)
                        arrow.Flying = false;
                }
                if (arrow.Distance >= MaxDistance)
                    arrow.Flying = false;
            }
            if (!arrow.Flying)
                arrow.Linger = LingerTicks;
        }
        /// <summary>Components, parameters, sliders and panels can be hit. Groups, scribbles and the gun itself cannot.</summary>
        private IGH_DocumentObject FindTarget(PointF point)
        {
            foreach (IGH_DocumentObject obj in new List<IGH_DocumentObject>(_document.Objects))
            {
                if (obj.InstanceGuid == _gunId || obj.Attributes == null)
                    continue;
                if (!(obj is IGH_Component) && !(obj is IGH_Param))
                    continue;
                if (obj.Attributes.Bounds.Contains(point))
                    return obj;
            }
            return null;
        }
        private void Remove(IGH_DocumentObject obj)
        {
            try
            {
                // Record the removal first so Ctrl+Z can bring the component back.
                var record = _document.UndoUtil.CreateRemoveObjectEvent("Gun shot", obj);
                _document.UndoUtil.RecordEvent(record);
                _document.RemoveObject(obj, true);
            }
            catch (Exception ex)
            {
                Rhino.RhinoApp.WriteLine("PARAMANIACS Gun: could not remove a component: " + ex.Message);
            }
        }
        private void OnPostPaintObjects(GH_Canvas sender)
        {
            if (_arrows.Count == 0 || !ReferenceEquals(sender.Document, _document))
                return;
            Graphics g = sender.Graphics;
            if (g == null)
                return;
            SmoothingMode oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var shaft = new Pen(Color.FromArgb(110, 70, 30), 3f))
            using (var fletching = new Pen(Color.FromArgb(200, 30, 30), 2f))
            using (var headBrush = new SolidBrush(Color.FromArgb(200, 30, 30)))
            {
                foreach (Arrow arrow in _arrows)
                {
                    float px = -arrow.Dy; // perpendicular direction
                    float py = arrow.Dx;
                    PointF tip = arrow.Tip;
                    PointF tail = new PointF(tip.X - arrow.Dx * ArrowLength, tip.Y - arrow.Dy * ArrowLength);
                    g.DrawLine(shaft, tail, tip);
                    PointF headBase = new PointF(tip.X - arrow.Dx * 14f, tip.Y - arrow.Dy * 14f);
                    g.FillPolygon(headBrush, new[]
                    {
                        tip,
                        new PointF(headBase.X + px * 6f, headBase.Y + py * 6f),
                        new PointF(headBase.X - px * 6f, headBase.Y - py * 6f)
                    });
                    PointF fletchStart = new PointF(tail.X + arrow.Dx * 10f, tail.Y + arrow.Dy * 10f);
                    g.DrawLine(fletching, fletchStart, new PointF(tail.X - arrow.Dx * 4f + px * 5f, tail.Y - arrow.Dy * 4f + py * 5f));
                    g.DrawLine(fletching, fletchStart, new PointF(tail.X - arrow.Dx * 4f - px * 5f, tail.Y - arrow.Dy * 4f - py * 5f));
                }
            }
            g.SmoothingMode = oldMode;
        }
        private sealed class Arrow
        {
            public PointF Tip;
            public float Dx;
            public float Dy;
            public float Distance;
            public bool Pierce;
            public bool Flying;
            public int Linger;
        }
    }
}