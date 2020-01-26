using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using System.Linq;

namespace HoneybeeRhino
{
    public class HoneybeeRhinoCommand : Command
    {
        public HoneybeeRhinoCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static HoneybeeRhinoCommand Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "HoneybeeRhinoCommand"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select a surface for converting to Honeybee Face3D");
                go.GeometryFilter = ObjectType.Brep;
                go.Get();
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();


                var selectedObjs = go.Objects().Select(_=> _.Geometry() as Brep);
                var hbFace3D = selectedObjs.Select(_ => _.ToHB());

                if (selectedObjs.Count() == hbFace3D.Count())
                {
                    RhinoApp.WriteLine($"There are {hbFace3D.Count()} surfaces have been converted to Honeybee faces!");
                    return Result.Success;
                }
                else
                {
                    return Result.Failure;
                }
                
            }

        }
    }
}
