using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
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
                go.SetCommandPrompt("Please select colsed objects for converting to Honeybee Room");
                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion;
                go.Get();
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();


                var selectedObjs = go.Objects().Select(_ => _.Brep());
                var hbObjs = selectedObjs.Select(_ => _.ToRoom()).ToList();

                if (selectedObjs.Count() == hbObjs.Count())
                {
                    for (int i = 0; i < selectedObjs.Count(); i++)
                    {
                        var rhobj = go.Objects()[i].Geometry();
                        var json = hbObjs[i].ToJson();
                        rhobj.UserDictionary.Set("HBData", json);
                    }
                    return Result.Success;
                }
                else
                {

                    RhinoApp.WriteLine("There are some object are not valid solid water-tight geometry!");
                    return Result.Failure;
                }

            }
        }
    }
}