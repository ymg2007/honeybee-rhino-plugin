using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB = HoneybeeDotNet.Model;
using RH = Rhino.Geometry;
using RHDoc = Rhino.DocObjects;

namespace HoneybeeRhino
{
    public static partial class Convert
    {
        public static RH.GeometryBase ToRoomGeo(this RH.GeometryBase roomGeometry, double maxRoofFloorAngle = 30)
        {
            var geo = roomGeometry;
            var brep = RH.Brep.TryConvertBrep(roomGeometry);
            if (brep != null)
            {
                var hbobj = brep.ToRoom();
                geo.UserDictionary.Set("HBData", hbobj.ToJson());
                geo.UserDictionary.Set("HBType", "Room");
                return geo;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");
            }

        }

        public static RH.GeometryBase ToApertureGeo(this RH.GeometryBase apertureGeometry)
        {
            var geo = apertureGeometry;
           
            if (geo is RH.PlaneSurface srf)
            {
                var hbobj = srf.ToAperture();
                geo.UserDictionary.Set("HBData", hbobj.ToJson());
                geo.UserDictionary.Set("HBType", "Aperture");
                return geo;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid planar object to convert to honeybee aperture!");
            }

        }
        /// <summary>
        /// This is the same as ToApertureGeo(), just for those are not familiar with the term "Aperture" in energy model.
        /// </summary>
        /// <param name="windowGeometry"></param>
        /// <returns>Rhino Surface with Honeybee data</returns>
        public static RH.GeometryBase ToWindowGeo(this RH.GeometryBase windowGeometry)
        {
            return windowGeometry.ToApertureGeo();

        }


    }
}
