using System;
using System.Linq;
using System.Collections.Generic;
using HoneybeeRhino;
using HoneybeeRhino.Entities;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_MassToRoom : Command
    {
        static HB_MassToRoom _instance;
        public HB_MassToRoom()
        {
            _instance = this;
        }

        ///<summary>The only instance of the MyCommand1 command.</summary>
        public static HB_MassToRoom Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_MassToRoom"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select closed objects for converting to Honeybee Room");
               
                //Only Brep is accepted, because we need to save meta data to sub-surface as well. 
                //Extrusion doesn't have sub-surface.
                //all extrusion will be converted to Brep.
                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion;
                go.EnableClearObjectsOnEntry(false);
                go.EnableUnselectObjectsOnExit(false);
                go.DeselectAllBeforePostSelect = false;

                //check if any brep has been converted to Room
                var optionSkipExistingRoom_toggle = new OptionToggle(true, "No_RecreateAllRooms", "Yes");
                while (true)
                {
                    go.ClearCommandOptions();
                    go.AddOptionToggle("SkipExistingRoom", ref optionSkipExistingRoom_toggle);
                    var rc = go.GetMultiple(1, 0);
                    if (rc == GetResult.Option)
                    {
                        go.EnablePreSelect(false, true);
                        continue;
                    }
                    else if (rc != GetResult.Object)
                    {
                        return Result.Cancel;
                    }
                    if (go.ObjectsWerePreselected)
                    {
                        go.EnablePreSelect(false, true);
                        continue;
                    }
                    break;
                }

                if (go.CommandResult() != Result.Success)
                    return Result.Failure;

                if (go.ObjectCount == 0)
                    return Result.Nothing;

                var ifSkip = optionSkipExistingRoom_toggle.CurrentValue;

                //Getting objects  
                var solidBreps = go.Objects().Where(_ => _.Brep() != null).Where(_ => _.Brep().IsSolid);
                var objectToConvert = solidBreps;
                if (ifSkip)
                {
                    objectToConvert = solidBreps.Where(_ => !_.IsRoom()).ToList();
                }

                ConvertToRoom(doc, objectToConvert);

                doc.Views.Redraw();

                var count = objectToConvert.Count();
                var msg = count > 1 ? $"{count} Honeybee rooms were created successfully!" : $"{count} Honeybee room was created successfully!";
                RhinoApp.WriteLine(msg);
                return Result.Success; 


            }
        }

        void ConvertToRoom(RhinoDoc doc, IEnumerable<ObjRef> roomObjRefs)
        {
            //get current working model, and its GroupEntityTable for roomEntity to add
            var tb = HoneybeeRhinoPlugIn.Instance.ModelEntityTable;
            var modelEntity = tb.First().Value;

            //Convert Room brep
            foreach (var item in roomObjRefs)
            {
                var roomBrep = EntityHelper.ToRoomBrepObj(item);
                var success = doc.Objects.Replace(item, roomBrep);
                if (success)
                {
                    //Remove from model if exists
                    var found = modelEntity.Rooms.FirstOrDefault(_ => _.ObjectId == item.ObjectId);
                    if (found != null)
                        modelEntity.Rooms.Remove(found);

                    //add to model
                    modelEntity.Rooms.Add(new ObjRef(item.ObjectId));
                }
                else
                {
                    throw new ArgumentException("Failed to convert to honeybee room!");
                }


            }
        }
    }
}