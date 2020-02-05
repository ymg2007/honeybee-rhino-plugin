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
                go.SetCommandPrompt("Please select closed objects for converting to Honeybee Room");
                //Only Brep is accepted, because we need to save meta data to sub-surface as well. 
                //Extrusion doesn't have sub-surface.
                //Convert all extrusion to Brep first.
                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion; 
                go.Get();
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();


                foreach (var item in go.Objects())
                {
                    if (item.Object().ObjectType == ObjectType.Extrusion)
                    {
                        doc.Objects.Replace(item, Brep.TryConvertBrep(item.Geometry()));
                    }

                    item.Object().ToRoomGeo();
                }
                
                doc.Views.Redraw();
                
                //user data at Geometry level is different at Brep level......
                //go.Objects().ToList().ForEach(_ => _.Object().ToRoomGeo());
           
                return Result.Success; 


            }
        }
    }
}