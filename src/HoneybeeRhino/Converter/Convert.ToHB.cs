using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB = HoneybeeSchema;
using RH = Rhino.Geometry;
using RHDoc = Rhino.DocObjects;

namespace HoneybeeRhino
{
    public static partial class Convert
    {

        public static List<double> ToDecimalList(this RH.Point3d point)
        {
            return (new List<double>() { 
                point.X, 
                point.Y, 
                point.Z }
            );
        }

        public static HB.Face3D ToHBFace3D(this RH.PlaneSurface surface)
        {
            return (surface as RH.Surface).ToHBFace3D();

        }

        public static HB.Face3D ToHBFace3D(this RH.Surface surface)
        {
            return (surface.ToBrep().ToHBFace3Ds().First());
           
        }

        public static List<HB.Face3D> ToHBFace3Ds(this RH.Brep brep)
        {
            var face3Ds = new List<HB.Face3D>();

            var bfaces = brep.Faces;
            foreach (var bface in bfaces)
            {

                var pts = new List<object>();

                if (bface.IsPlanar())
                {
                    var loops = bface.Loops;
                    foreach (var loop in loops)
                    {

                        var isPoly = loop.To3dCurve().TryGetPolyline(out RH.Polyline polyline);
                        if (isPoly)
                        {
                            var loopPts = polyline.Select(pt => pt.ToDecimalList()).ToList();
                            pts.Add(loopPts);
                        }
                        else
                        {
                            //TODO: maybe convert to mesh
                        }
                    }

                }
                else
                {
                    //TODO: convert it to mesh

                }

                //check if brep has holes
                var boundaryPts = pts.First() as List<List<double>>;
                var face3D = new HB.Face3D(boundaryPts);
                if (pts.Count > 1)
                {
                    face3D.Holes.AddRange(pts.Skip(1) as List<List<List<double>>>);
                }
                face3Ds.Add(face3D);
            }

            return face3Ds;
            
        }
        public static HB.Face ToHBFace(this RH.Surface surface, double maxRoofFloorAngle = 30)
        {
           
            var f = surface;
            var norm = f.NormalAt(0.5, 0.5);
            var floorBaseNorm = new RH.Vector3d(0, 0, -1);
            var roofBaseNorm = new RH.Vector3d(0, 0, 1);

            var face = new HB.Face(
                    Guid.NewGuid().ToString(),
                    f.ToHBFace3D(),
                    HB.Face.FaceTypeEnum.Wall,
                    new HB.Outdoors(),
                    new HB.FacePropertiesAbridged()
                    );

            var isGround = surface.PointAt(0.5, 0.5).Z <= 0;
            if (isGround)
            {
                face.BoundaryCondition = new HB.Ground();
            }


            if (CalAngle(norm, floorBaseNorm) <= maxRoofFloorAngle)
            {
                face.FaceType = HB.Face.FaceTypeEnum.Floor;
            }
            else if (CalAngle(norm, roofBaseNorm) <= maxRoofFloorAngle)
            {
                face.FaceType = HB.Face.FaceTypeEnum.RoofCeiling;
            }
            else
            {
                face.FaceType = HB.Face.FaceTypeEnum.Wall;
                //the rests are walls
            }
          
            return face;

            double CalAngle(RH.Vector3d v1, RH.Vector3d v2)
            {
                var cosA = v1 * v2 / (v1.Length * v2.Length);
                return Math.Acos(cosA);
            }
        }
        public static List<HB.Face> ToHBFaces(this RH.Brep brep, HB.Face.FaceTypeEnum faceType, HB.AnyOf<HB.Ground, HB.Outdoors, HB.Adiabatic, HB.Surface> boundaryCondition)
        {
            
            var faces = new List<HB.Face>();

            return brep.ToHBFace3Ds()
                .Select(_ => new HB.Face(
                    $"{faceType}_{Guid.NewGuid()}",
                    _,
                    faceType,
                    boundaryCondition,
                    new HB.FacePropertiesAbridged()
                    )
                ).ToList();
        }


        public static HB.Room ToRoom(this RH.Brep closedBrep, double maxRoofFloorAngle = 30)
        {
            if (closedBrep.IsSolid)
            {
                var dupBrep = closedBrep.DuplicateBrep();
                var subFaces = dupBrep.Faces.ToList();
                subFaces.ForEach(_ => _.ShrinkFace(RH.BrepFace.ShrinkDisableSide.ShrinkAllSides));
                var hbFaces = subFaces.Select(_ => _.ToHBFace(maxRoofFloorAngle)).ToList();

                for (int i = 0; i < hbFaces.Count; i++)
                {
                    var bFace = closedBrep.Surfaces[i];
                    var faceEnt = new Entities.FaceEntity(hbFaces[i]);
                    bFace.UserData.Add(faceEnt);
                }
               
                return new HB.Room($"Room_{Guid.NewGuid()}".ToString(), hbFaces, new HB.RoomPropertiesAbridged());

            }
            else
            {
                throw new ArgumentException("This rhino object is not a water-tight solid!");
            }

        }

        //public static HB.Room ToRoom(this RH.Extrusion extrusion, double maxRoofFloorAngle = 30)
        //{
        //    return extrusion.ToBrep().ToRoom(maxRoofFloorAngle);
        //}

        public static HB.Room ToRoom(this RH.Box box, double maxRoofFloorAngle = 30)
        {
            return box.ToBrep().ToRoom(maxRoofFloorAngle);
        }

        public static HB.Aperture ToAperture(this RH.Surface singleSurface)
        {
            if (singleSurface.IsPlanar())
            {
                var face3D = singleSurface.ToHBFace3D();
                return new HB.Aperture($"Aperture_{Guid.NewGuid()}", face3D, new HB.Outdoors(), new HB.AperturePropertiesAbridged());
            }
            else
            {
                throw new ArgumentException("Input aperture surface has to be planar!");
            }
        }

        public static HB.Aperture ToWindow(this RH.Surface singleSurface) => ToAperture(singleSurface);

        public static HB.Door ToDoor(this RH.Surface singleSurface)
        {
            var face3D = singleSurface.ToHBFace3D();
            return new HB.Door($"Door_{Guid.NewGuid()}", face3D, new HB.Outdoors(), new HB.DoorPropertiesAbridged());
        }



    }


}
