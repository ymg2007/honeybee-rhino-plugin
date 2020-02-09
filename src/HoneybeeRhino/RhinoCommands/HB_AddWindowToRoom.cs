using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Geometry;
using System.Collections.Generic;

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
                //TODO: match windows to rooms 
                //add HBdata to window geometry 
                var WinObjs = SelectedObjs.Select(objref => objref.Object());
                
                foreach (var aperture in WinObjs)
                {
                    //Check aperture surface
                    var apertureBrep = Brep.TryConvertBrep(aperture.Geometry).DuplicateBrep();
                    apertureBrep.Faces.ToList().ForEach(_ => _.ShrinkFace(BrepFace.ShrinkDisableSide.ShrinkAllSides));
                    var apertureSrf = apertureBrep.Surfaces.First() as PlaneSurface;
                    apertureSrf.TryGetPlane(out Plane aptPlane);
                    var apertureBBox = apertureBrep.GetBoundingBox(false);

                    //Get room surfaces
                    var room = rooms.First();
                    var isRoomContainsAperture = room.Brep().GetBoundingBox(false).Contains(apertureBBox);
                    if (!isRoomContainsAperture)
                        continue;

                    //Check with room surface contains this aperture.
                    var roomSrfs = Brep.TryConvertBrep( room.Geometry()).DuplicateBrep().Surfaces;
                    var roomApertures = new List<Brep>();
                    foreach (var roomSrf in roomSrfs)
                    {
                        var srfBBox = roomSrf.GetBoundingBox(false);
                        //TODO: need to test following method performance
                        var isInside = srfBBox.Contains(apertureBBox, false);
                        var isIntersected = srfBBox.Contains(apertureBBox.Max, false) || srfBBox.Contains(apertureBBox.Min, false);
                        var isCoPlanner = roomSrf.IsCoplanar(apertureSrf, RhinoMath.ZeroTolerance);
                        if (isInside)
                        {
                            var apertureGeo = aperture.Geometry.ToApertureGeo(aperture.Id);
                            roomApertures.Add(apertureGeo);
                        }
                    }
                    //add to groupEntity.
                    if (roomApertures.Any())
                    {
                        var groupEntity = Entities.GroupEntity.TryGetFrom(room.Geometry());
                        if (groupEntity.IsValid)
                        {
                            groupEntity.AddApertures(roomApertures);
                        }
                        else
                        {
                            //this shouldn't be happening, because all honeybee room must have to be part of group entity.
                        }
                    }

                }

                


                //TODO: replace the rhino object in order to be able to undo/redo

                doc.Views.Redraw();
                return Result.Success;
            }
        }

        public void checkAddAperture()
        {
            //TODO: check co-planner


        }
    }
}