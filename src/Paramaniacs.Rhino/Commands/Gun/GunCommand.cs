using System;
using System.Collections.Generic;
using System.Threading;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input;
using Rhino.Input.Custom;
namespace Paramaniacs.RhinoPlugin.Commands.Gun
{
    /// <summary>
    /// ParamaniacsGun: pick a block to be your gun, then click to shoot arrows at things.
    /// An arrow deletes the objects it hits. A hit means the arrow passes through the
    /// bounding box of an object. Locked and hidden objects (and the gun itself) are safe.
    /// Deletions are recorded in Rhino's undo list, so Undo brings the objects back.
    /// </summary>
    public sealed class GunCommand : Command
    {
        public GunCommand()
        {
            Instance = this;
        }
        public static GunCommand Instance { get; private set; }
        public override string EnglishName
        {
            get { return "ParamaniacsGun"; }
        }
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // Step 1: choose the gun (a block instance).
            var gunPicker = new GetObject();
            gunPicker.SetCommandPrompt("Select a block to use as the gun");
            gunPicker.GeometryFilter = ObjectType.InstanceReference;
            gunPicker.SubObjectSelect = false;
            if (gunPicker.Get() != GetResult.Object)
                return Result.Cancel;
            RhinoObject gun = gunPicker.Object(0).Object();
            if (gun == null)
                return Result.Failure;
            doc.Objects.UnselectAll();
            doc.Views.Redraw();
            // Step 2: shoot until the user presses Enter or Escape.
            var pierce = new OptionToggle(false, "Off", "On");
            var conduit = new ArrowConduit();
            int totalHits = 0;
            try
            {
                while (true)
                {
                    Point3d muzzle = gun.Geometry.GetBoundingBox(true).Center;
                    var aimPicker = new GetPoint();
                    aimPicker.SetCommandPrompt("Aim and click to shoot (Enter to stop)");
                    aimPicker.SetBasePoint(muzzle, true);
                    aimPicker.DrawLineFromPoint(muzzle, true);
                    aimPicker.AcceptNothing(true);
                    aimPicker.AddOptionToggle("Pierce", ref pierce);
                    GetResult result = aimPicker.Get();
                    if (result == GetResult.Option)
                        continue;
                    if (result != GetResult.Point)
                        break;
                    totalHits += Shoot(doc, gun, conduit, muzzle, aimPicker.Point(), pierce.CurrentValue);
                }
            }
            finally
            {
                conduit.Enabled = false;
                doc.Views.Redraw();
            }
            RhinoApp.WriteLine("PARAMANIACS: {0} object(s) were hit. Use Undo to bring them back.", totalHits);
            return Result.Success;
        }
        private static int Shoot(RhinoDoc doc, RhinoObject gun, ArrowConduit conduit, Point3d muzzle, Point3d aim, bool pierce)
        {
            Vector3d direction = aim - muzzle;
            if (!direction.Unitize())
                return 0;
            // Everything the arrow could hit: normal (not locked, not hidden) objects except the gun.
            var settings = new ObjectEnumeratorSettings
            {
                NormalObjects = true,
                LockedObjects = false,
                HiddenObjects = false,
                IncludeLights = false,
                IncludeGrips = false
            };
            var targets = new List<RhinoObject>();
            BoundingBox scene = BoundingBox.Empty;
            foreach (RhinoObject obj in doc.Objects.GetObjectList(settings))
            {
                if (obj.Id == gun.Id)
                    continue;
                targets.Add(obj);
                scene = BoundingBox.Union(scene, obj.Geometry.GetBoundingBox(true));
            }
            double reach = (scene.IsValid ? scene.Diagonal.Length : 100.0) * 2.0 + muzzle.DistanceTo(aim);
            Line ray = new Line(muzzle, direction * reach);
            // Find the objects whose bounding box the arrow passes through, nearest first.
            var hits = new List<KeyValuePair<double, RhinoObject>>();
            foreach (RhinoObject obj in targets)
            {
                Interval span;
                if (Intersection.LineBox(ray, obj.Geometry.GetBoundingBox(true), doc.ModelAbsoluteTolerance, out span))
                    hits.Add(new KeyValuePair<double, RhinoObject>(Math.Max(0.0, span.T0), obj));
            }
            hits.Sort((a, b) => a.Key.CompareTo(b.Key));
            if (!pierce && hits.Count > 1)
                hits.RemoveRange(1, hits.Count - 1);
            // The arrow stops at the first hit, or keeps flying when piercing or missing.
            double endParameter = (!pierce && hits.Count > 0) ? hits[0].Key : 1.0;
            Animate(doc, conduit, muzzle, ray.PointAt(endParameter), direction, reach * 0.03);
            // Delete what was hit, inside one undo record.
            int deleted = 0;
            if (hits.Count > 0)
            {
                uint undoRecord = doc.BeginUndoRecord("PARAMANIACS Gun");
                try
                {
                    foreach (KeyValuePair<double, RhinoObject> hit in hits)
                    {
                        if (doc.Objects.Delete(hit.Value.Id, false))
                            deleted++;
                    }
                }
                finally
                {
                    doc.EndUndoRecord(undoRecord);
                }
            }
            doc.Views.Redraw();
            return deleted;
        }
        private static void Animate(RhinoDoc doc, ArrowConduit conduit, Point3d start, Point3d end, Vector3d direction, double arrowLength)
        {
            const int frames = 18;
            conduit.Enabled = true;
            conduit.Visible = true;
            for (int i = 1; i <= frames; i++)
            {
                double fraction = i / (double)frames;
                Point3d tip = start + (end - start) * fraction;
                Point3d tail = start.DistanceTo(tip) < arrowLength ? start : tip - direction * arrowLength;
                conduit.Arrow = new Line(tail, tip);
                doc.Views.Redraw();
                RhinoApp.Wait();
                Thread.Sleep(12);
            }
            Thread.Sleep(250); // let the arrow "stick" for a moment
            conduit.Visible = false;
            doc.Views.Redraw();
        }
    }
}