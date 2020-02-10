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

        public static Entities.ApertureEntity TryGetApertureEntity(this RhinoObject rhinoRef) => Entities.ApertureEntity.TryGetFrom(rhinoRef.Geometry);
        public static Entities.RoomEntity TryGetRoomEntity(this RhinoObject rhinoRef) => Entities.RoomEntity.TryGetFrom(rhinoRef.Geometry);
        public static Entities.ApertureEntity TryGetApertureEntity(this GeometryBase rhinoRef) => Entities.ApertureEntity.TryGetFrom(rhinoRef);
        public static Entities.RoomEntity TryGetRoomEntity(this GeometryBase rhinoRef) => Entities.RoomEntity.TryGetFrom(rhinoRef);
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
            var ent = Entities.GroupEntity.TryGetFrom(rhinoRef.Geometry);
            return ent.IsValid;
        }

        //https://github.com/mcneel/rhino-developer-samples/blob/6/rhinocommon/cs/SampleCsCommands/SampleCsExtrusion.cs#L122
        public static bool IsCoplanar(this Plane plane, Plane testPlane, double tolerance)
        {
            if (!plane.IsValid || !testPlane.IsValid)
                return false;

            tolerance = tolerance < Rhino.RhinoMath.ZeroTolerance? Rhino.RhinoMath.ZeroTolerance: tolerance;

            var eq0 = plane.GetPlaneEquation();
            var eq1 = testPlane.GetPlaneEquation();

            return Math.Abs(eq0[0] - eq1[0]) < tolerance &&
                   Math.Abs(eq0[1] - eq1[1]) < tolerance &&
                   Math.Abs(eq0[2] - eq1[2]) < tolerance &&
                   Math.Abs(eq0[3] - eq1[3]) < tolerance;
        }

        public static bool IsCoplanar(this Surface surface, Surface testSurface, double tolerance)
        {
            if (!surface.IsValid || !testSurface.IsValid)
                return false;

            if (!surface.IsPlanar() || !testSurface.IsPlanar())
                return false;

            surface.TryGetPlane(out Plane plane);
            testSurface.TryGetPlane(out Plane testPlane);
            return plane.IsCoplanar(testPlane, tolerance);
        }
        public static bool isIntersected(this BoundingBox room, BoundingBox anotherRoom)
        {
            var roomMax = room.Max;
            var roomMin = room.Min;

            var otherMax = anotherRoom.Max;
            var otherMin = anotherRoom.Min;

            //Check intersections for each dimension.
            var isXIntersected = IsBetween((roomMax.X, roomMin.X), (otherMax.X, otherMin.X));
            var isYIntersected = IsBetween((roomMax.Y, roomMin.Y), (otherMax.Y, otherMin.Y));
            var isZIntersected = IsBetween((roomMax.Z, roomMin.Z), (otherMax.Z, otherMin.Z));

            return isXIntersected && isYIntersected && isZIntersected;

            bool IsBetween((double Max, double Min) v1, (double Max, double Min) v2)
            {
                return v1.Max >= v2.Min && v2.Max >= v1.Min;
            }

        }

        public static bool SolveAdjacny(this Brep roomGeo, List<Brep> otherRooms, double tolerance)
        {
            if (!roomGeo.IsRoom())
                return false;

            if (otherRooms.Any(_ => !_.IsRoom()))
                return false;

            //Check bounding boxes first
            var roomBBox = roomGeo.GetBoundingBox(false);
            var intersectedRooms = otherRooms.Where(_ => roomBBox.isIntersected(_.GetBoundingBox(false)));

            var currentBrep = roomGeo;
            var allBreps = otherRooms;
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            foreach (Brep item in allBreps)
            {
                var tempBrep = currentBrep.Split(item, tolerance);
                if (tempBrep.Any())
                {
                    var newBrep = Brep.JoinBreps(tempBrep, tolerance);

                    currentBrep = newBrep.First();
                    currentBrep.Faces.ShrinkFaces();
                }
            }

            return true;

            
            
        }

    }
}
