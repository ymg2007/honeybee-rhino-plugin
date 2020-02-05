using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_RoomList : Command
    {
        static HB_RoomList _instance;
        public HB_RoomList()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_GetInfo command.</summary>
        public static HB_RoomList Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_RoomList"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var groups = HoneybeeRhinoPlugIn.Instance.GroupEntityTable;
            var groupZoneNames = groups.Select(_ => _.Key.ToString());
            Rhino.UI.Dialogs.ShowEditBox("Ladybug Tools", "All Honeybee Rooms:", string.Join("\n", groupZoneNames), true, out string outJson);
            return Result.Success;
        }
    }
}