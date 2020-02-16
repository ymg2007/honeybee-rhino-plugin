using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino.Test.Tests
{
    [TestFixture]
    public class Tests_HBModelTests
    {
        [Test]
        public void Test_CreateModel()
        {
            //var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            //var box = new Box(bbox);
            //var room = box.ToRoom(maxRoofFloorAngle: 30);

            //var model = new HoneybeeSchema.Model(
            //    "modelName",
            //    new HoneybeeSchema.ModelProperties(energy: HoneybeeSchema.ModelEnergyProperties.Default),
            //    "a new displace name"
            //    );
            //model.Rooms = new List<HoneybeeSchema.Room>();
            //model.Rooms.Add(room);

            //var json = model.ToJson();

            ////TestContext.WriteLine(room.ToJson());
            //Assert.AreEqual(room.Faces.Count, 6);
        }

    }
}
