using System;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using HB = HoneybeeSchema;
using HoneybeeRhino.Entities;
using System.Collections.Generic;
using System.IO;

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

            //var groupEntities = HoneybeeRhinoPlugIn.Instance.GroupEntityTable.Select(_ => _.Value);
            //var rooms = groupEntities.Where(_=>_.IsValid).Select(_ => _.GetCompleteHBRoom()).ToList();


            var model = HoneybeeRhinoPlugIn.Instance.ModelEntityTable.First().Value.HBObject;
            var modelProp = new HB.ModelProperties(energy: HB.ModelEnergyProperties.Default);
            model.Properties = modelProp;
  
            var json = model.ToJson();

            var folder = @"D:\Dev\test\HB";
            var modelPath = Path.Combine(folder, "model.json");
            File.WriteAllText(modelPath, json);

            var cmdString = $"honeybee-energy translate model-to-osm {modelPath}";
            var cmdFile = Path.Combine(folder, "translate.bat");
            File.WriteAllText(cmdFile, cmdString);

            System.Diagnostics.Process.Start(cmdFile);

            return Result.Success;
        }
    }
}