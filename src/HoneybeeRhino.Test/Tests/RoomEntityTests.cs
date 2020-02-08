using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;

namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class RoomEntityTests
    {
        [Test]
        public void Test_InitRoomEntity()
        {
            //var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            //var box = new Box(bbox);

            ////Open or Create a new RhinoDOC
            //var doc = Rhino.RhinoDoc.Open(@"D:\Dev\test\Rhino\BoxWithJson.3dm", out bool wasAlreadyOpen);
            //var boxId = doc.Objects.AddBox(box);

            ////get RhinoObject
            //var rhinoObjext = doc.Objects.FindId(boxId);
           
            //Guid HostGeoID = Guid.Empty; //get real value from UserData
            ////Test if they match.
            //Assert.AreEqual(HostGeoID, rhinoObjext.Id);
        }
    }
}
