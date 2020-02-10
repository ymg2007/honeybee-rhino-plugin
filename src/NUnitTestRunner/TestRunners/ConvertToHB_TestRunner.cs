using System;
using Rhino;
using Rhino.Commands;
using NUnitLite;
using Tests = HoneybeeRhino.Test;

namespace RhinoNUnitTestRunner
{
    //https://github.com/JoinCAD/RhinoNUnitTestRunner
    public class ConvertToHB_TestRunner : Command
    {
        public ConvertToHB_TestRunner()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static ConvertToHB_TestRunner Instance
        {
            get; private set;
        }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName
        {
            get { return "ConvertToHB_TestRunner"; }
        }

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            try
            {
                // Go get the assembly through your preferred method.
                var assembly = typeof(Tests.ConvertToHB).Assembly;
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
