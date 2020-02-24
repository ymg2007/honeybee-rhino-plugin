using System;
using System.Linq;
using HoneybeeRhino;
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
                //all extrusion will be converted to Brep.
                go.GeometryFilter = ObjectType.Brep | ObjectType.Extrusion;
                go.GetMultiple(1, 0);
                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.ObjectCount == 0)
                    return go.CommandResult();

                var groupEntTable = HoneybeeRhinoPlugIn.Instance.GroupEntityTable;
                foreach (var item in go.Objects())
                {
                    Func<Brep, bool> func = (b) => doc.Objects.Replace(item, b);
                    var brepO = item.Object();
                    if (brepO is ExtrusionObject ex)
                    {
                        var b = Brep.TryConvertBrep(ex.Geometry);
                        doc.Objects.Replace(item, b);
                    }
                    item.ToRoomBrepObj(func, groupEntTable);

                }
                
                doc.Views.Redraw();
                return Result.Success; 


            }
        }
    }
}