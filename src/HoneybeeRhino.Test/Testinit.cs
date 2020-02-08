using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.InteropServices;


namespace HoneybeeRhino.Test
{
    ////https://github.com/tmakin/RhinoCommonUnitTesting/blob/master/Src/RhinoPlugin.Tests/TestInit.cs
    ////this is NUnit test
    //[SetUpFixture]
    //public class Testinit
    //{ 
    //    static bool initialized = false;
    //    static string systemDir = null;

    //    [OneTimeSetUp]
    //    public void Init()
    //    {
    //        if (initialized)
    //        {
    //            throw new InvalidOperationException("AssemblyInitialize should only be called once");
    //        }
    //        initialized = true;
    //        TestContext.WriteLine("Assembly init started");

    //        // Ensure we are 64 bit
    //        Assert.IsTrue(Environment.Is64BitProcess, "Tests must be run as x64. Test > Test Settings > Default Processor Architecture > x64");

    //        // Set path to rhino system directory
    //        string envPath = Environment.GetEnvironmentVariable("path");
    //        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    //        systemDir = System.IO.Path.Combine(programFiles, "Rhino 7 WIP", "System");
    //        Assert.IsTrue(System.IO.Directory.Exists(systemDir), "Rhino system dir not found: {0}", systemDir);

    //        // Add rhino system directory to path (for RhinoLibrary.dll)
    //        Environment.SetEnvironmentVariable("path", envPath + ";" + systemDir);

    //        // Add hook for .Net assmbly resolve (for RhinoCommmon.dll)
    //        AppDomain.CurrentDomain.AssemblyResolve += ResolveRhinoCommon;

    //        // Start headless Rhino process
    //        LaunchInProcess(0, 0);
    //    }

    //    private static Assembly ResolveRhinoCommon(object sender, ResolveEventArgs args)
    //    {
    //        var name = args.Name;

    //        if (!name.StartsWith("RhinoCommon"))
    //        {
    //            return null;
    //        }

    //        var path = System.IO.Path.Combine(systemDir, "RhinoCommon.dll");
    //        return Assembly.LoadFrom(path);
    //    }

    //    [OneTimeTearDown]
    //    public static void AssemblyCleanup()
    //    {
    //        // Shotdown the rhino process at the end of the test run
    //        ExitInProcess();
    //    }

    //    [DllImport("RhinoLibrary.dll")]
    //    internal static extern int LaunchInProcess(int reserved1, int reserved2);

    //    [DllImport("RhinoLibrary.dll")]
    //    internal static extern int ExitInProcess();

    //}
}
