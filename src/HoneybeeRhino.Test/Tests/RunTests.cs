using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class RunnerTests
    {
        /// <summary>
        /// Transform a brep using a translation
        /// </summary>
        [Test]
        public void RunOpenStudio_Test()
        {
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            var room = box.ToRoom(maxRoofFloorAngle: 30);

            var model = new HoneybeeDotNet.Model.Model(
                "modelName",
                new HoneybeeDotNet.Model.ModelProperties(),
                "a new displace name"
                );
            model.Properties.Energy = HoneybeeDotNet.Model.ModelEnergyProperties.Default;
            model.Rooms = new List<HoneybeeDotNet.Model.Room>();
            model.Rooms.Add(room);

            var json = model.ToJson();
            var modelPath = @"D:\Dev\test\HB\model.json";
            File.WriteAllText(modelPath, json);


            var studyFolder = @"D:\Dev\test\HB";
            var simuParPath = @"D:\Dev\test\HB\simPar.json";
            Runner.RunOpenStudio(studyFolder, modelPath, simuParPath);
        }

        
    }
}
