using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_GetInfo : Command
    {
        static HB_GetInfo _instance;
        public HB_GetInfo()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_GetInfo command.</summary>
        public static HB_GetInfo Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_GetInfo"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select closed objects for converting to Honeybee Room");
                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion;
                go.Get();
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();


                var selectedObjs = go.Objects()[0].Geometry();
                var json = selectedObjs.UserDictionary.GetString("HBData");
                if (!string.IsNullOrEmpty(json))
                {
                    var obj = HoneybeeSchema.Room.FromJson(json);
                    Rhino.UI.Dialogs.ShowEditBox("Honeybee Data", "Honeybee Schema Data can be shared across all platforms.", json, true, out string outJson);
                    return Result.Success;
                }
                else
                {

                    RhinoApp.WriteLine("Selected geometry doesn't contains any Honeybee data!");
                    return Result.Failure;
                }

            }
        }
    }
}