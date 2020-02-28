using System;
using System.Linq;
using Eto.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI;
using Command = Rhino.Commands.Command;
using HoneybeeRhino.Entities;
using HoneybeeSchema;
using Rhino.Geometry;
using HoneybeeRhino;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_SetRoomEnergyProperties : Command
    {
        static HB_SetRoomEnergyProperties _instance;
        public HB_SetRoomEnergyProperties()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_ProgramTypes command.</summary>
        public static HB_SetRoomEnergyProperties Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_SetRoomEnergyProperties"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;
            RoomEnergyPropertiesAbridged roomEnergyProperties = new RoomEnergyPropertiesAbridged();

            if (mode == RunMode.Interactive)
            {
                var dialog = new UI.RoomEnergyPropertyDialog(roomEnergyProperties);
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    roomEnergyProperties = dialog_rc;
                    rc = Result.Success;
                }
                    
            }
            else
            {
                var msg = string.Format($"Scriptable version of {EnglishName} command not implemented.");
                RhinoApp.WriteLine(msg);
            }

            if (roomEnergyProperties == null)
            {
                RhinoApp.WriteLine("No valid room energy property was set!");
                return Result.Nothing;
            }

            //Get honeybee rooms 
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select Honeybee Rooms to set its energy properties");
   
                go.GeometryFilter = ObjectType.Brep;
                go.GetMultiple(1, 0); 
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return Result.Nothing;

                //get all Room Entities.
                var roomEnts = go.Objects().Select(_ => _.Geometry().TryGetRoomEntity()).Where(_=>_.IsValid);
                if (go.Objects().Length > roomEnts.Count())
                {
                    RhinoApp.WriteLine("Some selected geometries are not Honeybee room, please use MassToRoom to create rooms first!");
                    return Result.Failure;
                }
                  
                //Assign program types 
                foreach (var item in go.Objects())
                {
                    var geo = item.Brep();
                    geo = HoneybeeRhino.SetRoomEnergyProperties(geo, roomEnergyProperties);
                    doc.Objects.Replace(item.ObjectId, geo);
                }

                doc.Views.Redraw();
                return Result.Success;


            }

        }

        
    }
}