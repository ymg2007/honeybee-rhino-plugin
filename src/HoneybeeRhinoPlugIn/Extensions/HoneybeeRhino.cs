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

        public static Brep SetRoomEnergyProperties(Brep roomBrep, RoomEnergyPropertiesAbridged roomEnergyProperties)
        {
            var geo = roomBrep.DuplicateBrep();
            var ent = geo.TryGetRoomEntity();
            if (!ent.IsValid)
                throw new ArgumentException("Non valid honeybee room brep!");

            ent.SetEnergyProp(roomEnergyProperties);
            return geo;
        }

    }
}
