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
    public class HB_SetProgramType : Command
    {
        static HB_SetProgramType _instance;
        public HB_SetProgramType()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_ProgramTypes command.</summary>
        public static HB_SetProgramType Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_SetProgramType"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var rc = Result.Cancel;
            HoneybeeSchema.ProgramTypeAbridged programType = null;

            if (mode == RunMode.Interactive)
            {
                var dialog = new UI.ProgramTypesDialog();
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    programType = dialog_rc;
                    rc = Result.Success;
                }
                var rcc = dialog.Result;
                    
            }
            else
            {
                var msg = string.Format($"Scriptable version of {EnglishName} command not implemented.");
                RhinoApp.WriteLine(msg);
            }

            if (programType == null)
            {
                RhinoApp.WriteLine("No program type was selected!");
                return Result.Nothing;
            }

            //Get honeybee rooms 
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select Honeybee Rooms to set its program type");

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
                    geo = HoneybeeRhino.SetRoomProgramType(geo, programType);
                    doc.Objects.Replace(item.ObjectId, geo);
                }

                doc.Views.Redraw();
                return Result.Success;


            }

        }

        
    }
}