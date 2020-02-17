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
    public static class Utility
    {
        public static string HoneybeeStandardURL { get; } = @"https://raw.githubusercontent.com/ladybug-tools/honeybee-standards/master/honeybee_standards/data/programtypes/default.json";
        public static string HoneybeeStandardFolder { get; } = Path.Combine(Path.GetTempPath(), "Ladybug", "Library");
        public static string DownLoadDefaultLibrary(string standardsUrl)
        {
            var url = standardsUrl;
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                Directory.CreateDirectory(HoneybeeStandardFolder);
                var file = Path.Combine(HoneybeeStandardFolder, "programtypes.json");
                wc.DownloadFile(url, file);
                return file;
            }
        }

        public static IEnumerable<HB.ProgramTypeAbridged> LoadProgramTypes(string jsonFile)
        {
            if (!File.Exists(jsonFile))
                throw new ArgumentException($"Invalid file: {jsonFile}");

            using (StreamReader file = File.OpenText(jsonFile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    
                    JObject o2 = (JObject)JToken.ReadFrom(reader);
                    var programs = o2.Values();
                    var st = programs.First().ToString();
                    return programs.Select(_ => HB.ProgramTypeAbridged.FromJson(_.ToString()));
                }
           
            }
     
        }
    }
}
