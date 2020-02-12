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

        public static Entities.FaceEntity TryGetFaceEntity(this GeometryBase rhinoRef) => Entities.FaceEntity.TryGetFrom(rhinoRef);
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
        public static bool IsCoplanar(this Plane plane, Plane testPlane, double tolerance, bool testInvertedNormalOnly = false)
        {
            if (!plane.IsValid || !testPlane.IsValid)
                return false;

            tolerance = tolerance < Rhino.RhinoMath.ZeroTolerance? Rhino.RhinoMath.ZeroTolerance: tolerance;

            var eq0 = plane.GetPlaneEquation();
            var eq1 = testPlane.GetPlaneEquation();

            //sameNormalCoPlane
            var cop = 
                Math.Abs(eq0[0] - eq1[0]) < tolerance &&
                Math.Abs(eq0[1] - eq1[1]) < tolerance &&
                Math.Abs(eq0[2] - eq1[2]) < tolerance &&
                Math.Abs(eq0[3] - eq1[3]) < tolerance;

            //invertNormalCoPlane
            if ((!cop) || testInvertedNormalOnly)
            {
                cop =
                    Math.Abs(eq0[0] + eq1[0]) < tolerance &&
                    Math.Abs(eq0[1] + eq1[1]) < tolerance &&
                    Math.Abs(eq0[2] + eq1[2]) < tolerance &&
                    Math.Abs(eq0[3] + eq1[3]) < tolerance;
            }


            return cop;
        }

        public static bool IsCoplanar(this Surface surface, Surface testSurface, double tolerance, bool testInvertedNormalOnly = false)
        {
            if (!surface.IsValid || !testSurface.IsValid)
                return false;

            if (!surface.IsPlanar() || !testSurface.IsPlanar())
                return false;

            surface.TryGetPlane(out Plane plane);
            testSurface.TryGetPlane(out Plane testPlane);
            return plane.IsCoplanar(testPlane, tolerance, testInvertedNormalOnly);
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
        public static IEnumerable<Brep> SolveAdjacency(this IEnumerable<Brep> rooms, double tolerance)
        {
            var checkedObjs = rooms.AsParallel().AsOrdered().Select(_ => _.DuplicateBrep().IntersectMasses(rooms, tolerance)).ToList();
            var matchedObj = checkedObjs.Select(_ => _.DuplicateBrep().MatchInteriorFaces(checkedObjs, tolerance));
            return matchedObj;
        }

        public static Brep IntersectMasses(this Brep roomGeo, IEnumerable<Brep> otherRooms, double tolerance)
        {
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            //Check bounding boxes first
            var roomBBox = roomGeo.GetBoundingBox(false);
            var adjacentRooms = otherRooms.Where(_ => roomBBox.isIntersected(_.GetBoundingBox(false)));

            var currentBrep = roomGeo;
            var allBreps = adjacentRooms;

            var currentBrepFaces = currentBrep.Faces;
            foreach (Brep brep in allBreps)
            {
                var isDup = brep.IsDuplicate(currentBrep,tolerance);
                if (isDup)
                    continue;

                var cutters = new List<Brep>();
                foreach (var curBrepFace in currentBrepFaces)
                {
                    var coplanned = brep.Faces.Where(_ => _.UnderlyingSurface().IsCoplanar(curBrepFace, tolerance, true));
                    var faceCutters = coplanned.Where(_ => _.GetBoundingBox(false).isIntersected(roomBBox)).Select(_=>_.ToBrep());
                    cutters.AddRange(faceCutters);
                }
                //There is no overlapping area.
                if (!cutters.Any())
                    continue;

                var newBreps = currentBrep.Split(cutters, tolerance);
                if (!newBreps.Any())
                    continue;

                //assign new name ID to newly split faces.
                newBreps.Where(_ => _.Surfaces.Count == 1).ToList().ForEach(_ => _.TryGetFaceEntity().UpdateNameID());
                var newBrep = Brep.JoinBreps(newBreps, tolerance);

                currentBrep = newBrep.First();
                currentBrep.Faces.ShrinkFaces();


            }
            //move over the roomEntity to new geometry.
            //all faceEntities in Brep.surface stays even after split.
            var roomEnt = roomGeo.TryGetRoomEntity();
            if (roomEnt == null)
            {
                throw new ArgumentNullException("Room entity is null");
            }
            else
            {
                currentBrep.UserData.Add(roomEnt);
            }
         
            //TODO: update subsurfaces geometry data
            //Probably there is no need to update this geometry data until export to simulation engine.
            //No one needs this data.

            return currentBrep;

            
        }
        public static Brep MatchInteriorFaces(this Brep room, IEnumerable<Brep> otherRooms, double tolerance)
        {
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            //Check bounding boxes first
            var roomBBox = room.GetBoundingBox(false);
            var adjacentRooms = otherRooms.Where(_ => roomBBox.isIntersected(_.GetBoundingBox(false)));

            var currentBrep = room;
            var currentRoomEnt = currentBrep.TryGetRoomEntity();
            var adjBreps = adjacentRooms;

            //Check sub-faces
            var currentFaces = currentBrep.Faces;
            foreach (Brep adjBrep in adjBreps)
            {
                var adjRoomEnt = adjBrep.TryGetRoomEntity();
                var adjFaces = adjBrep.Faces;
                //var isDup = adjBrep.IsDuplicate(currentBrep, tolerance);
                if (adjRoomEnt.GroupEntityID == currentRoomEnt.GroupEntityID)
                    continue;

                //Loop through current room's sub-faces
                foreach (var curBrepFace in currentFaces)
                {
                    var currentFaceBbox = curBrepFace.GetBoundingBox(false);
                    var currentFaceArea = AreaMassProperties.Compute(curBrepFace).Area;
                    var coplanned = adjFaces.Where(_ => _.UnderlyingSurface().IsCoplanar(curBrepFace, tolerance, true));
                    var matches = coplanned
                        .Where(_ => _.GetBoundingBox(false).isIntersected(currentFaceBbox));

                    //ignore this face, and keep its original outdoor, ground, or surface;
                    if (!matches.Any())
                        continue;

                    foreach (var adjFace in matches)
                    {
                        var adjProp = AreaMassProperties.Compute(adjFace);
                        var curProp = AreaMassProperties.Compute(curBrepFace);
                        var p1 = adjProp.Centroid;
                        var p2 = curProp.Centroid;

                        //Check if two adjacent faces are really matching.
                        var isAreaSame = Math.Abs(adjProp.Area - curProp.Area) < tolerance;
                        var isCenterSame = adjProp.Centroid.DistanceToSquared(curProp.Centroid) < Math.Sqrt(tolerance);

                        if (isAreaSame && isCenterSame)
                        {
                            var adjEnt = adjBrep.Surfaces[adjFace.SurfaceIndex].TryGetFaceEntity();
                            var curEnt = currentBrep.Surfaces[curBrepFace.SurfaceIndex].TryGetFaceEntity();
                            adjEnt.HBObject.BoundaryCondition =  new HoneybeeSchema.Surface(new List<string>(2) { curEnt.HBObject.Name, currentRoomEnt.HBObject.Name });
                            curEnt.HBObject.BoundaryCondition = new HoneybeeSchema.Surface(new List<string>(2) { adjEnt.HBObject.Name, adjRoomEnt.HBObject.Name });
                        }
                    }

                }


            }


            return currentBrep;
        }

    }
}
