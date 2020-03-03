using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Geometry;
using System.Collections.Generic;
using HoneybeeRhino.Entities;
using HoneybeeRhino;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_AddWindowToRoom : Command
    {
        static HB_AddWindowToRoom _instance;
        public HB_AddWindowToRoom()
        {
            _instance = this;
        }

        ///<summary>The only instance of the MyCommand1 command.</summary>
        public static HB_AddWindowToRoom Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_AddWindowToRoom"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select honeybee rooms for adding windows to");
                go.GeometryFilter = ObjectType.Brep;
                go.GroupSelect = false;
                go.Get();

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();

                //Check if all selected geoes are hb rooms.
                var rooms = go.Objects().Where(_ => _.Object().Geometry.IsRoom());
                if (go.ObjectCount != rooms.Count())
                    return Result.Failure;


                //Get Room geometry guid
                //var roomIds = go.Objects().Select(_ => _.ObjectId);


                var rs = AddApertureBySurface(doc, rooms);

                doc.Views.Redraw();


                return rs;
            }
        }

        public Result AddApertureBySurface(RhinoDoc doc, IEnumerable<ObjRef> rooms)
        {
            //Select window geometry.
            var rc = Rhino.Input.RhinoGet.GetMultipleObjects("Please select planer surfaces as windows to add to rooms", false, ObjectType.Surface, out ObjRef[] SelectedObjs);
            if (rc != Result.Success)
                return rc;
            if (SelectedObjs == null || SelectedObjs.Length < 1)
                return Result.Failure;

            //Check intersection, maybe provide an option for use to split window surfaces for zones.
            //TODO: do this later

            //prepare BrepObjects
            var WinObjs = SelectedObjs.ToList();
            var room = rooms.First();
            var roomBrep = room.Brep();

            //match window to room 
            var matchedRoomApts = room.AddApertures(WinObjs);
            var apts = matchedRoomApts.apertures;

            if (!apts.Any())
                return Result.Failure;

            foreach (var aperture in apts)
            {
                doc.Objects.Replace(aperture.id, aperture.brep);
            }


            //Replace the rhino object in order to be able to undo/redo
            doc.Objects.Replace(room.ObjectId, matchedRoomApts.room);

            return Result.Success;


        }
    }
}