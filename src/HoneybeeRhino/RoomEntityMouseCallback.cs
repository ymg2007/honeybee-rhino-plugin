using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace HoneybeeRhino
{
    public class RoomEntityMouseCallback : Rhino.UI.MouseCallback
    {
        private List<RhinoObject> greiedOutObjs = new List<RhinoObject>();
        public Guid EditingObj { get; private set; } = Guid.Empty;
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            
            var doc = RhinoDoc.ActiveDoc;
            var selectedRoom = doc.Objects.GetSelectedObjects(false, false).FirstOrDefault(_=>_.IsRoom());
            //var greiedOutObjs = new List<RhinoObject>();
            if (selectedRoom == null)
            {
                foreach (var item in greiedOutObjs)
                {
                    doc.Objects.Unlock(item, true);
                }
                EditingObj = Guid.Empty;
                greiedOutObjs.Clear();
                e.View.Redraw();
                return;
            }

            //do nothing when currently is editing one object already.
            if (EditingObj != Guid.Empty)
                return;


            var ent = Entities.RoomEntity.TryGetFrom(selectedRoom.Geometry);
            if (ent.IsValid)
            {
                RhinoApp.WriteLine($"Double clicked on: {ent.GroupEntityID}");
                EditingObj = ent.HostGeoID;
                greiedOutObjs = doc.Objects.Where(_ => (!_.IsHidden) && (!_.IsLocked) && (_.IsSelected(true) == 0)).ToList();

                foreach (var item in greiedOutObjs)
                {
                    doc.Objects.Lock(item, true);
                }
                e.View.Redraw();
            }


        }

    }
}