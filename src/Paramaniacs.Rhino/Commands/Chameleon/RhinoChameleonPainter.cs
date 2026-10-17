using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Paramaniacs.Core.Colors;
using Rhino;
using Rhino.DocObjects;

namespace Paramaniacs.RhinoPlugin.Commands.Chameleon
{
    /// <summary>
    /// Gives every object in a Rhino document its own random color, and remembers the
    /// original color settings so they can be restored.
    ///
    /// Why attributes and not a display trick? Overriding colors in a display conduit only
    /// works for curves, not for shaded surfaces, so we change the object color instead.
    /// To keep the user's work safe:
    ///   - the change is wrapped in an undo record (Ctrl+Z works),
    ///   - the original color settings are kept in memory and can be restored,
    ///   - the original colors are put back automatically while the file is being saved,
    ///     so the chameleon colors never end up inside the saved file.
    /// </summary>
    internal static class RhinoChameleonPainter
    {
        private static readonly RandomColorGenerator Generator = new RandomColorGenerator();
        private static readonly Dictionary<RhinoDoc, DocState> States = new Dictionary<RhinoDoc, DocState>();
        private static bool _hooked;

        public static bool IsActive(RhinoDoc doc)
        {
            return States.ContainsKey(doc);
        }

        /// <summary>Gives every object a new random color. Returns how many objects were painted.</summary>
        public static int Shuffle(RhinoDoc doc)
        {
            Hook();

            DocState state;
            if (!States.TryGetValue(doc, out state))
            {
                state = new DocState();
                States[doc] = state;
            }

            RhinoObject[] objects = doc.Objects.GetObjectList(AllObjects()).ToArray();
            int count = 0;

            uint undoRecord = doc.BeginUndoRecord("PARAMANIACS Chameleon");
            try
            {
                foreach (RhinoObject obj in objects)
                {
                    // Only remember the very first (real) color settings of each object.
                    if (!state.Originals.ContainsKey(obj.Id))
                        state.Originals[obj.Id] = new OriginalColor(obj.Attributes.ColorSource, obj.Attributes.ObjectColor);

                    Color color = ToColor(Generator.Next());
                    state.Applied[obj.Id] = color;
                    if (SetColor(doc, obj, ObjectColorSource.ColorFromObject, color))
                        count++;
                }
            }
            finally
            {
                doc.EndUndoRecord(undoRecord);
            }

            if (count == 0)
                States.Remove(doc);

            doc.Views.Redraw();
            return count;
        }

        /// <summary>Puts the original colors back. Returns how many objects were restored.</summary>
        public static int Restore(RhinoDoc doc)
        {
            DocState state;
            if (!States.TryGetValue(doc, out state))
                return 0;

            int count;
            uint undoRecord = doc.BeginUndoRecord("PARAMANIACS Chameleon restore");
            try
            {
                count = RestoreOriginals(doc, state);
            }
            finally
            {
                doc.EndUndoRecord(undoRecord);
            }

            States.Remove(doc);
            doc.Views.Redraw();
            return count;
        }

        private static ObjectEnumeratorSettings AllObjects()
        {
            return new ObjectEnumeratorSettings
            {
                NormalObjects = true,
                LockedObjects = true,
                HiddenObjects = true,
                IncludeLights = false,
                IncludeGrips = false
            };
        }

        private static int RestoreOriginals(RhinoDoc doc, DocState state)
        {
            int count = 0;
            foreach (KeyValuePair<Guid, OriginalColor> pair in state.Originals)
            {
                RhinoObject obj = doc.Objects.FindId(pair.Key);
                if (obj == null)
                    continue;

                if (SetColor(doc, obj, pair.Value.Source, pair.Value.Color))
                    count++;
            }

            return count;
        }

        private static bool SetColor(RhinoDoc doc, RhinoObject obj, ObjectColorSource source, Color color)
        {
            ObjectAttributes attributes = obj.Attributes.Duplicate();
            attributes.ColorSource = source;
            attributes.ObjectColor = color;
            return doc.Objects.ModifyAttributes(obj, attributes, true);
        }

        private static Color ToColor(Rgb rgb)
        {
            return Color.FromArgb(rgb.R, rgb.G, rgb.B);
        }

        // ---- keeping the saved file clean -------------------------------------------------

        private static void Hook()
        {
            if (_hooked)
                return;

            _hooked = true;
            RhinoDoc.BeginSaveDocument += OnBeginSave;
            RhinoDoc.EndSaveDocument += OnEndSave;
            RhinoDoc.CloseDocument += OnClose;
        }

        private static void OnBeginSave(object sender, DocumentSaveEventArgs e)
        {
            DocState state;
            if (e.Document == null || !States.TryGetValue(e.Document, out state))
                return;

            try
            {
                RestoreOriginals(e.Document, state);
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("PARAMANIACS: could not put the original colors back before saving: " + ex.Message);
            }
        }

        private static void OnEndSave(object sender, DocumentSaveEventArgs e)
        {
            DocState state;
            if (e.Document == null || !States.TryGetValue(e.Document, out state))
                return;

            try
            {
                foreach (KeyValuePair<Guid, Color> pair in state.Applied)
                {
                    RhinoObject obj = e.Document.Objects.FindId(pair.Key);
                    if (obj != null)
                        SetColor(e.Document, obj, ObjectColorSource.ColorFromObject, pair.Value);
                }

                // The file was just saved, so don't mark it as changed again.
                e.Document.Modified = false;
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine("PARAMANIACS: could not bring the chameleon colors back after saving: " + ex.Message);
            }
        }

        private static void OnClose(object sender, DocumentEventArgs e)
        {
            if (e.Document != null)
                States.Remove(e.Document);
        }

        // ---- data --------------------------------------------------------------------------

        private sealed class DocState
        {
            public readonly Dictionary<Guid, OriginalColor> Originals = new Dictionary<Guid, OriginalColor>();
            public readonly Dictionary<Guid, Color> Applied = new Dictionary<Guid, Color>();
        }

        private struct OriginalColor
        {
            public OriginalColor(ObjectColorSource source, Color color)
            {
                Source = source;
                Color = color;
            }

            public readonly ObjectColorSource Source;
            public readonly Color Color;
        }
    }
}
