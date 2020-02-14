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

        public static Entities.HBObjEntity TryGetHBObjEntity(this GeometryBase rhinoRef) => Entities.HBObjEntity.TryGetFrom(rhinoRef);
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
        public static bool IsCoplanar(this Plane plane, Plane testPlane, double tolerance, bool sameNormal = true, bool fliptedNormal = false)
        {
            if (!plane.IsValid || !testPlane.IsValid)
                return false;

            tolerance = tolerance < Rhino.RhinoMath.ZeroTolerance? Rhino.RhinoMath.ZeroTolerance: tolerance;

            var eq0 = plane.GetPlaneEquation();
            var eq1 = testPlane.GetPlaneEquation();

            //sameNormalCoPlane
            var sameNormalCop =
                 Math.Abs(eq0[0] - eq1[0]) < tolerance &&
                 Math.Abs(eq0[1] - eq1[1]) < tolerance &&
                 Math.Abs(eq0[2] - eq1[2]) < tolerance &&
                 Math.Abs(eq0[3] - eq1[3]) < tolerance;

            var fliptedCop =
                Math.Abs(eq0[0] + eq1[0]) < tolerance &&
                Math.Abs(eq0[1] + eq1[1]) < tolerance &&
                Math.Abs(eq0[2] + eq1[2]) < tolerance &&
                Math.Abs(eq0[3] + eq1[3]) < tolerance;

            if (sameNormal && !fliptedNormal)
            {
                //check the same normal only
                return sameNormalCop;
            }
            else if (!sameNormal && fliptedNormal)
            {
                //check the inverted normal only
                return fliptedCop;
            }
            else
            {
                //check both
                return sameNormalCop || fliptedCop;
            }

        }

        public static bool IsCoplanar(this Surface surface, Surface testSurface, double tolerance, bool sameNormal = true, bool fliptedNormal = false)
        {
            if (!surface.IsValid || !testSurface.IsValid)
                return false;

            if (!surface.IsPlanar() || !testSurface.IsPlanar())
                return false;

            surface.TryGetPlane(out Plane plane);
            testSurface.TryGetPlane(out Plane testPlane);
            return plane.IsCoplanar(testPlane, tolerance, sameNormal, fliptedNormal);
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
        public static IEnumerable<Brep> SolveAdjacency(this IEnumerable<Brep> rooms, double tolerance, bool parallelCompute)
        {
            var checkedObjs = rooms.IntersectMasses(tolerance, parallelCompute).ToList();
            //var matchedObj = checkedObjs.Select(_ => _.DuplicateBrep().MatchInteriorFaces(checkedObjs, tolerance));
            return checkedObjs;

        }

        public static IEnumerable<(BrepFace roomFace, IEnumerable<BrepFace> matchedCutters)> GetAdjFaces(this Brep room, Brep adjacentRoom, double tolerance)
        {
            var cutters = new List<(BrepFace, IEnumerable<BrepFace>)>();
            var currentBrepFaces = room.Faces;
            var adjRoom = adjacentRoom;
            //var roomBBox = room.GetBoundingBox(false);
            foreach (var curBrepFace in currentBrepFaces)
            {
                var curFaceBBox = curBrepFace.GetBoundingBox(false);
                var coplanned = adjRoom.Faces.Where(_ => _.UnderlyingSurface().IsCoplanar(curBrepFace, tolerance, sameNormal:false, fliptedNormal:true));
                var faceCutters = coplanned.Where(_ => _.GetBoundingBox(false).isIntersected(curFaceBBox));
                if (faceCutters.Any())
                {
                    var srf = faceCutters.Select(_ => _.UnderlyingSurface());
                    var b = faceCutters.Select(_ => _.ToBrep().Surfaces[0]);
                    var srfByindex = adjRoom.Surfaces[faceCutters.First().SurfaceIndex];
                    var c = curBrepFace.ToBrep().Surfaces[0];
                    var c2 = curBrepFace.UnderlyingSurface();
                    var c3 = room.Surfaces[curBrepFace.SurfaceIndex];
                    cutters.Add((curBrepFace, faceCutters));
                }
                
            }
            return cutters;
        }

        public static IEnumerable<Brep> IntersectMasses(this IEnumerable<Brep> allRooms, double tolerance, bool parallelCompute = false)
        {

            if (allRooms.Count() <= 1)
                return allRooms;

 
            //match each room's adjacent rooms before do split.
            var copy = allRooms.Select(_ => _.DuplicateBrep()).ToList();
            var count = copy.Count;
            var roomAndAdjacents = new List<(Brep room, IEnumerable<Brep> adjacentRooms)>();
            var solo = new List<Brep>();
            for (int i = 0; i < count; i++)
            {
                var room = copy[i];
                var roomBBox = room.GetBoundingBox(false);
                var adjacentRooms = copy.Where(_ => _.GetBoundingBox(false).isIntersected(roomBBox)).Select(_ => _.DuplicateBrep().DeleteHBEntity());
                if (adjacentRooms.Any())
                {
                    var dupRoom = room.DuplicateBrep().DetachHBEntity();
                    roomAndAdjacents.Add((dupRoom, adjacentRooms));
                }
                else
                {
                    solo.Add(room);
                }
            }

            try
            {
                var intersected = new Brep[count];
                if (parallelCompute)
                {

                    //Parallel.ForEach(roomAndAdjacents, (item, state, index) =>
                    //{
                    //    var room = item.room;
                    //    var adjRooms = item.adjacentRooms;
                    //    var newRoom = room.IntersectWithMasses(adjRooms, tolerance);
                    //    intersected[index] = newRoom;

                    //});

                    intersected = roomAndAdjacents.AsParallel().AsOrdered().Select(_ => _.room.IntersectWithMasses(_.adjacentRooms, tolerance)).ToArray();
                }
                else
                {
                    intersected = roomAndAdjacents.Select(_ => _.room.IntersectWithMasses(_.adjacentRooms, tolerance)).ToArray();
                }
                //add HBObjEntity back to geometry again.
                var intersetedRooms = intersected.Select(_ => _.ReinstallHBEntity()).ToList();
                intersetedRooms.AddRange(solo);
                HoneybeeRhinoPlugIn.Instance.TempEntityHolder.Clear();
                //var isRoom = otherRooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));
                //var isRoom2 = intersected.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

                return intersetedRooms;
            }
            catch (Exception)
            {

                throw;
            }
            
           
        }

        private static Brep IntersectWithMasses(this Brep roomGeo, IEnumerable<Brep> adjacentRooms, double tolerance)
        {
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            //Check bounding boxes first
            var roomBBox = roomGeo.GetBoundingBox(false);

            var currentBrep = roomGeo;
            var allBreps = adjacentRooms;

            //var currentBrepFaces = currentBrep.Faces;
            //var isRoomValid = allBreps.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));
            //var isThisRoomValid = currentBrep.Surfaces.All(s => s.TryGetFaceEntity().IsValid);
            //var faceAreasBeforeSplit = currentBrepFaces.Select(_ => AreaMassProperties.Compute(_).Area);
            foreach (Brep adjBrep in allBreps)
            {
                var isDup = adjBrep.IsDuplicate(roomGeo, tolerance);
                if (isDup)
                    continue;

                //Get matched faces, and its adjacent cutters.
                var matchAndCutters = currentBrep.GetAdjFaces(adjBrep, tolerance);
                //There is no overlapping area.
                if (!matchAndCutters.Any())
                    continue;


                //Split and Join
                var solidBrep = currentBrep;
                var cuttedElements = new List<Brep>();
                foreach (var matchAndCutter in matchAndCutters)
                {
                    var currentRoomFace = matchAndCutter.roomFace;
                    var cutters = matchAndCutter.matchedCutters.Select(_=>_.DuplicateFace(false)).ToList();
                    //Split the current brep by cutters
                    var newBreps = solidBrep.Split(cutters, tolerance).SkipWhile(_ => _ == null);

                    if (!newBreps.Any())
                        continue;


                    //var ent1 = currentRoomFace.UnderlyingSurface().TryGetFaceEntity();
                    ////var ent2 = currentBrep.Surfaces[currentRoomFace.SurfaceIndex].TryGetFaceEntity();
                    //////var ent3 = currentRoomFace.ToBrep().Surfaces[0].TryGetFaceEntity();
                    ////var ent = currentRoomFace.TryGetFaceEntity();

                    var faceEntID = currentRoomFace.UnderlyingSurface().GetUserString("HBDataID");
                    //assign new name ID to newly split faces.
                    //DO NOT use following Linq expression, because ToList() creates new object, instead of referencing the same one. 
                    //newBreps.Where(_ => _.Faces.Count == 1).ToList().ForEach(_ => _.TryGetFaceEntity().UpdateID_CopyFrom(ent));
                    if (!string.IsNullOrEmpty(faceEntID))
                    {
                        foreach (var item in newBreps)
                        {
                            if (item.Faces.Count == 1)
                            {
                                item.Surfaces[0].SetUserString("HBDataID", faceEntID);
                            }
                        }
                    }
                 

                    //Join back to solid
                    var roomEnts = newBreps.Select(_ => _.Surfaces.Select(s=>s.TryGetFaceEntity()));
                    var newBrep = Brep.CreateSolid(newBreps, tolerance);
                    solidBrep = newBrep.First();
                    solidBrep.Faces.ShrinkFaces();
                }
                //just to make logically clear, but they are essentially the same.
                //solidBrep is only used in above foreach loop
                currentBrep = solidBrep;

            }
            //move over the roomEntity to new geometry.
            //all faceEntities in Brep.surface stays even after split.
            var roomEntID = roomGeo.GetUserString("HBDataID");
            if (!string.IsNullOrEmpty(roomEntID))
            {
                currentBrep.SetUserString("HBDataID", roomEntID);
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
            foreach (Brep adjBrep in adjBreps)
            {
                var adjRoomEnt = adjBrep.TryGetRoomEntity();
                //var adjFaces = adjBrep.Faces;
                //var isDup = adjBrep.IsDuplicate(currentBrep, tolerance);
                if (adjRoomEnt.GroupEntityID == currentRoomEnt.GroupEntityID)
                    continue;

                var matches = currentBrep.GetAdjFaces(adjBrep, tolerance);
                //ignore this face, and keep its original outdoor, ground, or surface;
                if (!matches.Any())
                    continue;

                foreach (var match in matches)
                {
                    var matchedSubFace = match.roomFace;
                    var curProp = AreaMassProperties.Compute(matchedSubFace);

                    var matchedadjFaces = match.matchedCutters;
                    var sameAreaFaces = matchedadjFaces.Where(_ => Math.Abs(AreaMassProperties.Compute(_).Area - curProp.Area) < tolerance);
                    if (!sameAreaFaces.Any())
                        continue;

                    //Check if two adjacent faces are really matching.
                    var sameCenterFaces = sameAreaFaces.Where(_ => AreaMassProperties.Compute(_).Centroid.DistanceToSquared(curProp.Centroid) < Math.Pow(tolerance, 2));
                    if (!sameCenterFaces.Any())
                        continue;

                    var matchedAdjFace = sameCenterFaces.First();

                    var adjEnt = adjBrep.Surfaces[matchedAdjFace.SurfaceIndex].TryGetFaceEntity();
                    var curEnt = currentBrep.Surfaces[matchedSubFace.SurfaceIndex].TryGetFaceEntity();
                    adjEnt.HBObject.BoundaryCondition = new HoneybeeSchema.Surface(new List<string>(2) { curEnt.HBObject.Name, currentRoomEnt.HBObject.Name });
                    curEnt.HBObject.BoundaryCondition = new HoneybeeSchema.Surface(new List<string>(2) { adjEnt.HBObject.Name, adjRoomEnt.HBObject.Name });
                }

            }


            return currentBrep;
        }


        public static Brep ToAllPlaneBrep(this Brep roomBrep, double tolerance = 0.0001)
        {
            var tol = tolerance;
            var surfs = roomBrep.Faces;
            var checkedSrfs = new List<Brep>();
       
            foreach (var srf in surfs)
            {
                var s = srf.UnderlyingSurface();
                if (s is PlaneSurface ps)
                {
                    checkedSrfs.Add(ps.ToBrep());
               
                }
                else if (srf.IsPlanar())
                {
                    var cv = srf.OuterLoop.To3dCurve();
                    var p = Brep.CreatePlanarBreps(cv, tol).First();
                    checkedSrfs.Add(p);

                }
                else
                {
                    throw new ArgumentException("Non-planar surfaces are not accepted!");
                }

            }

            //Method 1
            var solid = Brep.CreateSolid(checkedSrfs, tol).OrderBy(_ => _.Faces.Count).LastOrDefault();
            if (solid.IsSolid)
                return solid;

            //Method 2
            solid = new Brep();
            checkedSrfs.ToList().ForEach(_ => solid.Append(_));
            solid.JoinNakedEdges(tol);
            if (solid.IsSolid)
                return solid;

            //Method 3
            var joined = Brep.JoinBreps(checkedSrfs, tol).OrderBy(_ => _.Faces.Count).ToList();
            if (!joined.LastOrDefault().IsSolid)
            {
                solid = joined.Select(_ => _.CapPlanarHoles(tol)).SkipWhile(_ => _ == null).FirstOrDefault();
            }
            return solid;

        }


        public static Brep DetachHBEntity(this Brep honeybeeObj)
        {
            var hbObj = honeybeeObj.DuplicateBrep();
            if (hbObj.IsRoom())
            {
                var ent = hbObj.TryGetRoomEntity();
                hbObj.UserData.Remove(ent);
                var guid = Guid.NewGuid();
                hbObj.SetUserString("HBDataID", guid.ToString());
                HoneybeeRhinoPlugIn.Instance.TempEntityHolder.Add(guid, ent);
                

                foreach (var srf in hbObj.Surfaces)
                {
                    var srfEnt = srf.TryGetFaceEntity();
                    srf.UserData.Remove(srfEnt);
                    var srfGuid = Guid.NewGuid();
                    //Do not add to UserDictionary it would crash Rhino.
                    srf.SetUserString("HBDataID", srfGuid.ToString());
                    HoneybeeRhinoPlugIn.Instance.TempEntityHolder.Add(srfGuid, srfEnt);

                }
            }
            else if(hbObj.IsAperture())
            {
                //TODO:
            }
            return hbObj;
           
        }
        public static Brep ReinstallHBEntity(this Brep hbObj)
        {
            if (hbObj == null)
                return hbObj;

            var honeybeeObj = hbObj.DuplicateBrep();
            if (honeybeeObj.Faces.Count>1 && honeybeeObj.IsSolid)
            {
                //This is a room
                var data = honeybeeObj.GetUserString("HBDataID");
                if (string.IsNullOrEmpty(data))
                    return honeybeeObj;

                var guid = Guid.Parse(data);
                var found = HoneybeeRhinoPlugIn.Instance.TempEntityHolder.TryGetValue(guid, out Entities.HBObjEntity entity);
                if (!found)
                    return honeybeeObj;
                if (entity is Entities.RoomEntity roomEnt)
                {
                    var dup = new Entities.RoomEntity();
                    dup.Duplicate(roomEnt);
                    honeybeeObj.UserData.Add(dup);
                }

                //Now add subSurfaces
                foreach (var srf in honeybeeObj.Surfaces)
                {
                    var srfdata = srf.GetUserString("HBDataID");
                    if (string.IsNullOrEmpty(srfdata))
                        throw new ArgumentException("Lost Honeybee data after last step!");

                    var srfguid = Guid.Parse(srfdata);
                    var srffound = HoneybeeRhinoPlugIn.Instance.TempEntityHolder.TryGetValue(srfguid, out Entities.HBObjEntity faceentity);
                    if (!srffound)
                        throw new ArgumentException("This shouldn't be happening, but still lost Honeybee face data after last step!");
                    if (faceentity is Entities.FaceEntity faceEnt)
                    {
                        var dup = new Entities.FaceEntity();
                        dup.Duplicate(faceEnt);
                        srf.UserData.Add(dup);
                    }
                }
                return honeybeeObj;

            }
            else if (honeybeeObj.Faces.Count ==1)
            {
                //TODO: Aperture;
                //Might never be used. 
                return honeybeeObj;
            }
            else
            {
                //TODO: Shading
                //Might never be used. 
                return honeybeeObj;
            }

        }
        public static Brep DeleteHBEntity(this Brep honeybeeObj)
        {
            var hbObj = honeybeeObj.DuplicateBrep();
            if (hbObj.IsRoom())
            {
                var ent = hbObj.TryGetRoomEntity();
                hbObj.UserData.Remove(ent);
                foreach (var srf in hbObj.Surfaces)
                {
                    var srfEnt = srf.TryGetFaceEntity();
                    srf.UserData.Remove(srfEnt);
                }
            }
            else if (hbObj.IsAperture())
            {
                //TODO:
            }
            return hbObj;

        }
    }
}
