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
    }
}
