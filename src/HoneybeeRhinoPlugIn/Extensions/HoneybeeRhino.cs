using HoneybeeRhino.Entities;
using HoneybeeSchema;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino
{
    public static class HoneybeeRhino
    {

        public static Brep SetRoomProgramType(Brep roomBrep, ProgramTypeAbridged programType)
        {
            var geo = roomBrep.DuplicateBrep();
            var ent = geo.TryGetRoomEntity();
            if (!ent.IsValid)
            {
                throw new ArgumentException("Non valid honeybee room brep!");
            }
            var enertyProp = ent.HBObject.Properties.Energy;
            if (enertyProp == null)
            {
                ent.HBObject.Properties.Energy = new HoneybeeSchema.RoomEnergyPropertiesAbridged();
            }
            ent.HBObject.Properties.Energy.ProgramType = programType.Name;
            return geo;
        }
    }
}
