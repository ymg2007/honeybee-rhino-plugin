﻿using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace HoneybeeRhino
{
    public class SimulationRunner
    {
        private static string _studyFolder;
        public SimulationRunner()
        {

        }

        public static void RunOpenStudio(string studyFolder, string modelJsonPath, string simParPath)
        {
            var osw =  WriteOSW(studyFolder, modelJsonPath, simParPath);
            if (!string.IsNullOrEmpty(osw))
            {
                WriteBash(studyFolder, osw);
            }
            else
            {
                throw new ArgumentException("Failed to write osw file");
            }
           

        }

        public static string WriteOSW(string studyFolder, string modelJsonPath, string simParPath)
        {
            //Measure path
            var measureFolder = @"C:\Users\mingo\AppData\Roaming\McNeel\Rhinoceros\6.0\scripts\energy_model_measure\lib";
            var epwFile = @"C:\openstudio - 2.9.1\Examples\compact_osw\files\srrl_2013_amy.epw";

            //var osw = @"
            //{
            //    'steps':
            //     [
            //        {
            //                'arguments' : {
            //                    'model_json' : ##model_json_path
            //         },
            //                 'measure_dir_name': 'from_honeybee_model'
            //                 },
            //        {
            //                'arguments' : {
            //                    'simulation_parameter_json' : ##sim_par_json_path
            //                    },
            //                 'measure_dir_name': 'from_honeybee_simulation_parameter'
            //        }
            //    ],
            //    'measure_paths':[##measure_directory],
            //    'weather_file':[##epw_file],
            //}";
            //osw = osw.Replace("##model_json_path", modelJsonPath);
            //osw = osw.Replace("##sim_par_json_path", simParPath);
            //osw = osw.Replace("##measure_directory", measureFolder);
            //osw = osw.Replace("##epw_file", epwFile);

            //var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(osw);

            JObject oswJson = new JObject(
                new JProperty("steps",
                    new JArray()
                    {
                        new JObject(
                            new JProperty("arguments", new JObject(new JProperty("model_json", modelJsonPath))),
                            new JProperty("measure_dir_name", "from_honeybee_model")
                            ),

                        new JObject(
                            new JProperty("arguments", new JObject(new JProperty("simulation_parameter_json", simParPath))),
                            new JProperty("measure_dir_name", "from_honeybee_simulation_parameter")
                            )

                    }),
                new JProperty("measure_paths", measureFolder),
                new JProperty("weather_file", epwFile)
                );


            var workflow = Path.Combine(studyFolder, "workflow.osw");
            File.Delete(workflow);
            File.WriteAllText(workflow, oswJson.ToString());
            if (File.Exists(workflow))
            {
                return workflow;
            }
            else
            {
                return string.Empty;
            }
        

        }


        public static bool WriteBash(string studyFolder, string oswPath )
        {
            var text = @"c:
cd C:\openstudio-2.9.1\bin
""openstudio.exe"" -I C:\Users\mingo\AppData\Roaming\McNeel\Rhinoceros\6.0\scripts\energy_model_measure\lib run -m -w " + oswPath;

            var bash = Path.Combine(studyFolder, "run_simulation.bat");
            File.Delete(bash);
            File.WriteAllText(bash, text.ToString());
            return File.Exists(bash);
        }
    }
}
