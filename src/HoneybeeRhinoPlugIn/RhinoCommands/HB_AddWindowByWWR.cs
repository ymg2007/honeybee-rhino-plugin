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
            try
            {
                using (var go = new GetObject())
                {
                    var wwr = 0.6;
                    wwr = Settings.GetDouble(nameof(wwr), 0.6);
                    var optionWWR = new OptionDouble(wwr, 0.01, 0.98);

                    var optionSkipFaceExistingWindow = new OptionToggle(true, "No_CreateForAllFaces", "Yes");

                    go.SetCommandPrompt("Please select honeybee rooms for adding windows to");
                    go.GeometryFilter = ObjectType.Brep;
                    go.GroupSelect = false;
                    go.SubObjectSelect = false;
                    go.EnableClearObjectsOnEntry(false);
                    go.EnableUnselectObjectsOnExit(false);
                    go.DeselectAllBeforePostSelect = false;

                    bool bHavePreselectedObjects = false;

                    while (true)
                    {
                        go.ClearCommandOptions();
                        go.AddOptionDouble("WindowWallRatio", ref optionWWR);
                        go.AddOptionToggle("SkipFacesWithWindows", ref optionSkipFaceExistingWindow);
                        var res = go.GetMultiple(1, 0);

                        if (res == Rhino.Input.GetResult.Option)
                        {
                            go.EnablePreSelect(false, true);
                            continue;
                        }
                        else if (res != Rhino.Input.GetResult.Object)
                        {
                            return Result.Cancel;
                        }

                        if (go.ObjectsWerePreselected)
                        {
                            bHavePreselectedObjects = true;
                            go.EnablePreSelect(false, true);
                            continue;
                        }

                        break;
                    }

                    if (go.CommandResult() != Result.Success)
                        throw new ArgumentException("Failed to execute command!");

                    if (go.ObjectCount == 0)
                        throw new ArgumentException("No object is selected!");

                    //get option values
                    Settings.SetDouble(nameof(wwr), optionWWR.CurrentValue);
                    var ifSkipFaceWithWindow = optionSkipFaceExistingWindow.CurrentValue;

                    //all selected room geometries
                    var solidBreps = go.Objects().Where(_ => _.Brep() != null).Where(_ => _.Brep().IsSolid);
                    var rooms = solidBreps.Where(_ => _.IsRoom()).ToList();
                    if (solidBreps.Count() != rooms.Count())
                    {
                        doc.Objects.UnselectAll();
                        var nonRooms = solidBreps.Where(_ => !_.Brep().IsRoom());
                        foreach (var item in nonRooms)
                        {
                            doc.Objects.Select(item, true, true);
                        }

                        doc.Views.Redraw();
                        Rhino.UI.Dialogs.ShowMessage("These are not Honeybee rooms, please use MassToRoom to convert them first!", "Honeybee Rhino Plugin");
                        return Result.Failure;
                    }


                    //Add Windows
                    AddApertureByWWR(doc, rooms, optionWWR.CurrentValue, ifSkipFaceWithWindow);

                    doc.Views.Redraw();
                    return Result.Success;
                }
            }
            catch (Exception e)
            {

                RhinoApp.WriteLine($"ERROR: {e.Message}");
                return Result.Failure;
            }
            
        }
        public void AddApertureByWWR(RhinoDoc doc, IEnumerable<ObjRef> rooms, double wwr, bool skipFaceWithWindow = true)
        {
            foreach (var room in rooms)
            {
                AddApertureByWWR(doc, room, wwr, skipFaceWithWindow);
            }
        }

        public void AddApertureByWWR(RhinoDoc doc, ObjRef roomObjRef, double wwr, bool skipFaceWithWindow = true)
        {
            //Get all outdoor faces
            var room = roomObjRef;
            var ourdoorWalls = room.Brep().Faces.Where(_ => {
                var ent = _.TryGetFaceEntity();

                //skip if face has aperture already
                if (ent.ApertureObjRefs.Any() && skipFaceWithWindow)
                    return false;

                var face = ent.HBObject;
                var isOutdoor = face.BoundaryCondition.Obj is HoneybeeSchema.Outdoors;
                var isWall = face.FaceType == HoneybeeSchema.Face.FaceTypeEnum.Wall;
                return isOutdoor && isWall;
            });

            //prepare BrepObjects
            //create window
            var apertures = new List<ObjRef>();
            //Brep newRoomWithApt = null;
            foreach (var wall in ourdoorWalls)
            {
                var apt = wall.DuplicateFace(false);
                var c = AreaMassProperties.Compute(apt).Centroid;
                var ts = Transform.Scale(c, Math.Sqrt(wwr));
                var success = apt.Transform(ts);
                if (!success)
                    continue;

                var id = doc.Objects.AddBrep(apt);
                var aperture = new ObjRef(id);
                apertures.Add(aperture);

            }

            //match window to room 
            var matchedRoomApt = room.AddApertures(apertures);
            //TODO: figure out remove those are unsuccessfully added.
            //if (matchedRoomApt.apertures == null)
            //{
            //    //Failed to add 
            //    doc.Objects.Delete(id, true);
            //    continue;
            //}

            //by here, two lists are equal length
            var apts = matchedRoomApt.apertures;
            foreach (var item in apts)
            {
                doc.Objects.Replace(item.id, item.brep);
            }

            var newRoomWithApt = matchedRoomApt.room;
            if (newRoomWithApt != null)
            {
                //Replace the rhino object in order to be able to undo/redo
                doc.Objects.Replace(room.ObjectId, newRoomWithApt);
            }

        }
    }
}