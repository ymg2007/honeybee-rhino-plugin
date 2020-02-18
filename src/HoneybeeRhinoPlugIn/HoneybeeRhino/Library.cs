using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                (_defaultConstructionSetUrl, "defaultConstructionSetUrl.json"),
                (_defaultProgramTypesUrl, "defaultProgramTypesUrl.json"),
                (_defaultHVACUrl, "defaultHVACUrl.json" )
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

        //public static IEnumerable<string> DownloadAllDefaultLibraries()
        //{
        //    var files = _defaultLibraryFiles.Select(_ => DownLoadLibrary(_.Url, _.FilePath));
        //    return files;
        //}

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
            //Check or download all json libraries.
            CheckAllDefaultLibraries();

            var jsonFile = Path.Combine(HoneybeeStandardFolder, jsonFilename);

            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            using (StreamReader file = File.OpenText(jsonFile))
            {
                var json = file.ReadToEnd();

                var hbObj = func(json);
                return new T[1] { hbObj };
            }
               
        }

        //public static IEnumerable<HB.ProgramTypeAbridged> LoadProgramTypes(string jsonFile)
        //{
        //    if (!File.Exists(jsonFile))
        //        throw new ArgumentException($"Invalid file: {jsonFile}");

        //    using (StreamReader file = File.OpenText(jsonFile))
        //    {
        //        using (JsonTextReader reader = new JsonTextReader(file))
        //        {
        //            JObject o2 = (JObject)JToken.ReadFrom(reader);
        //            var programs = o2.Values();
        //            var st = programs.First().ToString();
        //            return programs.Select(_ => HB.ProgramTypeAbridged.FromJson(_.ToString()));
        //        }

        //    }

        //}
    }
}
