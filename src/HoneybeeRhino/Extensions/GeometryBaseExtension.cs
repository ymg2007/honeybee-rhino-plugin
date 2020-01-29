using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace HoneybeeRhino
{
    public static class GeometryBaseExtension
    {
        public static string GetHBJson(this GeometryBase geometry)
        {
            var isHB = geometry.UserDictionary.TryGetString("HBData", out string json);
            if (isHB)
            {
                return json;
            }
            else
            {
                throw new ArgumentException("This is not a valid Honeybee geometery!");
            }
        }

        public static bool HasHBJson(this GeometryBase geometry)
        {
            return geometry.UserDictionary.TryGetString("HBData", out string json);
        }
    }
}
