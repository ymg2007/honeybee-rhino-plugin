using System;
using Rhino;
using Rhino.Commands;
using Newtonsoft.Json;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using System.Collections.Generic;

namespace HoneybeeRhino.RhinoCommands
{
    public class GetBrepJson : Command
    {
        static GetBrepJson _instance;
        public GetBrepJson()
        {
            _instance = this;
        }

        ///<summary>The only instance of the GetBrepJson command.</summary>
        public static GetBrepJson Instance
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
                    string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
                    string json = System.IO.File.ReadAllText(file);
                    var breps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Rhino.Geometry.Brep>>(json);
                    foreach (var b in breps)
                    {
                        doc.Objects.AddBrep(b);
                    }
                    doc.Views.Redraw();
                    return Result.Success;
                }
                   

                var jsons = new List<string>();
                foreach (var item in go.Objects())
                {
                    var geo = item.Brep();
                    var json = JsonConvert.SerializeObject(geo);
                    jsons.Add(json);
                }
                
                return Result.Success;


            }
           
         
        }
    }
}