using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class ConvertToHB
    {
        [Test]
        public void Test_PlanerSurface()
        {
            var p = Plane.WorldXY;
            var srf = new PlaneSurface(p, new Interval(0, 1), new Interval(0, 2));

            var face3D = srf.ToHB();
            var boudary = face3D.Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 2, 0});
        }

        [Test]
        public void Test_Surface()
        {
            var p = Plane.WorldXY;
            var srf = new PlaneSurface(p, new Interval(0, 1), new Interval(0, 2)).ToBrep().Surfaces.First();

            var face3D = srf.ToHB();
            var boudary = face3D.Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 2, 0 });
        }

        [Test]
        public void Test_Brep()
        {
            BoundingBox box = new BoundingBox(new Point3d(0, 0, 0), new Point3d(1, 1, 1));
            Brep brep = box.ToBrep();
            var face3D = brep.ToHB();
            var boudary = face3D.First().Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 0, 1 });
        }
    }
}
