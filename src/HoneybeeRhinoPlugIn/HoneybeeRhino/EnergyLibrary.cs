﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HB = HoneybeeSchema;

namespace HoneybeeRhino
{
    public static class EnergyLibrary
    {
        private const string _defaultConstructionSetUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/construction_set/constructionset_complete.json";
        private const string _defaultProgramTypesUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/program_type/program_type_office.json";
        private const string _defaultHVACUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/hvac/ideal_air_default.json";

        private static (string Url, string FilePath)[] _defaultLibraryFiles
            = new (string Url, string FilePath)[3] {
                (_defaultConstructionSetUrl, "defaultConstructionSets.json"),
                (_defaultProgramTypesUrl, "defaultProgramTypes.json"),
                (_defaultHVACUrl, "defaultHVACs.json" )
            };

        public static string HoneybeeStandardFolder { get; } = Path.Combine(Path.GetTempPath(), "Ladybug", "Library");
        private static string[] _LoadLibraries = new string[7]
        {
            Path.Combine(HoneybeeStandardFolder,"defaultPeopleLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultLightingLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultElectricEquipmentLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultGasEquipmentLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultInfiltrationLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultVentilationLoads.json"),
            Path.Combine(HoneybeeStandardFolder,"defaultSetpoints.json") 
        };

        

        public static string HoneybeeRootFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "honeybee");
        public static string StandardsFolder { get; } = Path.Combine(HoneybeeRootFolder, "honeybee_standards", "data");

        //honeybee_energy_standards
        public static string EnergyStandardsFolder { get; } = Path.Combine(HoneybeeRootFolder, "honeybee_energy_standards", "data");
        public static string BuildingVintagesFolder { get; } = Path.Combine(EnergyStandardsFolder, "programtypes_registry");
        public static string BuildingProgramTypesFolder { get; } = Path.Combine(EnergyStandardsFolder, "programtypes");
        public static string ConstructionsFolder { get; } = Path.Combine(EnergyStandardsFolder, "constructions");
        public static string ConstructionSetFolder { get; } = Path.Combine(EnergyStandardsFolder, "constructionsets");
        public static string ScheduleFolder { get; } = Path.Combine(EnergyStandardsFolder, "schedules");




        //BuildingVintages 2004, 2007, 2010, 2013, etc..
        private static IEnumerable<string> _buildingVintages;
        public static IEnumerable<string> BuildingVintages => _buildingVintages ?? GetBuildingVintages();


        //ConstructionSets
        private static IEnumerable<HB.ConstructionSetAbridged> _defaultConstructionSets;
        public static IEnumerable<HB.ConstructionSetAbridged> DefaultConstructionSets => _defaultConstructionSets ?? 
            LoadLibrary(_defaultLibraryFiles[0].FilePath, HB.ConstructionSetAbridged.FromJson);

        private static Dictionary<string, IEnumerable<HB.ConstructionSetAbridged>> _standardsConstructionSets = new Dictionary<string, IEnumerable<HB.ConstructionSetAbridged>>();
        public static Dictionary<string, IEnumerable<HB.ConstructionSetAbridged>> StandardsConstructionSets => throw new ArgumentException("Use GetorLoadStandardsConstructionSets(jsonFile)");



        //Constructions  load from honeybee\honeybee_energy_standards\data\constructions\window_construction.json
        private static IEnumerable<HB.WindowConstructionAbridged> _standardsWindowConstructions;
        public static IEnumerable<HB.WindowConstructionAbridged> StandardsWindowConstructions => _standardsWindowConstructions ??
            LoadLibrary(Path.Combine(ConstructionsFolder, "window_construction.json"), HB.WindowConstructionAbridged.FromJson);

        //                  load from honeybee\honeybee_energy_standards\data\constructions\opaque_construction.json
        private static IEnumerable<HB.OpaqueConstructionAbridged> _standardsOpaqueConstructions;
        public static IEnumerable<HB.OpaqueConstructionAbridged> StandardsOpaqueConstructions => _standardsOpaqueConstructions ??
            LoadLibrary(Path.Combine(ConstructionsFolder, "opaque_construction.json"), HB.OpaqueConstructionAbridged.FromJson);



        //Window Materials load from honeybee\honeybee_energy_standards\data\constructions\window_material.json
        private static IEnumerable<HB.IEnergyWindowMaterial> _standardsWindowMaterials;
        public static IEnumerable<HB.IEnergyWindowMaterial> StandardsWindowMaterials => _standardsWindowMaterials ??
            LoadWindowMaterials(Path.Combine(ConstructionsFolder, "window_material.json"));

        //                 load from honeybee\honeybee_energy_standards\data\constructions\opaque_material.json
        private static IEnumerable<HB.IEnergyMaterial> _standardsOpaqueMaterials;
        public static IEnumerable<HB.IEnergyMaterial> StandardsOpaqueMaterials => _standardsOpaqueMaterials ??
            LoadOpqueMaterials(Path.Combine(ConstructionsFolder, "opaque_material.json"));



        //ProgramTypes
        private static IEnumerable<HB.ProgramTypeAbridged> _defaultProgramTypes;
        public static IEnumerable<HB.ProgramTypeAbridged> DefaultProgramTypes 
            => _defaultProgramTypes ?? 
            LoadLibrary(_defaultLibraryFiles[1].FilePath, HB.ProgramTypeAbridged.FromJson);

        private static Dictionary<string, IEnumerable<HB.ProgramTypeAbridged>> _standardsProgramTypesByVintage = new Dictionary<string, IEnumerable<HB.ProgramTypeAbridged>>();
        public static Dictionary<string, IEnumerable<HB.ProgramTypeAbridged>> StandardsProgramTypesByVintage => throw new ArgumentException("Use GetOrLoadProgramTypesFromJson(jsonFile)");



        //Schedules
        private static IEnumerable<HB.ScheduleRulesetAbridged> _standardsSchedules;
        public static IEnumerable<HB.ScheduleRulesetAbridged> StandardsSchedules => _standardsSchedules ??
            LoadLibraryParallel(Path.Combine(ScheduleFolder, "schedule.json"), HB.ScheduleRulesetAbridged.FromJson);



        //HVACs
        private static IEnumerable<HB.IdealAirSystemAbridged> _defaultHVACs;
        public static IEnumerable<HB.IdealAirSystemAbridged> DefaultHVACs => _defaultHVACs ?? 
            LoadLibrary(_defaultLibraryFiles[2].FilePath, HB.IdealAirSystemAbridged.FromJson);

        //People load
        private static IEnumerable<HB.PeopleAbridged> _defaultPeopleLoads;
        public static IEnumerable<HB.PeopleAbridged> DefaultPeopleLoads => _defaultPeopleLoads ?? 
            LoadLibrary(_LoadLibraries[0], HB.PeopleAbridged.FromJson);

        //Lighting load
        private static IEnumerable<HB.LightingAbridged> _defaultLightingLoads;
        public static IEnumerable<HB.LightingAbridged> DefaultLightingLoads => _defaultLightingLoads ?? 
            LoadLibrary(_LoadLibraries[1], HB.LightingAbridged.FromJson);

        //ElecEqp load
        private static IEnumerable<HB.ElectricEquipmentAbridged> _defaultElectricEquipmentLoads;
        public static IEnumerable<HB.ElectricEquipmentAbridged> DefaultElectricEquipmentLoads => _defaultElectricEquipmentLoads ?? 
            LoadLibrary(_LoadLibraries[2], HB.ElectricEquipmentAbridged.FromJson);

        //GasEqp load
        private static IEnumerable<HB.GasEquipmentAbridged> _defaultGasEquipmentLoads;
        public static IEnumerable<HB.GasEquipmentAbridged> GasEquipmentLoads => _defaultGasEquipmentLoads ?? 
            LoadLibrary(_LoadLibraries[3], HB.GasEquipmentAbridged.FromJson);

        //GasEqp load
        private static IEnumerable<HB.InfiltrationAbridged> _defaultInfiltrationLoads;
        public static IEnumerable<HB.InfiltrationAbridged> DefaultInfiltrationLoads => _defaultInfiltrationLoads ?? 
            LoadLibrary(_LoadLibraries[4], HB.InfiltrationAbridged.FromJson);

        //Ventilation load
        private static IEnumerable<HB.VentilationAbridged> _defaultVentilationLoads;
        public static IEnumerable<HB.VentilationAbridged> DefaultVentilationLoads => _defaultVentilationLoads ?? 
            LoadLibrary(_LoadLibraries[5], HB.VentilationAbridged.FromJson);

        //Setpoints
        private static IEnumerable<HB.SetpointAbridged> _defaultSetpoints;
        public static IEnumerable<HB.SetpointAbridged> DefaultSetpoints => _defaultSetpoints ?? 
            LoadLibrary(_LoadLibraries[6], HB.SetpointAbridged.FromJson);

        public static string DownLoadLibrary(string standardsUrl, string saveAsfileName)
        {
            var url = standardsUrl;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                Directory.CreateDirectory(HoneybeeStandardFolder);
                var file = Path.Combine(HoneybeeStandardFolder, saveAsfileName);
                wc.DownloadFile(url, file);
                return file;
            }
        }

        public static List<string> CheckAllDefaultLibraries()
        {
            var libs = new List<string>();
            foreach (var item in _defaultLibraryFiles)
            {
                var jsonFile = Path.Combine(HoneybeeStandardFolder, item.FilePath);
                if (!File.Exists(jsonFile))
                    DownLoadLibrary(item.Url, item.FilePath);
                libs.Add(jsonFile);
            }

            return libs;
        }

        public static IEnumerable<T> LoadLibrary<T>(string jsonFile, Func<string,T> func)
        {
            //TODO: remove this later for real deployment
            //Check or download all json libraries.
            //CheckAllDefaultLibraries();

            //var jsonFile = Path.Combine(HoneybeeStandardFolder, jsonFilename);

            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            using (var file = File.OpenText(jsonFile))
            using(var reader = new JsonTextReader(file))
            {
                var jObjs = JToken.ReadFrom(reader);
                var libItems = jObjs.Values();

                var hbObjs = libItems.Select(_ => func(_.ToString()));
                return hbObjs;
            }
               
        }

        public static IEnumerable<T> LoadLibraryParallel<T>(string jsonFile, Func<string, T> func)
        {

            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            using (var file = File.OpenText(jsonFile))
            using (var reader = new JsonTextReader(file))
            {
                var jObjs = JToken.ReadFrom(reader);
                var libItems = jObjs.Values();

                var hbObjs = libItems.AsParallel().Select(_ => func(_.ToString()));
                return hbObjs;
            }

        }

        public static IEnumerable<string> GetBuildingVintages() => Directory.GetFiles(BuildingVintagesFolder, "*.json");


        public static Dictionary<string, IEnumerable<string>> LoadBuildingVintage(string buildingVintageFile)
        {
            var vintageJson = Path.Combine(BuildingVintagesFolder, buildingVintageFile);

            if (!File.Exists(vintageJson))
                throw new ArgumentException($"{vintageJson} doesn't exist");

            var vintageDic = new Dictionary<string, IEnumerable<string>>();
            using (var file = File.OpenText(vintageJson))
            using (var reader = new JsonTextReader(file))
            {
                var jObjs = JObject.Load(reader);

                var buildingTypes = jObjs.Children<JProperty>();
                foreach (var item in buildingTypes)
                {
                    var name = item.Name;
                    var spaceTypes = item.Value.Select(_ => _.ToString());
                    vintageDic.Add(name, spaceTypes);

                }

            }

            return vintageDic;


        }

  
        public static IEnumerable<HB.ConstructionSetAbridged> GetOrLoadStandardsConstructionSets(string jsonFile)
        {
            var jsonFilePath = jsonFile;

            var fileName = Path.GetFileName(jsonFilePath);
            IEnumerable<HB.ConstructionSetAbridged> constructionSets;

            //Check if this is loaded previously
            var loadedBefore = _standardsConstructionSets.TryGetValue(fileName, out constructionSets);
            if (loadedBefore)
                return constructionSets;

            //Load from Json 
            constructionSets = LoadLibrary(jsonFilePath, HB.ConstructionSetAbridged.FromJson);
            return constructionSets;

        }
        public static IEnumerable<HB.ProgramTypeAbridged> GetOrLoadProgramTypesFromJson(string jsonFile)
        {
            var jsonFilePath = jsonFile;

            var fileName = Path.GetFileName(jsonFilePath);
            IEnumerable<HB.ProgramTypeAbridged> programTypes;

            //Check if this is loaded previously
            var loadedBefore = _standardsProgramTypesByVintage.TryGetValue(fileName, out programTypes);
            if (loadedBefore)
                return programTypes;

            //Load from Json 
            programTypes = LoadLibrary(jsonFilePath, HB.ProgramTypeAbridged.FromJson);
            return programTypes;

        }

        public static List<HB.IEnergyWindowMaterial> LoadWindowMaterials(string windowMaterialJsonFile)
        {
            var jsonFile = windowMaterialJsonFile;
            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            var materials = new List<HB.IEnergyWindowMaterial>();


            using (var file = File.OpenText(jsonFile))
            using (var reader = new JsonTextReader(file))
            {
                var jObjs = JToken.ReadFrom(reader);
                var libItems = jObjs.Values();
                foreach (var item in libItems)
                {
                    var typeName = item.Value<string>("type");
                    switch (typeName)
                    {
                        case nameof(HB.EnergyWindowMaterialBlind):
                            materials.Add(HB.EnergyWindowMaterialBlind.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialGas):
                            materials.Add(HB.EnergyWindowMaterialGas.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialGasCustom):
                            materials.Add(HB.EnergyWindowMaterialGasCustom.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialGasMixture):
                            materials.Add(HB.EnergyWindowMaterialGasMixture.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialGlazing):
                            materials.Add(HB.EnergyWindowMaterialGlazing.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialShade):
                            materials.Add(HB.EnergyWindowMaterialShade.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyWindowMaterialSimpleGlazSys):
                            materials.Add(HB.EnergyWindowMaterialSimpleGlazSys.FromJson(item.ToString()));
                            break;
                        default:
                            //do nothing
                            break;
                    }
                }

                return materials;
            }
        }

        public static List<HB.IEnergyMaterial> LoadOpqueMaterials(string opaqueMaterialJsonFile)
        {
            var jsonFile = opaqueMaterialJsonFile;
            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            var materials = new List<HB.IEnergyMaterial>();

            using (var file = File.OpenText(jsonFile))
            using (var reader = new JsonTextReader(file))
            {
                var jObjs = JToken.ReadFrom(reader);
                var libItems = jObjs.Values();
                foreach (var item in libItems)
                {
                    var typeName = item.Value<string>("type");
                    switch (typeName)
                    {
                        case nameof(HB.EnergyMaterial):
                            materials.Add(HB.EnergyMaterial.FromJson(item.ToString()));
                            break;
                        case nameof(HB.EnergyMaterialNoMass):
                            materials.Add(HB.EnergyMaterialNoMass.FromJson(item.ToString()));
                            break;
                        default:
                            //do nothing
                            break;
                    }
                }

                return materials;
            }
        }



    }
}