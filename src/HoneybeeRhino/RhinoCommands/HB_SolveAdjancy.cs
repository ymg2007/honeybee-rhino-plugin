using System;
using Rhino;
using Rhino.Commands;

namespace HoneybeeRhino.RhinoCommands
{
    public class HB_SolveAdjancy : Command
    {
        static HB_SolveAdjancy _instance;
        public HB_SolveAdjancy()
        {
            _instance = this;
        }

        ///<summary>The only instance of the HB_SolveAdjancy command.</summary>
        public static HB_SolveAdjancy Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "HB_SolveAdjancy"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // TODO: complete command.
            return Result.Success;
        }
    }
}