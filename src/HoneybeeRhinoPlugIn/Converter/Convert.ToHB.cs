﻿using System;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;
using RH = Rhino.Geometry;

namespace HoneybeeRhino
{
    public static partial class Convert
    {
        static RH.Vector3d floorBaseNorm = new RH.Vector3d(0, 0, -1);
        static RH.Vector3d roofBaseNorm = new RH.Vector3d(0, 0, 1);

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
            return surface.ToBrep().Faces.First().ToHBFace3D();
        }

        public static HB.Face3D ToHBFace3D(this RH.BrepFace brepFace)
        {
            var pts = new List<object>();
            var bface = brepFace;
            if (bface.IsPlanar())
            {
                var loops = bface.Loops;
                foreach (var loop in loops)
                {

                    var isPoly = loop.To3dCurve().TryGetPolyline(out RH.Polyline polyline);
                    if (isPoly)
                    {
                        var loopPts = polyline.Distinct().Select(pt => pt.ToDecimalList()).ToList();
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

            return face3D;

        }

        public static List<HB.Face3D> ToHBFace3Ds(this RH.Brep brep)
        {
            var bfaces = brep.Faces;
            var face3Ds = bfaces.Select(_ => _.ToHBFace3D()).ToList();
            return face3Ds;
        }

        public static HB.Face ToHBFace(this RH.BrepFace surface, double maxRoofFloorAngle = 30)
        {

            var f = surface;
            var norm = f.NormalAt(0.5, 0.5);

            var id = Guid.NewGuid();
            var name = $"Face_{id}";
            var displayName = $"Face {id.ToString().Substring(0, 5)}";
            var face = new HB.Face(
                    name,
                    f.ToHBFace3D(),
                    HB.Face.FaceTypeEnum.Wall,
                    new HB.Outdoors(),
                    new HB.FacePropertiesAbridged()
                    );
            face.DisplayName = displayName;

            var isGround = RH.AreaMassProperties.Compute(surface).Centroid.Z <= 0;
            if (isGround)
            {
                face.BoundaryCondition = new HB.Ground();
            }

            var maxRoofFloorAngleRad = maxRoofFloorAngle * Math.PI / 180;
            if (CalAngle(norm, floorBaseNorm) <= maxRoofFloorAngleRad)
            {
                face.FaceType = HB.Face.FaceTypeEnum.Floor;
            }
            else if (CalAngle(norm, roofBaseNorm) <= maxRoofFloorAngleRad)
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
        //public static List<HB.Face> ToHBFaces(this RH.Brep brep, HB.Face.FaceTypeEnum faceType, HB.AnyOf<HB.Ground, HB.Outdoors, HB.Adiabatic, HB.Surface> boundaryCondition)
        //{

        //    var faces = new List<HB.Face>();

        //    return brep.ToHBFace3Ds()
        //        .Select(_ => new HB.Face(
        //            $"{faceType}_{Guid.NewGuid()}",
        //            _,
        //            faceType,
        //            boundaryCondition,
        //            new HB.FacePropertiesAbridged()
        //            )
        //        ).ToList();
        //}


        public static HB.Aperture ToAperture(this RH.BrepFace singleSurface, Guid hostID)
        {
            if (singleSurface.IsPlanar())
            {
                var face3D = singleSurface.ToHBFace3D();
                var apt = new HB.Aperture($"Aperture_{hostID}", face3D, new HB.Outdoors(), new HB.AperturePropertiesAbridged());
                apt.DisplayName = $"My Aperture {hostID.ToString().Substring(0, 5)}";
                return apt;
            }
            else
            {
                throw new ArgumentException("Input surface has to be planar!");
            }
        }

        //public static HB.Aperture ToWindow(this RH.Surface singleSurface) => ToAperture(singleSurface);

        public static HB.Door ToDoor(this RH.BrepFace singleSurface, Guid hostID)
        {
            if (singleSurface.IsPlanar())
            {
                var face3D = singleSurface.ToHBFace3D();
                var apt = new HB.Door($"Door_{hostID}", face3D, new HB.Outdoors(), new HB.DoorPropertiesAbridged());
                apt.DisplayName = $"My Door {hostID.ToString().Substring(0, 5)}";
                return apt;
            }
            else
            {
                throw new ArgumentException("Input surface has to be planar!");
            }
        }

        public static HB.Shade ToShade(this RH.BrepFace singleSurface, Guid hostID)
        {
            if (singleSurface.IsPlanar())
            {
                var face3D = singleSurface.ToHBFace3D();
                var obj = new HB.Shade($"Shade_{hostID}", face3D, new HB.ShadePropertiesAbridged());
                obj.DisplayName = $"My Shade {hostID.ToString().Substring(0, 5)}";
                return obj;
            }
            else
            {
                throw new ArgumentException("Input surface has to be planar!");
            }
        }

    }


}
