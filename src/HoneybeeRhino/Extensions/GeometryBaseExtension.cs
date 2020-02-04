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
        //public static bool IsRoom(this ObjRef rhinoRef) => rhinoRef.Geometry().IsRoom();
        public static bool IsRoom(this RhinoObject rhinoRef) => rhinoRef.Geometry.IsRoom();
        public static bool IsRoom(this GeometryBase geometry)
        {
            var ent = Entities.RoomEntity.TryGetFrom(geometry);
            return ent.IsValid;
            
        }

        //public static bool IsAperture(this ObjRef rhinoRef) => rhinoRef.Geometry().IsAperture();
        public static bool IsAperture(this RhinoObject rhinoRef) => rhinoRef.Geometry.IsAperture();
        public static bool IsAperture(this GeometryBase geometry)
        {
            var ent = Entities.ApertureEntity.TryGetFrom(geometry);
            return ent.IsValid;

        }
        

        //public static string GetHBJson(this GeometryBase geometry)
        //{
        //    var isHB = geometry.UserDictionary.TryGetString("HBData", out string json);
        //    if (isHB)
        //    {
        //        return json;
        //    }
        //    else
        //    {
        //        throw new ArgumentException("This is not a valid Honeybee geometery!");
        //    }
        //}

        public static bool HasHBObjEntity(this GeometryBase geometry)
        {
            var ent = Entities.HBObjEntity.TryGetFrom(geometry);
            return ent != null;
        }

        public static bool HasGroupEntity(this RhinoObject rhinoRef)
        {
            var ent = Entities.GroupEntity.TryGetFrom(rhinoRef);
            return ent.IsValid;
        }
    }
}
