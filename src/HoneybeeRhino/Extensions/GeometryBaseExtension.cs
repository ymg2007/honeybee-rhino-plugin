using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace HoneybeeRhino
{
    public static class GeometryBaseExtension
    {
        public static bool IsRoom(this ObjRef rhinoRef) => rhinoRef.Geometry().IsRoom();
        public static bool IsRoom(this RhinoObject rhinoRef) => rhinoRef.Geometry.IsRoom();
        public static bool IsRoom(this GeometryBase geometry)
        {
            if (geometry.UserDictionary.TryGetString("HBType", out string typeName))
            {
                return typeName == "Room";
            }
            else
            {
                return false;
            }
            
        }

        public static bool IsAperture(this ObjRef rhinoRef) => rhinoRef.Geometry().IsAperture();
        public static bool IsAperture(this RhinoObject rhinoRef) => rhinoRef.Geometry.IsAperture();
        public static bool IsAperture(this GeometryBase geometry)
        {
            if (geometry.UserDictionary.TryGetString("HBType", out string typeName))
            {
                return typeName == "Aperture";
            }
            else
            {
                return false;
            }

        }
        

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

        public static bool HasGroupEntity(this RhinoObject rhinoRef)
        {
            var ent = Entities.GroupEntity.TryGet(rhinoRef);
            return ent.IsValid;
        }
    }
}
