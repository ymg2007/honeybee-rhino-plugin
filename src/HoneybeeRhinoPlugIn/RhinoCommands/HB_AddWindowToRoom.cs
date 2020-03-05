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
                go.SubObjectSelect = false;
                go.GetMultiple(1, 0);

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();

                //Check if all selected geoes are hb rooms.
                var solidBreps = go.Objects().Where(_ => _.Brep() != null).Where(_ => _.Brep().IsSolid);
                var rooms = solidBreps.Where(_ => _.IsRoom()).ToList();
                if (solidBreps.Count() != rooms.Count())
                {
                    doc.Objects.UnselectAll();
                    var nonRooms = solidBreps.Where(_ => ! _.Brep().IsRoom());
                    foreach (var item in nonRooms)
                    {
                        doc.Objects.Select(item, true, true);
                    }

                    doc.Views.Redraw();
                    Rhino.UI.Dialogs.ShowMessage("These are not Honeybee rooms, please use MassToRoom to convert them first!", "Honeybee Rhino Plugin");
                    return Result.Failure;
                }
                  

                var rs = AddApertureBySurface(doc, rooms);
                doc.Views.Redraw();


                return rs;
            }
        }

        public Result AddApertureBySurface(RhinoDoc doc, IEnumerable<ObjRef> rooms)
        {
            //Select window geometry.
            using (var go2 = new GetObject())
            {
                go2.SetCommandPrompt("Please select planer window surfaces");
                go2.GeometryFilter = ObjectType.Surface;
                go2.GroupSelect = false;
                go2.DisablePreSelect();
                go2.EnableSelPrevious(false);
                go2.SubObjectSelect = false;
                go2.GetMultiple(1, 0);

                if (go2.CommandResult() != Result.Success)
                    return go2.CommandResult();

                if (go2.Objects().Count() == 0)
                    return Result.Failure;

                var SelectedObjs = go2.Objects();

          
                //prepare BrepObjects
                var WinObjs = SelectedObjs.ToList();
                //var room = rooms.First();
                //var roomBrep = room.Brep();

                //match window to room 
                var matchedRoomApts = rooms.AddApertures(WinObjs);

                foreach (var match in matchedRoomApts)
                {
                    var apts = match.apertures;

                    if (!apts.Any())
                        continue;

                    foreach (var aperture in apts)
                    {
                        doc.Objects.Replace(aperture.id, aperture.brep);
                    }

                    //Replace the rhino object in order to be able to undo/redo
                    doc.Objects.Replace(match.roomId, match.room);
                    RhinoApp.WriteLine($"{apts.Count} windows have been successfully added to room {match.roomId}");
                }
             

                return Result.Success;

            }
      

        }
    }
}