using System;
using Rhino;
using Rhino.Commands;
using System.Linq;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_CreateHBModel : Command
    {
        static HB_CreateHBModel _instance;
        public HB_CreateHBModel()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_CreateHBModel command.</summary>
        public static HB_CreateHBModel Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_CreateHBModel"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: ask for a name:

            // TODO: create a default energy property

            // TODO: select all rooms in this rhino document
            var allRooms = doc.Objects
                .Where(_=>_.Geometry.IsRoom())
                .Select(_=>Rhino.Geometry.Brep.TryConvertBrep(_.Geometry).ToRoom()) ;
          

            // TODO: create a model.

            var model = new HoneybeeDotNet.Model.Model(
                "modelName",
                new HoneybeeDotNet.Model.ModelProperties(),
                "a new displace name"
                );
            model.Rooms.AddRange(allRooms);

            return Result.Success;
        }
    }
}