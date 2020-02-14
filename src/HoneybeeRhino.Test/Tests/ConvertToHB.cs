using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using System.IO;

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

            var face3D = srf.ToHBFace3D();
            var boudary = face3D.Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 2, 0});
        }

        [Test]
        public void Test_Surface()
        {
            var p = Plane.WorldXY;
            var srf = new PlaneSurface(p, new Interval(0, 1), new Interval(0, 2)).ToBrep().Surfaces.First();

            var face3D = srf.ToHBFace3D();
            var boudary = face3D.Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 2, 0 });
        }

        [Test]
        public void Test_Brep()
        {
            BoundingBox box = new BoundingBox(new Point3d(0, 0, 0), new Point3d(1, 1, 1));
            Brep brep = box.ToBrep();
            var face3D = brep.ToHBFace3Ds();
            var boudary = face3D.First().Boundary;

            TestContext.WriteLine(string.Join(",", boudary[2]));
            Assert.AreEqual(boudary[2], new List<decimal> { 1, 0, 1 });
        }

        [Test]
        public void Test_AllPlaneBrep()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            string json = System.IO.File.ReadAllText(file);
            var breps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Brep>>(json);
            var srfTypes = breps.Select(_ => _.Surfaces.Select(s => s.GetType()));

            var newBreps = breps.Select(_ => _.ToAllPlaneBrep());
            var newSrfTypes = newBreps.Select(_ => _.Surfaces.All(s => s.GetType() == typeof(PlaneSurface)));
            try
            {
                Assert.IsTrue(newBreps.Count() == breps.Count);
                Assert.IsTrue(newBreps.All(_ => _.IsSolid));
                Assert.IsTrue(newSrfTypes.All(_=>_ == true));
            }
            catch (AssertionException e)
            {
                throw e;
            }


        }

       
        

    }
}
