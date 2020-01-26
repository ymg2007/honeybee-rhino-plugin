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

        public static List<decimal> ToDecimalList(this RH.Point3d point)
        {
            return (new List<decimal>() { 
                (decimal)point.X, 
                (decimal)point.Y, 
                (decimal)point.Z }
            );
        }

        public static HB.Face3D ToHB(this RH.PlaneSurface surface)
        {
            return (surface as RH.Surface).ToHB();

        }

        public static HB.Face3D ToHB(this RH.Surface surface)
        {
            return (surface.ToBrep().ToHB().First());
           
        }

        public static List<HB.Face3D> ToHB(this RH.Brep brep)
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
                var boundaryPts = pts.First() as List<List<decimal>>;
                var face3D = new HB.Face3D(boundaryPts);
                if (pts.Count > 1)
                {
                    face3D.Holes.AddRange(pts.Skip(1) as List<List<List<decimal>>>);
                }
                face3Ds.Add(face3D);
            }

            return face3Ds;
            
        }


    }

   
}
