using System;
using Rhino;
using Rhino.Commands;
using NUnitLite;
using Tests = HoneybeeRhino.Test;

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
                // Go get the assembly through your preferred method.
                var assembly = typeof(Tests.RoomEntityTests).Assembly;
                new AutoRun(assembly).Execute(new string[] { }, new RhinoConsoleTextWriter(), null);
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