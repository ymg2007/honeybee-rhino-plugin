using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace HoneybeeRhino
{
    public static class GeometryBaseExtension
    {

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

        public static bool IsCoplanar(this BrepFace face, BrepFace otherFace, double tolerance, bool sameNormal = true, bool fliptedNormal = false)
        {
            if (!face.IsValid || !otherFace.IsValid)
                return false;

            if (!face.IsPlanar() || !otherFace.IsPlanar())
                return false;

            face.TryGetPlane(out Plane plane, tolerance);
            otherFace.TryGetPlane(out Plane testPlane, tolerance);

            //Ensure the plane's normal is real normal
            if (face.OrientationIsReversed)
                plane.Flip();

            if (otherFace.OrientationIsReversed)
                testPlane.Flip();

            return plane.IsCoplanar(testPlane, tolerance, sameNormal, fliptedNormal);
        }
        public static bool isIntersected(this BoundingBox room, BoundingBox anotherRoom, double tolerance)
        {
            //Room cannot be intersected with self.
            var issame = room.Equals(anotherRoom);
            if (issame)
                return false;

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
                var v1A = v1.Max - v2.Min;
                var v2A = v2.Max - v1.Min;
                var isBetween1 = v1A >= 0 ? true : Math.Abs(v1A) <= tolerance;
                var isBetween2 = v2A >= 0 ? true : Math.Abs(v2A) <= tolerance;
            
                return isBetween1 && isBetween2;
            }

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
                var coplanned = adjRoom.Faces.Where(_ => _.IsCoplanar(curBrepFace, tolerance, sameNormal: false, fliptedNormal: true));
                var faceCutters = coplanned.Where(_ => _.GetBoundingBox(false).isIntersected(curFaceBBox, tolerance));
                if (faceCutters.Any())
                {
                    cutters.Add((curBrepFace, faceCutters));
                }

            }
            return cutters;
        }

        public static Brep ToAllPlaneBrep(this Brep roomBrep, double tolerance = 0.0001)
        {
            var tol = tolerance;
            var surfs = roomBrep.Faces;
            surfs.ShrinkFaces();
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
        public static bool SelectHighlight(this IEnumerable<ObjRef> objects)
        {

            var rc = true;
            foreach (var item in objects)
            {
                //Check if object is visible or locked. deleted
                var obj = item.Object();

                if (obj == null)
                    continue;

                if (!obj.IsValid)
                    continue;

                if (obj.IsLocked || obj.IsDeleted || obj.IsHidden)
                    continue;

                //the entire object (including subobjects) is already selected
                //Do nothing
                if (obj.IsSelected(false) == 2)
                    continue;

                //Select and highlight obj
                rc = rc && Rhino.RhinoDoc.ActiveDoc.Objects.Select(item.ObjectId, true, true);

            }
            return rc;
        }


    }
}
