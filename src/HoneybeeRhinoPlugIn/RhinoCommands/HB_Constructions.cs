using System;
using Rhino;
using Rhino.Commands;
using HoneybeeSchema;
using Rhino.UI;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_Constructions : Command
    {
        static HB_Constructions _instance;
        public HB_Constructions()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_Constructions command.</summary>
        public static HB_Constructions Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_Constructions"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {

            var rc = Result.Cancel;
            OpaqueConstructionAbridged construction = null;

            if (mode == RunMode.Interactive)
            {
                var dialog = new UI.LibraryDialog_Constructions();
                dialog.RestorePosition();
                var dialog_rc = dialog.ShowSemiModal(doc, RhinoEtoApp.MainWindow);
                dialog.SavePosition();
                if (dialog_rc != null)
                {
                    construction = dialog_rc;
                    rc = Result.Success;
                }

            }
            else
            {
                var msg = string.Format($"Scriptable version of {EnglishName} command not implemented.");
                RhinoApp.WriteLine(msg);
            }

            if (construction == null)
            {
                RhinoApp.WriteLine("No valid room energy property was set!");
                return Result.Nothing;
            }

            return rc;
        }
    }
}