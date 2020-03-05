using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Geometry;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_SolveAdjacency : Command
    {
        static HB_SolveAdjacency _instance;
        public HB_SolveAdjacency()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_SolveAdjancy command.</summary>
        public static HB_SolveAdjacency Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_SolveAdjacency"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select closed objects for converting to Honeybee Room");

                go.GeometryFilter = ObjectType.Brep;
                go.GetMultiple(2, 0);
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount <= 1)
                    return go.CommandResult();


                //Must be all room objects.
                var rooms = go.Objects().Select(_ => _.Brep()).Where(_ => _ != null).Where(_ => _.IsSolid);
                //if (rooms.Any(_ => !_.IsRoom()))
                //{
                //    RhinoApp.WriteLine("Not all selected objects are Honeybee room, please double check, and convert it to room first.");
                //    return go.CommandResult();
                //}

                var ids = go.Objects().Select(_ => _.ObjectId).ToList();

                var tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance / 10;
                var dupRooms = rooms.Select(_ => _.DuplicateBrep());

                var adjSolver = new AdjacencySolver(dupRooms);

                var checkedObjs = adjSolver.Execute(tol, true);

                var counts = rooms.Count();


                var objs = checkedObjs.ToList();

                for (int i = 0; i < counts; i++)
                {
                    doc.Objects.Replace(ids[i], objs[i]);
                    //doc.Objects.Add(objs[i]);
                }

                doc.Views.Redraw();
                return Result.Success;


            }
        }
    }
}