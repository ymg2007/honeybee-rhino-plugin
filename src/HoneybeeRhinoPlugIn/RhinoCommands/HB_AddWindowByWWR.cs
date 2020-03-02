using System;
using System.Collections.Generic;
using System.Linq;
using HoneybeeRhino.Entities;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_AddWindowByWWR : Command
    {
        static HB_AddWindowByWWR _instance;
        public HB_AddWindowByWWR()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_AddWindowByWWR command.</summary>
        public static HB_AddWindowByWWR Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_AddWindowByWWR"; }
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
                var roomIds = go.Objects().Select(_ => _.ObjectId);


                ////Select window geometry.
                //var rc = Rhino.Input.RhinoGet.GetMultipleObjects("Please select planer surfaces as windows to add to rooms", false, ObjectType.Surface, out ObjRef[] SelectedObjs);
                //if (rc != Result.Success)
                //    return rc;
                //if (SelectedObjs == null || SelectedObjs.Length < 1)
                //    return Result.Failure;

                //Get all outdoor faces
                var room = rooms.First();
                var ourdoorWalls = room.Brep().Faces.Where(_ => {
                    var face = _.TryGetFaceEntity().HBObject;
                    var isOutdoor = face.BoundaryCondition.Obj is HoneybeeSchema.Outdoors;
                    var isWall = face.FaceType == HoneybeeSchema.Face.FaceTypeEnum.Wall;
                    return isOutdoor && isWall;
                    });

                //prepare BrepObjects
                //create window
                var aptGUIDs = new List<Guid>();
                foreach (var wall in ourdoorWalls)
                {
                    var apt = wall.DuplicateFace(false);
                    var c = AreaMassProperties.Compute(apt).Centroid;
                    var ts = Transform.Scale(c, 0.6);
                    var success = apt.Transform(ts);
                    if (!success)
                        continue;

                    var id = doc.Objects.AddBrep(apt);
                    aptGUIDs.Add(id);
                }


                Brep newRoomWithApt = null;
                foreach (var aptID in aptGUIDs)
                {
                    var aperture = new ObjRef(aptID);
                    //match window to room 
                    var matchedRoomApt = room.AddAperture(aperture);
                    if (matchedRoomApt.aperture == null)
                        continue;

                    newRoomWithApt = matchedRoomApt.room;
                    doc.Objects.Replace(aperture.ObjectId, matchedRoomApt.aperture);
                }

                if (newRoomWithApt == null)
                    return Result.Failure;

                //Replace the rhino object in order to be able to undo/redo
                doc.Objects.Replace(room.ObjectId, newRoomWithApt);

                doc.Views.Redraw();

                return Result.Success;
            }
        }
    }
}