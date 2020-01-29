using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
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
                go.SetCommandPrompt("Please select colsed objects for converting to Honeybee Room v1");
                go.GeometryFilter = ObjectType.Extrusion | ObjectType.Brep;
                go.Get();
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();


                //user data at Geomergy level is different at Brep level......
                var geos = go.Objects().Select(_ => _.Geometry()).ToList(); //this kept extrusion and brep types
                if (go.Objects().Any(_=>!_.Brep().IsSolid))
                {
                    RhinoApp.WriteLine("Not all objects are valid solid water-tight geometry!");
                    return Result.Failure;
                }
                var hbObjs = go.Objects().Select(_ => _.Brep().ToRoom().ToJson()).ToList(); 
              

                if (geos.Count() == hbObjs.Count())
                {
                    var total = geos.Count();
                    for (int i = 0; i < total; i++)
                    {

                        var geo = geos[i];
                        var json = hbObjs[i];
                        geo.UserDictionary.Set("HBData", json);
                    }
                    return Result.Success;
                }
                else
                {
                    RhinoApp.WriteLine("Something went wrong. Not all objects are valid solid water-tight geometry!");
                    return Result.Failure;
                }

            }
        }
    }
}