using System;
using Rhino;
using Rhino.Commands;
using Newtonsoft.Json;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace HoneybeeRhino.RhinoCommands
{
    public class BrepJson : Command
    {
        static BrepJson _instance;
        public BrepJson()
        {
            _instance = this;
        }

        ///<summary>The only instance of the GetBrepJson command.</summary>
        public static BrepJson Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "GetBrepJson"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select Breps for converting to json");

                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion;
                go.GetMultiple(1, 0);
                //if (go.CommandResult() != Result.Success)
                //    return go.CommandResult();

                if (go.ObjectCount == 0)
                {
                    string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
                    string j = System.IO.File.ReadAllText(file);
                    var bs = JsonConvert.DeserializeObject<List<Rhino.Geometry.Brep>>(j);
                    foreach (var b in bs)
                    {
                        doc.Objects.AddBrep(b);
                    }
                    doc.Views.Redraw();
                    return Result.Success;
                }
                   
                var breps = go.Objects().Select(_ => _.Brep());
                var json = JsonConvert.SerializeObject(breps);

                string tempFile = Path.Combine(Path.GetTempPath(), "temp.json");

                using (StreamWriter file = new StreamWriter(tempFile, true))
                {
                    file.Write(json);
                }

                RhinoApp.WriteLine($"Saved to: {tempFile}");
                return Result.Success;


            }
           
         
        }
    }
}