using Newtonsoft.Json;
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
        private static string _defaultConstructionSetUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/construction_set/constructionset_complete.json";
        private static string _defaultProgramTypesUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/program_type/program_type_office.json";
        private static string _defaultHVACUrl = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-schema/master/samples/hvac/ideal_air_default.json";

        private static (string Url, string FilePath)[] _defaultLibraryFiles
            = new (string Url, string FilePath)[3] {
                (_defaultConstructionSetUrl, "defaultConstructionSets.json"),
                (_defaultProgramTypesUrl, "defaultProgramTypes.json"),
                (_defaultHVACUrl, "defaultHVACs.json" )
            };

        private static string[] _LoadLibraries = new string[7]
        {
            "defaultPeopleLoads.json",
            "defaultLightingLoads.json",
            "defaultElectricEquipmentLoads.json",
            "defaultGasEquipmentLoads.json",
            "defaultInfiltrationLoads.json",
            "defaultVentilationLoads.json",
            "defaultSetpoints.json"
        };

        public static string HoneybeeStandardFolder { get; } = Path.Combine(Path.GetTempPath(), "Ladybug", "Library");

        //ConstructionSets
        private static IEnumerable<HB.ConstructionSetAbridged> _defaultConstructionSets;
        public static IEnumerable<HB.ConstructionSetAbridged> DefaultConstructionSets
            => _defaultConstructionSets ?? LoadLibrary(_defaultLibraryFiles[0].FilePath, HB.ConstructionSetAbridged.FromJson);

        //ProgramTypes
        private static IEnumerable<HB.ProgramTypeAbridged> _defaultProgramTypes;
        public static IEnumerable<HB.ProgramTypeAbridged> DefaultProgramTypes 
            => _defaultProgramTypes ?? LoadLibrary(_defaultLibraryFiles[1].FilePath, HB.ProgramTypeAbridged.FromJson);

        //HVACs
        private static IEnumerable<HB.IdealAirSystemAbridged> _defaultHVACs;
        public static IEnumerable<HB.IdealAirSystemAbridged> DefaultHVACs
            => _defaultHVACs ?? LoadLibrary(_defaultLibraryFiles[2].FilePath, HB.IdealAirSystemAbridged.FromJson);

        //People load
        private static IEnumerable<HB.PeopleAbridged> _defaultPeopleLoads;
        public static IEnumerable<HB.PeopleAbridged> DefaultPeopleLoads
            => _defaultPeopleLoads ?? LoadLibrary(_LoadLibraries[0], HB.PeopleAbridged.FromJson);

        //Lighting load
        private static IEnumerable<HB.LightingAbridged> _defaultLightingLoads;
        public static IEnumerable<HB.LightingAbridged> DefaultLightingLoads
            => _defaultLightingLoads ?? LoadLibrary(_LoadLibraries[1], HB.LightingAbridged.FromJson);

        //ElecEqp load
        private static IEnumerable<HB.ElectricEquipmentAbridged> _defaultElectricEquipmentLoads;
        public static IEnumerable<HB.ElectricEquipmentAbridged> DefaultElectricEquipmentLoads
            => _defaultElectricEquipmentLoads ?? LoadLibrary(_LoadLibraries[2], HB.ElectricEquipmentAbridged.FromJson);

        //GasEqp load
        private static IEnumerable<HB.GasEquipmentAbridged> _defaultGasEquipmentLoads;
        public static IEnumerable<HB.GasEquipmentAbridged> GasEquipmentLoads
            => _defaultGasEquipmentLoads ?? LoadLibrary(_LoadLibraries[3], HB.GasEquipmentAbridged.FromJson);

        //GasEqp load
        private static IEnumerable<HB.InfiltrationAbridged> _defaultInfiltrationLoads;
        public static IEnumerable<HB.InfiltrationAbridged> DefaultInfiltrationLoads
            => _defaultInfiltrationLoads ?? LoadLibrary(_LoadLibraries[4], HB.InfiltrationAbridged.FromJson);

        //Ventilation load
        private static IEnumerable<HB.VentilationAbridged> _defaultVentilationLoads;
        public static IEnumerable<HB.VentilationAbridged> DefaultVentilationLoads
            => _defaultVentilationLoads ?? LoadLibrary(_LoadLibraries[5], HB.VentilationAbridged.FromJson);

        //Setpoints
        private static IEnumerable<HB.SetpointAbridged> _defaultSetpoints;
        public static IEnumerable<HB.SetpointAbridged> DefaultSetpoints
            => _defaultSetpoints ?? LoadLibrary(_LoadLibraries[6], HB.SetpointAbridged.FromJson);

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

        public static IEnumerable<T> LoadLibrary<T>(string jsonFilename, Func<string,T> func)
        {
            //TODO: remove this later for real deployment
            //Check or download all json libraries.
            CheckAllDefaultLibraries();

            var jsonFile = Path.Combine(HoneybeeStandardFolder, jsonFilename);

            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            using (var file = File.OpenText(jsonFile))
            using(var reader = new JsonTextReader(file))
            {
                var jObjs = (JObject)JToken.ReadFrom(reader);
                var libItems = jObjs.Values();

                var hbObjs = libItems.Select(_ => func(_.ToString()));
                return hbObjs;
            }
               
        }

    }
}
