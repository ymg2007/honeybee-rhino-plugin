using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.UI;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_EditRoom : Command
    {
        static HB_EditRoom _instance;
        public HB_EditRoom()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_GetInfo command.</summary>
        public static HB_EditRoom Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_EditRoom"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            //HoneybeeRhinoPlugIn.Instance.ObjectSelectMode = "Aperture";
            //RhinoApp.WriteLine($"Selection mode: { HoneybeeRhinoPlugIn.Instance.ObjectSelectMode}");

            //using (var go = new GetObject())
            //{
            //    go.SetCommandPrompt("Please select closed objects for converting to Honeybee Room");
            //    go.GeometryFilter = ObjectType.Extrusion | ObjectType.Brep;
            //    go.Get();
            //    if (go.CommandResult() != Result.Success)
            //        return go.CommandResult();

            //    if (go.ObjectCount == 0)
            //        return go.CommandResult();


            //    var allOtherVisiableObjs = doc.Objects.Where(_ => (!_.IsHidden) && (!_.IsLocked) && (_.IsSelected(true) == 0));

            //    foreach (var item in allOtherVisiableObjs)
            //    {
            //        doc.Objects.Lock(item, true);
            //    }


            //    return Result.Success;


            //}
            return Result.Success;

        }
    }
}