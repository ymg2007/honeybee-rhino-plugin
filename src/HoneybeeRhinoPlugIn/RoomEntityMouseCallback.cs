using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.ApplicationSettings;
using HoneybeeRhino.Entities;

namespace HoneybeeRhino
{
    public class RoomEntityMouseCallback : Rhino.UI.MouseCallback
    {
        private List<RhinoObject> greiedOutObjs = new List<RhinoObject>();
        public Guid EditingObj { get; private set; } = Guid.Empty;
        private System.Drawing.Color _defaultBackgroundColor = AppearanceSettings.ViewportBackgroundColor;
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            
            var doc = RhinoDoc.ActiveDoc;
            var selectedRoom = doc.Objects.GetSelectedObjects(false, false).FirstOrDefault(_=>_.Geometry.IsRoom());
            //var greiedOutObjs = new List<RhinoObject>();
            if (selectedRoom == null)
            {
                ExitGroupEntity(doc);
                return;
            }

            //do nothing when currently is editing one object already.
            if (EditingObj != Guid.Empty)
                return;


            var ent = global::HoneybeeRhino.Entities.RoomEntity.TryGetFrom(selectedRoom.Geometry);
            if (ent.IsValid)
            {
                RhinoApp.EscapeKeyPressed += RhinoApp_EscapeKeyPressed;
                RhinoApp.WriteLine($"Double clicked on: {ent.GroupEntityID}");
                EditingObj = ent.HostGeoID;
                greiedOutObjs = doc.Objects.Where(_ => (!_.IsHidden) && (!_.IsLocked) && (_.IsSelected(true) == 0)).ToList();

                foreach (var item in greiedOutObjs)
                {
                    doc.Objects.Lock(item, true);
                }

                AppearanceSettings.ViewportBackgroundColor =  System.Drawing.Color.FromArgb(_defaultBackgroundColor.R + 50, _defaultBackgroundColor.G + 50,_defaultBackgroundColor.B + 50);
                e.View.Redraw();
            }


        }

        private void ExitGroupEntity(RhinoDoc doc)
        {
            RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
            foreach (var item in greiedOutObjs)
            {
                doc.Objects.Unlock(item, true);
            }
            EditingObj = Guid.Empty;
            greiedOutObjs.Clear();
            AppearanceSettings.ViewportBackgroundColor = this._defaultBackgroundColor;
            doc.Views.Redraw();
            return;
        }

        private void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
        {
            RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
            if (EditingObj == Guid.Empty)
                return;

            ExitGroupEntity(RhinoDoc.ActiveDoc);


        }
    }
}