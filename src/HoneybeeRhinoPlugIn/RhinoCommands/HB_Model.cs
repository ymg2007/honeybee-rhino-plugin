using System;
using Rhino;
using Rhino.Commands;
using System.Linq;
using Rhino.UI;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_Model : Command
    {
        static HB_Model _instance;
        public HB_Model()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_CreateHBModel command.</summary>
        public static HB_Model Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_Model"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            var tb = HoneybeeRhinoPlugIn.Instance.ModelEntityTable;
            if (!tb.Any())
            {
                var modelName = $"Model_{System.Guid.NewGuid()}";
                var model = new HB.Model(modelName, new HB.ModelProperties(energy: HB.ModelEnergyProperties.Default));
                var modelEnt = new Entities.ModelEntity(model);
                modelEnt.AddToDocument(tb);
            }

            var modelEntity = tb.First().Value;
            
            var rc = Result.Cancel;
            if (mode == RunMode.Interactive)
            {
                var dialog = new UI.HBModelDialog(modelEntity);
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    tb[dialog_rc.ModelNameID] = dialog_rc;
                    rc = Result.Success;
                }

            }
            else
            {
                var msg = string.Format($"Scriptable version of {EnglishName} command not implemented.");
                RhinoApp.WriteLine(msg);
            }
            return rc;

            //if (roomEnergyProperties == null)
            //{
            //    RhinoApp.WriteLine("No valid room energy property was set!");
            //    return Result.Nothing;
            //}
        }
    }
}