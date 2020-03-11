using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using HB = HoneybeeSchema;
using HoneybeeRhino.Entities;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_RunSimu : Command
    {
        static HB_RunSimu _instance;
        public HB_RunSimu()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_RunSimu command.</summary>
        public static HB_RunSimu Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_RunSimu"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var modelEnt = HoneybeeRhinoPlugIn.Instance.ModelEntityTable.First().Value;
            var model = modelEnt.GetHBModel();

            var folder = @"D:\Dev\test\HB";
            folder = Path.Combine(Path.GetTempPath(), "Honeybee", Path.GetRandomFileName());
            Directory.CreateDirectory(folder);

            var modelPath = Path.Combine(folder, "model.json");
            File.WriteAllText(modelPath, model.ToJson());

            try
            {
                Run(modelPath);
            }
            catch
            {
                // Log error.
            }
            finally
            {
                RhinoApp.WriteLine($"OpenStudio files saved at: {folder}");
                RhinoApp.WriteLine($"Right now this command only translates model to osm. \nRun simulation will be added after SimulationParamerter and ResourceManagement is implemented");
            }
         

            return Result.Success;
        }

        static void Run(string modelFilePath)
        {
            var folder = Path.GetDirectoryName(modelFilePath); // @"C:\Users\mingo\AppData\Local\Temp\Honeybee\fq1odqrr.5p3";
    
            var scr = $"/C honeybee-energy translate model-to-osm {modelFilePath} \n\rPAUSE";
            //scr = $"/C honeybee-energy simulate model {Path.Combine(folder, model)} {epw}";

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "cmd.exe";
            startInfo.WorkingDirectory = folder;
            startInfo.Arguments = scr;

            using (var exeProcess = new Process())
            {
                exeProcess.StartInfo = startInfo;
                exeProcess.Start();
                exeProcess.WaitForExit();
            }
            
        }
    }
}