using HoneybeeRhino.Entities;
using HoneybeeSchema;
using NUnit.Framework;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino.Test.Tests
{
    [TestFixture]
    public class Tests_HBModelTests
    {
        RhinoDoc _doc = RhinoDoc.ActiveDoc;
        double _tol = 0.0001;

        [Test]
        public void Test_CreateModel()
        {
            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox).ToBrep();
            var hbFaces = box.Faces.Select(_ => _.ToHBFace(maxRoofFloorAngle: 30)).ToList();

            var room = new Room($"Room_{Guid.NewGuid()}", hbFaces, new RoomPropertiesAbridged(energy: new RoomEnergyPropertiesAbridged()));

            var modelProp = new ModelProperties(energy: ModelEnergyProperties.Default);
            //var stone = new EnergyMaterial(
            //    name: "Thick Stone",
            //    thickness: 0.3,
            //    conductivity: 2.31,
            //    density: 2322,
            //    specificHeat: 832,
            //    roughness: EnergyMaterial.RoughnessEnum.Rough,
            //    thermalAbsorptance: 0.95,
            //    solarAbsorptance: 0.75,
            //    visibleAbsorptance: 0.8
            //    );

            //modelProp.Energy.Materials.Add(stone);

            //var thermalMassConstr = new OpaqueConstructionAbridged("Thermal Mass Floor", new List<string>() { stone.Name });
            //modelProp.Energy.Constructions.Add(thermalMassConstr);

            //var faceEnergyProp = new FaceEnergyPropertiesAbridged();
            //faceEnergyProp.Construction = thermalMassConstr.Name;
            //room.Faces[0].Properties.Energy = faceEnergyProp;


            var model = new Model(
                "modelName",
                modelProp,
                "a new displace name"
                );
            model.Rooms = new List<HoneybeeSchema.Room>();
            model.Rooms.Add(room);


            var json = model.ToJson();

            var modelPath = @"D:\Dev\test\HB\model.json";
            File.WriteAllText(modelPath, json);


            //var studyFolder = @"D:\Dev\test\HB";
            //var simuParPath = @"D:\Dev\test\HB\simPar.json";
            //Runner.RunOpenStudio(studyFolder, modelPath, simuParPath);

            //TestContext.WriteLine(room.ToJson());
            Assert.AreEqual(room.Faces.Count, 6);
        }

        [Test]
        public void Test_TestSampleModel()
        {
            //var sampleModel = @"D:\Dev\Test\HB\model_energy_shoe_box.json";

            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox).ToBrep();
            var hbFaces = box.Faces.Select(_ => _.ToHBFace(maxRoofFloorAngle: 30)).ToList();

            var room = new Room($"Room_{Guid.NewGuid()}", hbFaces, new RoomPropertiesAbridged(energy: new RoomEnergyPropertiesAbridged()));

            var modelProp = new ModelProperties(energy: ModelEnergyProperties.Default);
            var stone = new EnergyMaterial(
                name: "Thick Stone",
                thickness: 0.3,
                conductivity: 2.31,
                density: 2322,
                specificHeat: 832,
                roughness: EnergyMaterial.RoughnessEnum.Rough,
                thermalAbsorptance: 0.95,
                solarAbsorptance: 0.75,
                visibleAbsorptance: 0.8
                );

            modelProp.Energy.Materials.Add(stone);

            var thermalMassConstr = new OpaqueConstructionAbridged("Thermal Mass Floor", new List<string>() { stone.Name });
            modelProp.Energy.Constructions.Add(thermalMassConstr);

            var faceEnergyProp = new FaceEnergyPropertiesAbridged();
            faceEnergyProp.Construction = thermalMassConstr.Name;
            room.Faces[0].Properties.Energy = faceEnergyProp;


            var model = new Model(
                "modelName",
                modelProp,
                "a new displace name"
                );
            model.Rooms = new List<HoneybeeSchema.Room>();
            model.Rooms.Add(room);


            var json = model.ToJson();

            var modelPath = @"D:\Dev\test\HB\model.json";
            File.WriteAllText(modelPath, json);


            var studyFolder = @"D:\Dev\test\HB";
            var simuParPath = @"D:\Dev\test\HB\simPar.json";
            Runner.RunOpenStudio(studyFolder, modelPath, simuParPath);

            //TestContext.WriteLine(room.ToJson());
            Assert.AreEqual(room.Faces.Count, 6);
        }

    }
}
