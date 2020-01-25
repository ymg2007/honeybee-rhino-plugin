﻿// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Geometry;
using HoneybeeDotNet.Model;

namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class TestClass
    {
        /// <summary>
        /// Transform a brep using a translation
        /// </summary>
        [Test]
        public void Init_RhinoObj_Test()
        {
            // Arrange
            var bb = new BoundingBox(new Point3d(0, 0, 0), new Point3d(100, 100, 100));
            var brep = bb.ToBrep();
            var t = Transform.Translation(new Vector3d(30, 40, 50));

            // Act
            brep.Transform(t);

            // Assert
            Assert.AreEqual(brep.GetBoundingBox(true).Center, new Point3d(80, 90, 100));
        }

        [Test]
        public void Init_HBObj_Test()
        {
            var face = new Face3D(
                new List<List<decimal>>()
                {
                    new List<decimal>(){0,0,0 },
                    new List<decimal>(){0.5M,0.5M,0.5M },
                    new List<decimal>(){1,0,0 }
                });

            var door = new Door("mainEntrance", face);

            // Assert
            Assert.AreEqual(door.BoundaryCondition.Obj.GetType(), typeof(Outdoors));
        }
    }
}
