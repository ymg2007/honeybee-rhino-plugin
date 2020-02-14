using System;
using Rhino;
using Rhino.Commands;
using NUnitLite;
using Tests = HoneybeeRhino.Test;
using Rhino.Input.Custom;

namespace RhinoNUnitTestRunner
{
    public class NUnitTestRunner : Command
    {
        static NUnitTestRunner _instance;
        public NUnitTestRunner()
        {
            _instance = this;
        }

        ///<summary>The only instance of the RoomEntity_TestRunner command.</summary>
        public static NUnitTestRunner Instance
        {
            get { return _instance; }
        }

        public override string EnglishName
        {
            get { return "NUnitTestRunner"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                var gs = new GetString();
                gs.SetCommandPrompt("set arguments");
                gs.GetLiteralString();
                if (gs.CommandResult() != Result.Success)
                    return gs.CommandResult();
                //https://github.com/nunit/docs/wiki/Console-Command-Line
                var args = gs.StringResult().Split(',');
                var assembly = typeof(Tests.RoomEntityTests).Assembly;
                new AutoRun(assembly).Execute(args, new RhinoConsoleTextWriter(), null);
            }
            catch (Exception e)
            {
                RhinoApp.Write(e.ToString());
                return Result.Failure;
            }

            return Result.Success;
        }
    }
}