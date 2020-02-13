using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.DocObjects;

namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class RoomEntityTests
    {
        RhinoDoc _doc = RhinoDoc.ActiveDoc;
        double _tol = 0.0001;
        public RhinoObject InitRoomBox()
        {
            var allObjs = _doc.Objects;

            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var id = _doc.Objects.Add(box.ToBrep());
            var newHBGeo = box.ToBrep().ToRoomGeo(id);
            _doc.Objects.Replace(id, newHBGeo);
            var rhinoObj = _doc.Objects.FindId(id);

            return rhinoObj;
        }

        public List<Brep> InitTwoBoxes()
        {
            var bbox1 = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 8, 3));
            var b1 = new Box(bbox1).ToBrep();
            var bbox2 = new BoundingBox(new Point3d(0, 0, 0), new Point3d(4, -6, 3.5));
            var b2 = new Box(bbox2).ToBrep();

            return new List<Brep>() { b1, b2 };
        }
        public List<Brep> LoadBoxesFromJson(string file)
        {
            string json = System.IO.File.ReadAllText(file);
            var breps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Brep>>(json);
            return breps;
        }


        [Test]
        public void Test_BoxToRoom()
        {
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var room = box.ToBrep().ToRoomGeo(Guid.NewGuid());

            var ent = room.TryGetRoomEntity();
            var srfNames = room.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name);
            try
            {
                Assert.AreEqual(ent.HBObject.Faces.Count, 6);
                Assert.IsTrue(ent.HostGeoID != Guid.Empty);
                Assert.AreEqual(srfNames.Count(), 6);
            }
            catch (AssertionException e)
            {
                throw e;
            }
           
        }

        [Test]
        public void Test_RingFiveRooms()
        {
            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBoxesFromJson(file);
            //breps = _doc.Objects.Where(_ => _.IsSelected(true) >= 1).Select(_=>Brep.TryConvertBrep(_.Geometry)).ToList();
            var rooms = breps.Select(_=>_.ToRoomGeo(Guid.NewGuid()));

            var ents = rooms.Select(_=>_.TryGetRoomEntity());
            var srfNames = rooms.Select(rm=>rm.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name)).ToList();

            try
            {
                Assert.IsTrue(ents.All(_=>_.IsValid));
                Assert.IsTrue(ents.All(_=>_.HostGeoID != Guid.Empty));
                for (int i = 0; i < breps.Count(); i++)
                {
                    Assert.IsTrue(srfNames[i].Count() == breps[i].Faces.Count);
                }
            }
            catch (AssertionException e)
            {
                throw e;
            }

        }

        [Test]
        public void Test_MoveRoom()
        {
            var rhinoObj = InitRoomBox();
            var geo = rhinoObj.Geometry;

            var t = Transform.Translation(new Vector3d(10, 10, 0));
            geo.Transform(t);
            _doc.Objects.Replace(rhinoObj.Id, Brep.TryConvertBrep(geo));
            var newObj = _doc.Objects.FindId(rhinoObj.Id);
            var ent = newObj.TryGetRoomEntity();

            try
            {
                Assert.IsTrue(ent.IsValid);
                Assert.AreEqual(ent.HBObject.Faces.Count, 6);
                Assert.IsTrue(ent.HostGeoID == rhinoObj.Id);
            }
            catch (AssertionException e)
            {
                throw e;
            }
            
            _doc.Views.Redraw();
            _doc.Objects.Purge(newObj);
            _doc.Objects.Purge(rhinoObj);
            _doc.Views.Redraw();
        }

        [Test]
        public void Test_CopyRoomEntity()
        {
            var rhinoObj = InitRoomBox();
       

            var dupGeo = rhinoObj.DuplicateGeometry();
            var id = _doc.Objects.Add(dupGeo);
            var newObj = _doc.Objects.FindId(id);

            var ent = dupGeo.TryGetRoomEntity();
            ent.UpdateHostFrom(newObj);

            try
            {
                Assert.AreEqual(ent.HBObject.Faces.Count, 6);
                Assert.IsTrue(ent.HostGeoID == id);
            }
            catch (AssertionException e)
            {
                throw e;
            }

            _doc.Objects.Purge(rhinoObj);
            _doc.Objects.Purge(newObj);

        }

        [Test]
        public void Test_Coplanar()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
            var breps = LoadBoxesFromJson(file);

            var firstB = breps[0];
            var secondB = breps[1];

            var cutters = firstB.GetAdjFaces(secondB, _tol);


            try
            {
                Assert.AreEqual(cutters.Count(), 1);

                var cutterProp = AreaMassProperties.Compute(cutters.First().matchedCutters.First());
                Assert.IsTrue(Math.Abs(cutterProp.Area - 4 * 3.5) < _tol);
                Assert.IsTrue(cutterProp.Centroid.DistanceToSquared(new Point3d(2, 0, 1.75)) < Math.Pow(_tol, 2));
            }
            catch (AssertionException e)
            {
                throw e;
            }


        }

        [Test]
        public void Test_IntersectMass_TwoSimpleBreps()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
            var breps = LoadBoxesFromJson(file);

            var rooms = breps.Select(_ => _.ToRoomGeo(Guid.NewGuid()));
            var intersectedBreps = rooms.IntersectMasses(_tol).ToList();

            var firstB = intersectedBreps[0];
            var secondB = intersectedBreps[1];

            //check if there is a new face created
            try
            {
                Assert.IsTrue(firstB.IsSolid);
                Assert.AreEqual(firstB.Faces.Count, 7);
                Assert.IsTrue(secondB.IsSolid);
                Assert.AreEqual(secondB.Faces.Count, 7);
            }
            catch (AssertionException e)
            {
                throw e;
            }
           

            //Check if all face names are identical
            var hbObjs_first = firstB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);
            var hbObjs_second = secondB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);

            var faceNames_first = hbObjs_first.Select(_ => _.Name);
            var faceNames_second = hbObjs_second.Select(_ => _.Name);

            try
            {
                Assert.IsTrue(faceNames_first.Count() == faceNames_first.Distinct().Count());
                Assert.IsTrue(faceNames_second.Count() == faceNames_second.Distinct().Count());
            }
            catch (AssertionException e)
            {
                throw e;
            }
           

            //check adjacent face property: match center point
            var indoorFace_first = firstB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);
            var indoorFace_second = secondB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);

            var center_first = AreaMassProperties.Compute(indoorFace_first).Centroid;
            var center_second = AreaMassProperties.Compute(indoorFace_second).Centroid;

            try
            {
                Assert.IsTrue(center_first.DistanceToSquared(center_second) < Math.Pow(_tol, 2));
            }
            catch (AssertionException e)
            {
                throw e;
            }
           

        }

        [Test]
        public void Test_DuplicateBreps_WithUserData()
        {
            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBoxesFromJson(file);
            var rooms = breps.Select(_ => _.ToRoomGeo(Guid.NewGuid()));
            var isRoom = rooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

            var intersectedBreps = rooms.Select(_ => _.DuplicateBrep());
            var isNewRoomSurfacesOK = rooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

            try
            {
                Assert.IsTrue(isNewRoomSurfacesOK.All(_ => _));
            }
            catch (AssertionException e)
            {
                throw e;
            }
        }

        [Test]
        public void Test_IntersectMass_MoreBreps()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBoxesFromJson(file);
            var rooms = breps.Select(_ => _.ToRoomGeo(Guid.NewGuid()));

            var intersectedBreps = rooms.Select(_ => _.DuplicateBrep());
            intersectedBreps = rooms.IntersectMasses(_tol);

            //foreach (var item in intersectedBreps)
            //{
            //    if (item.Faces.Count !=8)
            //    {
            //        _doc.Objects.Add(item);
            //    }

            //}
            //_doc.Views.Redraw();

            //check if there is a new face created
            try
            {
                Assert.IsTrue(intersectedBreps.All(_ => _.IsSolid));
                Assert.IsTrue(intersectedBreps.All(_ => _.Faces.Count == 8));
                Assert.IsTrue(intersectedBreps.All(_ => _.IsRoom()));
            }
            catch (AssertionException e)
            {
                throw e;
            }


            ////Check if all face names are identical
            var names = intersectedBreps.Select(b => b.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name));
            var diffNames = names.Distinct();
            try
            {
                Assert.IsTrue(names.Sum(_=>_.Count()) == diffNames.Sum(_ => _.Count()));
            }
            catch (AssertionException e)
            {
                throw e;
            }
            //var hbObjs_second = secondB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);

            //var faceNames_first = hbObjs_first.Select(_ => _.Name).Distinct();
            //var faceNames_second = hbObjs_second.Select(_ => _.Name).Distinct();

            //Assert.IsTrue(faceNames_first.Count() == 7);
            //Assert.IsTrue(faceNames_second.Count() == 7);

            ////check adjacent face property: match center point
            //var indoorFace_first = firstB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);
            //var indoorFace_second = secondB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);

            //var center_first = AreaMassProperties.Compute(indoorFace_first).Centroid;
            //var center_second = AreaMassProperties.Compute(indoorFace_second).Centroid;
            //Assert.IsTrue(center_first.DistanceToSquared(center_second) < Math.Pow(_tol, 2));
            //throw new Exception("dddd");

        }

    }
}
