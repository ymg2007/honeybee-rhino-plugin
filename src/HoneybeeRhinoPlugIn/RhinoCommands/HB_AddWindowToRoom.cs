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
                var rooms = go.Objects().Where(_ => _.Geometry().IsRoom());
                if (go.ObjectCount != rooms.Count())
                    return Result.Failure;


                //Get Room geometry guid
                var roomIds = go.Objects().Select(_ => _.ObjectId);
        

                //Select window geometry.
                var rc = Rhino.Input.RhinoGet.GetMultipleObjects("Please select planer surfaces as windows to add to rooms", false, ObjectType.Surface, out ObjRef[] SelectedObjs);
                if (rc != Result.Success)
                    return rc;
                if (SelectedObjs == null || SelectedObjs.Length < 1)
                    return Result.Failure;

                //Check intersection, maybe provide an option for use to split window surfaces for zones.
                //TODO: do this later
           
                //add HBdata to window geometry 
                var WinObjs = SelectedObjs.Select(objref => objref.Object());
                var room = rooms.First();
                var dupRoomBrep = room.Brep().DuplicateBrep();

                var validRoomApertures = new List<(Guid id, Brep brep)>();
                foreach (var aperture in WinObjs)
                {
                    //match window to room 
                    var matchedRoomApt = dupRoomBrep.AddAperture((Brep.TryConvertBrep(aperture.Geometry), aperture.Id));
                    if (matchedRoomApt.aperture == null)
                        continue;

                    dupRoomBrep = matchedRoomApt.room;
                    validRoomApertures.Add((aperture.Id, matchedRoomApt.aperture));
                }

                if (!validRoomApertures.Any())
                    return Result.Failure;

#if DEBUG
                if (!dupRoomBrep.Surfaces.Where(_ => _.TryGetFaceEntity().Apertures.Any()).Any())
                    throw new ArgumentException("some thing wrong with assigning aperture!");
#endif
                //add to groupEntity.
                var groupEntity = dupRoomBrep.TryGetGroupEntity(HoneybeeRhinoPlugIn.Instance.GroupEntityTable);
#if DEBUG
                //this shouldn't be happening, because all honeybee room must have to be part of a group entity.
                if (!groupEntity.IsValid)
                    throw new ArgumentException("Failed to get valid group entity from room!");
#endif
                groupEntity.AddApertures(validRoomApertures.Select(_=>_.brep));

                //Replace the rhino object in order to be able to undo/redo
                foreach (var apt in validRoomApertures)
                {
                    doc.Objects.Replace(apt.id, apt.brep);
                }

             
                doc.Objects.Replace(room.ObjectId, dupRoomBrep);

                doc.Views.Redraw();


#if DEBUG
                var newRoom = Brep.TryConvertBrep(doc.Objects.FindId(room.ObjectId).Geometry);
                if (!newRoom.Surfaces.Where(_ => _.TryGetFaceEntity().Apertures.Any()).Any())
                    throw new ArgumentException("some thing wrong with assigning aperture!");
#endif
               
                

                return Result.Success;
            }
        }

        public void checkAddAperture()
        {
            //TODO: check co-planner


        }
    }
}