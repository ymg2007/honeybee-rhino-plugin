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
    public class RoomEntityMouseCallback : MouseCallback
    {
        private List<RhinoObject> greiedOutObjs = new List<RhinoObject>();
        public Guid EditingObj { get; private set; } = Guid.Empty;
        private System.Drawing.Color _defaultColor = AppearanceSettings.ViewportBackgroundColor;

        public bool IsEditingRoom => !EditingObj.Equals(Guid.Empty);

    

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


            var ent = RoomEntity.TryGetFrom(selectedRoom.Geometry);
            if (ent.IsValid)
            {
                RhinoApp.EscapeKeyPressed += RhinoApp_EscapeKeyPressed;
                RhinoApp.WriteLine($"Double clicked on: {ent.Name}");
                EditingObj = ent.HostObjRef.ObjectId;
                greiedOutObjs = doc.Objects.Where(_ => (!_.IsHidden) && (!_.IsLocked) && (_.IsSelected(true) == 0)).ToList();

                foreach (var item in greiedOutObjs)
                {
                    doc.Objects.Lock(item, true);
                }

                //save the current default color
                var color = AppearanceSettings.ViewportBackgroundColor;
                _defaultColor = color;

                var newR = Math.Min(color.R + 30, 255);
                var newG = Math.Min(color.G + 30, 255);
                var newB = Math.Min(color.B + 30, 255);

                AppearanceSettings.ViewportBackgroundColor =  System.Drawing.Color.FromArgb(newR, newG, newB);
                e.View.Redraw();
            }


        }

        public void ExitEditing()
        {
            ExitGroupEntity(RhinoDoc.ActiveDoc);
        }
        private void ExitGroupEntity(RhinoDoc doc)
        {
            if (!this.IsEditingRoom)
                return;

            //reset
            RhinoApp.EscapeKeyPressed -= RhinoApp_EscapeKeyPressed;
            AppearanceSettings.ViewportBackgroundColor = this._defaultColor;
            EditingObj = Guid.Empty;

            //in case document is closed, doc is null
            if (doc == null)
                return;

            foreach (var item in greiedOutObjs)
            {
                doc.Objects.Unlock(item, true);
            }
           
            greiedOutObjs.Clear();

            doc.Views.Redraw();
            return;
        }

        private void RhinoApp_EscapeKeyPressed(object sender, EventArgs e)
        {
            ExitEditing();

        }
    }
}