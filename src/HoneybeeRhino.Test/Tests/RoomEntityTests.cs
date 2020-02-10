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
        public RhinoObject InitRoomBox()
        {
            var allObjs = _doc.Objects;
            allObjs.ToList().ForEach(_ => allObjs.Delete(_));
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var id = _doc.Objects.Add(box.ToBrep());
            var rhinoObj = _doc.Objects.FindId(id);
            return rhinoObj;
        }

        [Test]
        public void Test_InitRoomEntity()
        {
            var rhinoObj = InitRoomBox();
            var geo = rhinoObj.Geometry;
            geo.ToRoomGeo(rhinoObj.Id);

            var t = Transform.Translation(new Vector3d(30, 40, 50));
            geo.Transform(t);
            _doc.Objects.Replace(rhinoObj.Id, Brep.TryConvertBrep(geo));
            var ent = rhinoObj.TryGetRoomEntity();

            Assert.IsTrue(ent.IsValid);
            Assert.AreEqual(ent.HBObject.Faces.Count, 6);
            Assert.IsTrue(ent.HostGeoID == rhinoObj.Id);
            _doc.Views.Redraw();
        }

        [Test]
        public void Test_CopyRoomEntity()
        {
            var rhinoObj = InitRoomBox();
            var geo = rhinoObj.Geometry;
            geo.ToRoomGeo(rhinoObj.Id);

            var dupGeo = rhinoObj.DuplicateGeometry();
            var id = _doc.Objects.Add(dupGeo);
            var newObj = _doc.Objects.FindId(id);

            var ent = dupGeo.TryGetRoomEntity();
            ent.UpdateHostFrom(newObj);

            Assert.AreEqual(ent.HBObject.Faces.Count, 6);
            Assert.IsTrue(ent.HostGeoID == id);
            RhinoDoc.ActiveDoc.Views.Redraw();
        }
    }
}
