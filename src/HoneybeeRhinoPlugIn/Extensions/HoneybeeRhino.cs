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

            //var enertyProp = ent.HBObject.Properties.Energy ?? new RoomEnergyPropertiesAbridged();
            //enertyProp.ProgramType = programType.Name;

            ent.HBObject.Properties.Energy = roomEnergyProperties;
            return geo;
        }

        public static Brep SetRoomLightingLoad(Brep roomBrep, LightingAbridged lightingLoad)
        {
            var geo = roomBrep.DuplicateBrep();
            var ent = geo.TryGetRoomEntity();
            if (!ent.IsValid)
                throw new ArgumentException("Non valid honeybee room brep!");

            var enertyProp = ent.HBObject.Properties.Energy ?? new RoomEnergyPropertiesAbridged();
            enertyProp.Lighting = lightingLoad;

            ent.HBObject.Properties.Energy = enertyProp;
            return geo;
        }

        public static Brep SetRoomConstructionSet(Brep roomBrep, ConstructionSetAbridged constructionSet)
        {
            var geo = roomBrep.DuplicateBrep();
            var ent = geo.TryGetRoomEntity();
            if (!ent.IsValid)
                throw new ArgumentException("Non valid honeybee room brep!");

            var enertyProp = ent.HBObject.Properties.Energy ?? new RoomEnergyPropertiesAbridged();
            enertyProp.ConstructionSet = constructionSet.Name;

            ent.HBObject.Properties.Energy = enertyProp;
            return geo;
        }
    }
}
