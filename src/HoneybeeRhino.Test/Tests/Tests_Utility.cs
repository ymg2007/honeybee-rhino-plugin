using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino.Test.Tests
{
    [TestFixture]
    public class Tests_Utility
    {
        [Test]
        public void DownLoadDefaultLibrary()
        {
            var url = Utility.HoneybeeStandardURL;
            var jsonFile = HoneybeeRhino.Utility.DownLoadDefaultLibrary(url);
            
            Assert.IsTrue(File.Exists(jsonFile));
        }

        [Test]
        public void LoadProgramType()
        {
            var file = Path.Combine(Utility.HoneybeeStandardFolder, "programtypes.json");
            var hbObj = Utility.LoadProgramTypes(file).ToList();
            Assert.IsTrue(hbObj.First().Name == "Plenum");
            Assert.IsTrue(hbObj[1].People != null);
            Assert.IsTrue(hbObj[1].Lighting != null);
            Assert.IsTrue(hbObj[1].ElectricEquipment != null);
            Assert.IsTrue(hbObj[1].Infiltration != null);
            Assert.IsTrue(hbObj[1].Ventilation != null);
            Assert.IsTrue(hbObj[1].Setpoint != null);

        }
    }
}
