using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Geometry;
using System.Collections.Generic;
using HoneybeeRhino.Entities;
using HoneybeeRhino;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_AddShades : Command
    {
        static HB_AddShades _instance;
        public HB_AddShades()
        {
            _instance = this;
        }

        ///<summary>The only instance of the MyCommand1 command.</summary>
        public static HB_AddShades Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_AddShades"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            using (var go = new GetObject())
            {
                go.SetCommandPrompt("Please select planer shade surfaces");
                go.GeometryFilter = ObjectType.Surface | ObjectType.Brep;
                go.GroupSelect = false;
                go.SubObjectSelect = false;
                go.GetMultiple(1, 0);

                if (go.CommandResult() != Result.Success)
                    return go.CommandResult();

                if (go.Objects().Count() == 0)
                    return Result.Failure;

                var SelectedObjs = go.Objects();


                //prepare BrepObjects
                var shades = SelectedObjs.ToList(); 

                //add shades to model
                ConvertToShades(doc, shades);

                return Result.Success;

            }

           
        }

        private void ConvertToShades(RhinoDoc doc, IEnumerable<ObjRef> shadeObjRefs)
        {
            //get current working model
            var tb = HoneybeeRhinoPlugIn.Instance.ModelEntityTable;
            var modelEntity = tb.First().Value;

            //Convert Room brep
            foreach (var item in shadeObjRefs)
            {
                var geo = item.Brep().DuplicateBrep();
                var shadeBrep = EntityHelper.ToShadeBrep(geo, item.ObjectId);
                var success = doc.Objects.Replace(item, shadeBrep);
                if (success)
                {
                    //Remove from model if exists
                    var found = modelEntity.OrphanedShades.FirstOrDefault(_ => _.ObjectId == item.ObjectId);
                    if (found != null)
                        modelEntity.OrphanedShades.Remove(found);

                    //add to model
                    modelEntity.OrphanedShades.Add(new ObjRef(item.ObjectId));
                }
                else
                {
                    throw new ArgumentException("Failed to convert to honeybee shades!");
                }


            }
        }


    }
}