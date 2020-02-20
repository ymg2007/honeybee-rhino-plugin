using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoneybeeSchema;

namespace HoneybeeRhino.Test.Tests
{
    [TestFixture]
    public class Tests_EnergyLibrary
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        [Test]
        public void Test_GetBuildingVintages()
        {
            sw.Restart();
            var vintages = EnergyLibrary.GetBuildingVintages();
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            Assert.IsTrue(vintages.Any(_=>_.EndsWith("2013_registry.json")));
        }

        [Test]
        public void Test_LoadBuildingVintage()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_LoadBuildingVintage)}");

            sw.Restart();
            var vintages = EnergyLibrary.GetBuildingVintages();
            var vintage = vintages.FirstOrDefault(_ => _.EndsWith("2013_registry.json"));
            var dics = EnergyLibrary.LoadBuildingVintage(vintage);

            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");
            Assert.IsTrue(dics["LargeOffice"].Contains("OpenOffice"));
        }

        [Test]
        public void Test_GetOrLoadProgramTypesFromJson()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_GetOrLoadProgramTypesFromJson)}");
            
            sw.Restart();
            var programTypes = EnergyLibrary.GetOrLoadProgramTypesFromJson(Path.Combine(EnergyLibrary.BuildingProgramTypesFolder, "2013_data.json"));
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var programType = programTypes.Where(_ => _.Name == "2013::Courthouse::Courtroom").FirstOrDefault();

            Assert.IsTrue(programType.Ventilation.FlowPerPerson == 0.00471947);
        }

        [Test]
        public void Test_GetOrLoadStandardsConstructionSets()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_GetOrLoadStandardsConstructionSets)}");

            sw.Restart();
            var programTypes = EnergyLibrary.GetOrLoadStandardsConstructionSets(Path.Combine(EnergyLibrary.ConstructionSetFolder, "2013_data.json"));
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var programType = programTypes.Where(_ => _.Name == "2013::ClimateZone1::SteelFramed").FirstOrDefault();

            Assert.IsTrue(programType.ApertureSet.WindowConstruction == "U 0.60 SHGC 0.25 Dbl 2.5mm air");
        }

        [Test]
        public void Test_StandardsOpaqueMaterial()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_StandardsOpaqueMaterial)}");

            sw.Restart();
            var objs = EnergyLibrary.StandardsOpaqueMaterials;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var found = objs.FirstOrDefault(_ => _.Name == "1/2 in. Gypsum Board") as EnergyMaterial;
            Assert.IsTrue(found.Conductivity == 0.15989299909405463);
        }

        [Test]
        public void Test_StandardsWindowMaterials()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_StandardsWindowMaterials)}");

            sw.Restart();
            var objs = EnergyLibrary.StandardsWindowMaterials;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var found = objs.FirstOrDefault(_ => _.Name == "BLUE 6MM") as EnergyWindowMaterialGlazing;
            Assert.IsTrue(found.Thickness == 0.005999999999999977);
        }


        [Test]
        public void Test_StandardsWindowConstructions()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_StandardsWindowConstructions)}");

            sw.Restart();
            var objs = EnergyLibrary.StandardsWindowConstructions;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var found = objs.FirstOrDefault(_ => _.Name == "U 0.33 SHGC 0.40 Dbl LoE (e2-.1) Tint 6mm/13mm Air") as WindowConstructionAbridged;
            Assert.IsTrue(found.Layers.Count == 3);
        }
        [Test]
        public void Test_StandardsOpaqueConstructions()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_StandardsOpaqueConstructions)}");

            sw.Restart();
            var objs = EnergyLibrary.StandardsOpaqueConstructions;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var found = objs.FirstOrDefault(_ => _.Name == "Typical Insulated Wood Framed Exterior Wall-R6") as OpaqueConstructionAbridged;
            Assert.IsTrue(found.Layers.First() == "25mm Stucco");
        }

        [Test]
        public void Test_StandardsSchedules()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_StandardsSchedules)}");

            sw.Restart();
            var objs = EnergyLibrary.StandardsSchedules;
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            var found = objs.FirstOrDefault(_ => _.Name == "ApartmentHighRise APT_DHW_SCH") as ScheduleRulesetAbridged;
            Assert.IsTrue(found.DefaultDaySchedule == "ApartmentHighRise APT_DHW_SCH_Default");
        }


    }
}
