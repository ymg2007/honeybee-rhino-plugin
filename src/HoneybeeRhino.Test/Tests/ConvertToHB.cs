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
        public void Test_BoxToRoom()
        {
            
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var id = RhinoDoc.ActiveDoc.Objects.Add(box.ToBrep());
            var rhinoObj = RhinoDoc.ActiveDoc.Objects.FindId(id);
            var geo = rhinoObj.Geometry;
            geo.ToRoomGeo(rhinoObj.Id);

            var t = Transform.Translation(new Vector3d(30, 40, 50));
            geo.Transform(t);
            RhinoDoc.ActiveDoc.Objects.Replace(id, Brep.TryConvertBrep(geo));
            var ent = Entities.RoomEntity.TryGetFrom(rhinoObj.Geometry);
            //TestContext.WriteLine(ent.HBObject.ToJson());
            Assert.AreEqual(ent.HBObject.Faces.Count, 6);
            Assert.IsTrue(ent.HostGeoID != Guid.Empty);
            RhinoDoc.ActiveDoc.Views.Redraw();
        }

        [Test]
        public void Test_CreateModel()
        {
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var room = box.ToRoom(maxRoofFloorAngle: 30);

            var model = new HoneybeeSchema.Model(
                "modelName",
                new HoneybeeSchema.ModelProperties(energy: HoneybeeSchema.ModelEnergyProperties.Default),
                "a new displace name"
                );
            model.Rooms = new List<HoneybeeSchema.Room>();
            model.Rooms.Add(room);

            var json = model.ToJson();



            //TestContext.WriteLine(room.ToJson());
            Assert.AreEqual(room.Faces.Count, 6);
        }

        [Test]
        public void Test_Coplanar()
        {
            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
            string json = System.IO.File.ReadAllText(file);
            var breps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Brep>>(json);
            var tol = 0.0001;

            var firstB = breps[0];
            var secondB = breps[1];

            var cutters = new List<Brep>();
            foreach (var face in firstB.Faces)
            {
                var intersectedFaces = secondB.Faces.Where(_ => _.GetBoundingBox(false).isIntersected(face.GetBoundingBox(false)));
                var coplanarFaces = intersectedFaces.Where(_ => _.UnderlyingSurface().IsCoplanar(face.UnderlyingSurface(), tol, true)).Select(_ => _.ToBrep());
                cutters.AddRange(coplanarFaces);
            }

            Assert.AreEqual(cutters.Count, 1);

            var cutterProp = AreaMassProperties.Compute(cutters.First());
            Assert.IsTrue(Math.Abs(cutterProp.Area - 4*3.5)< tol);
            Assert.IsTrue(cutterProp.Centroid.DistanceToSquared(new Point3d(2, 0, 1.75))< Math.Pow(tol,2));

        }

    }
}
