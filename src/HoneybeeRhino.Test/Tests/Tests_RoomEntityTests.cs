﻿using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino;
using Rhino.DocObjects;
using HoneybeeRhino.Entities;
using HoneybeeRhino;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Test
{
    [TestFixture]
    public class Tests_RoomEntityTests
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        RhinoDoc _doc = RhinoDoc.ActiveDoc;
        double _tol = 0.0001;

        public ObjRef InitRoomBrepObject(Brep brep)
        {
            var id = _doc.Objects.Add(brep);
            //var rhinoObj = _doc.Objects.FindId(id);
            var rhinoObj = new ObjRef(id);
            var a =  EntityHelper.ToRoomBrepObj(rhinoObj);
            _doc.Objects.Replace(rhinoObj, a);
            return rhinoObj;
        }
        public ObjRef InitBrepObject(Brep brep)
        {
            var id = _doc.Objects.Add(brep);
            var rhinoObj = new ObjRef(id);
            return rhinoObj;
        }

        public ObjRef InitRoomBox()
        {
            var allObjs = _doc.Objects;

            var bbox = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 10, 3));
            var box = new Box(bbox);
            return InitRoomBrepObject(box.ToBrep());

        }

        public List<ObjRef> InitTwoBoxes()
        {
            var bbox1 = new BoundingBox(new Point3d(0, 0, 0), new Point3d(10, 8, 3));
            var b1 = new Box(bbox1).ToBrep();
            var bbox2 = new BoundingBox(new Point3d(-1, -1, 3.00001), new Point3d(3, 3, 5.5));
            var b2 = new Box(bbox2).ToBrep();

            var obj1 = InitRoomBrepObject(b1);
            var obj2 = InitRoomBrepObject(b2);
            return new List<ObjRef>() { obj1 , obj2};
        }
        public List<Brep> LoadBrepsFromJson(string file)
        {
            string json = System.IO.File.ReadAllText(file);
            var breps = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Brep>>(json);
            return breps;
        }


        [Test]
        public void Test_BoxToRoom()
        {
            var rhinoObj = InitRoomBox();
            var room = rhinoObj.Brep();

            var ent = room.TryGetRoomEntity();
            Assert.IsTrue(ent.IsValid);
            var srfNames = room.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name);
            Assert.AreEqual(ent.HBFaces.Count(), 6);
            Assert.AreEqual(srfNames.Count(), 6);

        }

        [Test]
        public void Test_SingleRoomBoundaryCondition()
        {
            var rhinoObj = InitRoomBox();
            var room = rhinoObj.Brep();

            var faces = room.Faces.Select(_ => _.TryGetFaceEntity().HBObject);

            Assert.IsTrue(faces.Where(_ => _.BoundaryCondition.Obj is HB.Outdoors).Count() == 5);
            Assert.IsTrue(faces.Where(_ => _.BoundaryCondition.Obj is HB.Ground).Count() == 1);
        }

        [Test]
        public void Test_SingleRoomFaceType()
        {
            var rhinoObj = InitRoomBox();
            var room = rhinoObj.Brep();

            var faces = room.Faces.Select(_ => _.TryGetFaceEntity().HBObject);

            Assert.IsTrue(faces.Where(_ => _.FaceType == HB.Face.FaceTypeEnum.Wall).Count() == 4);
            Assert.IsTrue(faces.Where(_ => _.FaceType == HB.Face.FaceTypeEnum.Floor).Count() == 1);
            Assert.IsTrue(faces.Where(_ => _.FaceType == HB.Face.FaceTypeEnum.Floor).First().BoundaryCondition.Obj is HB.Ground);
            Assert.IsTrue(faces.Where(_ => _.FaceType == HB.Face.FaceTypeEnum.RoofCeiling).Count() == 1);
        }

        [Test]
        public void Test_Room_DuplicateBrep()
        {
            var rhinoObj = InitRoomBox();
            var room = rhinoObj.Brep();
            var newRoom = room.DuplicateBrep();

            var ent = room.TryGetRoomEntity();
            var newEnt = newRoom.TryGetRoomEntity();
            var srfNames = room.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name).Distinct().ToArray();
            var newSrfNames = room.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name).Distinct().ToArray();
            Assert.AreEqual(ent.HBFaces.Count(), newEnt.HBFaces.Count());
            Assert.IsTrue(ent.HostObjRef == newEnt.HostObjRef);
            Assert.AreEqual(srfNames[1], newSrfNames[1]);

        }

        [Test]
        public void Test_Room_DuplicateBrep_Parallel()
        {
            var rhinoObj = InitRoomBox();
            var room = rhinoObj.Brep();
            var rooms = new Brep[50];

            Parallel.For(0, 50, i =>
            {
                rooms[i] = room.DuplicateBrep();
            });
         
            var ents = rooms.Select(_=>_.TryGetRoomEntity());

            Assert.IsTrue(ents.All(_ => _.HBObject != null));

        }

        [Test]
        public void Test_RingFiveRooms()
        {
            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBrepsFromJson(file);
            //breps = _doc.Objects.Where(_ => _.IsSelected(true) >= 1).Select(_=>Brep.TryConvertBrep(_.Geometry)).ToList();
            var rooms = breps.Select(_=> InitRoomBrepObject(_).Brep());

            var ents = rooms.Select(_=>_.TryGetRoomEntity());
            var srfNames = rooms.Select(rm=>rm.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name)).ToList();

            Assert.IsTrue(rooms.Count() == 5);
            Assert.IsTrue(ents.All(_ => _.IsValid));
            Assert.IsTrue(ents.All(_ => _.HostObjRef != null));
            for (int i = 0; i < breps.Count(); i++)
            {
                Assert.IsTrue(srfNames[i].Count() == breps[i].Faces.Count);
            }

        }

        [Test]
        public void Test_MoveRoom()
        {
            var rhinoObj = InitRoomBox().Object() as BrepObject;
            var geo = rhinoObj.Geometry;

            var t = Transform.Translation(new Vector3d(10, 10, 0));
            geo.Transform(t);
            _doc.Objects.Replace(rhinoObj.Id, Brep.TryConvertBrep(geo));
            var newObj = new ObjRef(_doc.Objects.FindId(rhinoObj.Id));
            var ent = newObj.TryGetRoomEntity();

            Assert.IsTrue(ent.IsValid);
            Assert.AreEqual(ent.HBFaces.Count, 6);
            Assert.IsTrue(ent.HostObjRef.ObjectId == rhinoObj.Id);

            _doc.Objects.Purge(newObj.Object());
            _doc.Objects.Purge(rhinoObj);
        }

        [Test]
        public void Test_CopyRoomEntity()
        {
            var rhinoObj = InitRoomBox();


            var dupGeo = rhinoObj.Brep().DuplicateBrep();

            var newObj = InitRoomBrepObject(dupGeo);

            var ent = dupGeo.TryGetRoomEntity();
            ent.UpdateHost(newObj);

            Assert.AreEqual(ent.HBFaces.Count, rhinoObj.TryGetRoomEntity().HBFaces.Count);
            Assert.IsTrue(ent.HostObjRef.ObjectId == newObj.ObjectId);

            _doc.Objects.Purge(rhinoObj.Object());
            _doc.Objects.Purge(newObj.Object());

        }

        [Test]
        public void Test_Coplanar()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
            var breps = LoadBrepsFromJson(file);

            var firstB = breps[0];
            var secondB = breps[1];

            var cutters = firstB.GetAdjFaces(secondB, _tol);

            Assert.AreEqual(cutters.Count(), 1);

            var cutterProp = AreaMassProperties.Compute(cutters.First().matchedCutters.First());
            Assert.IsTrue(Math.Abs(cutterProp.Area - 4 * 3.5) < _tol);
            Assert.IsTrue(cutterProp.Centroid.DistanceToSquared(new Point3d(2, 0, 1.75)) < Math.Pow(_tol, 2));

        }

        [Test]
        public void Test_IntersectMass_TwoSimpleBreps()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\TwoSimpleBreps.json";
            var breps = LoadBrepsFromJson(file);

            var rooms = breps.Select(_ => InitRoomBrepObject(_).Brep());
            var solver = new AdjacencySolver(rooms);

            var intersectedBreps = solver.ExecuteIntersectMasses(rooms, _tol).ToList();

            var firstB = intersectedBreps[0];
            var secondB = intersectedBreps[1];

            //check if there is a new face created
            Assert.IsTrue(firstB.IsSolid);
            Assert.AreEqual(firstB.Faces.Count, 7);
            Assert.IsTrue(secondB.IsSolid);
            Assert.AreEqual(secondB.Faces.Count, 7);


            //Check if all face names are identical
            var hbObjs_first = firstB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);
            var hbObjs_second = secondB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);

            var faceNames_first = hbObjs_first.Select(_ => _.Name);
            var faceNames_second = hbObjs_second.Select(_ => _.Name);

            Assert.IsTrue(faceNames_first.Count() - faceNames_first.Distinct().Count() == 1);
            Assert.IsTrue(faceNames_second.Count() - faceNames_second.Distinct().Count() == 1);


            //check adjacent face property: match center point
            var indoorFace_first = firstB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);
            var indoorFace_second = secondB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);

            var center_first = AreaMassProperties.Compute(indoorFace_first).Centroid;
            var center_second = AreaMassProperties.Compute(indoorFace_second).Centroid;

            Assert.IsTrue(center_first.DistanceToSquared(center_second) < Math.Pow(_tol, 2));


        }

        [Test]
        public void Test_SolveAdjacency_TwoSimpleBrepsWithTol()
        {

            var rhinoObjs = InitTwoBoxes();
            var breps = rhinoObjs.Select(_ => _.Brep());

            var solver = new AdjacencySolver(breps);

            var intersectedBreps = solver.ExecuteIntersectMasses(breps, _tol).ToList();
            intersectedBreps = solver.ExecuteMatchInteriorFaces(intersectedBreps, _tol).ToList();

            var firstB = intersectedBreps[0];
            var secondB = intersectedBreps[1];

            //check if there is a new face created
            Assert.IsTrue(firstB.IsSolid);
            Assert.AreEqual(firstB.Faces.Count, 7);
            Assert.IsTrue(secondB.IsSolid);
            Assert.AreEqual(secondB.Faces.Count, 7);


            //Check if all face names are identical
            var hbObjs_first = firstB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);
            var hbObjs_second = secondB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);

            var faceNames_first = hbObjs_first.Select(_ => _.Name);
            var faceNames_second = hbObjs_second.Select(_ => _.Name);

            Assert.IsTrue( faceNames_first.Distinct().Count() == 7);
            Assert.IsTrue( faceNames_second.Distinct().Count() == 7);


            var faceBC_first = hbObjs_first.Select(_ => _.BoundaryCondition);
            var faceBC_second = hbObjs_second.Select(_ => _.BoundaryCondition);

            Assert.IsTrue(faceBC_first.Where(_=>_.Obj is HB.Surface).Count()==1);
            Assert.IsTrue(faceBC_second.Where(_ => _.Obj is HB.Surface).Count() == 1);


        }

        [Test]
        public void Test_DuplicateBreps_WithUserData()
        {
            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBrepsFromJson(file);
            var rooms = breps.Select(_ => InitRoomBrepObject(_).Brep());
            var isRoom = rooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

            var intersectedBreps = rooms.Select(_ => _.DuplicateBrep());
            var isNewRoomSurfacesOK = rooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

            Assert.IsTrue(isNewRoomSurfacesOK.All(_ => _));
        }

        [Test]
        public void Test_IntersectMass_MoreBreps()
        {

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\RingFiveBreps.json";
            var breps = LoadBrepsFromJson(file);
            var rooms = breps.Select(_ => InitRoomBrepObject(_).Brep());

            var dupBreps = rooms.Select(_ => _.DuplicateBrep());
            var adjSolver = new AdjacencySolver(dupBreps);

            var intersectedBreps = adjSolver.Execute(_tol);

            //check if there is a new face created
            Assert.IsTrue(intersectedBreps.All(_ => _.IsSolid));
            Assert.IsTrue(intersectedBreps.All(_ => _.Faces.Count == 8));
            Assert.IsTrue(intersectedBreps.All(_ => _.IsRoom()));


            ////Check if all face names are identical
            var names = intersectedBreps.Select(b => b.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name));
            var diffNames = names.Distinct();
            Assert.IsTrue(names.Sum(_ => _.Count()) == diffNames.Sum(_ => _.Count()));

            //var hbObjs_second = secondB.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject);

            //var faceNames_first = hbObjs_first.Select(_ => _.Name).Distinct();
            //var faceNames_second = hbObjs_second.Select(_ => _.Name).Distinct();

            //Assert.IsTrue(faceNames_first.Count() == 7);
            //Assert.IsTrue(faceNames_second.Count() == 7);

            ////check adjacent face property: match center point
            //var indoorFace_first = firstB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);
            //var indoorFace_second = secondB.Faces.First(_ => Math.Abs(AreaMassProperties.Compute(_).Area - 12) < _tol);

            //var center_first = AreaMassProperties.Compute(indoorFace_first).Centroid;
            //var center_second = AreaMassProperties.Compute(indoorFace_second).Centroid;
            //Assert.IsTrue(center_first.DistanceToSquared(center_second) < Math.Pow(_tol, 2));
            //throw new Exception("dddd");

        }

        [Test]
        public void Test_ParallelIntersectMass_28Breps()
        {
            TestContext.WriteLine($"Testing: {nameof(Test_ParallelIntersectMass_28Breps)}");

            string file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\28Breps.json";
            var breps = LoadBrepsFromJson(file);
            var rooms = breps.Select(_ => InitRoomBrepObject(_).Brep());

            var dupBreps = rooms.Select(_ => _.DuplicateBrep());
            var adjSolver = new AdjacencySolver(dupBreps);
            sw.Restart();
            var intersectedBreps = adjSolver.Execute(_tol, true);
            sw.Stop();
            var ms = sw.Elapsed.TotalSeconds;
            TestContext.WriteLine($"Runtime: {ms} ms");

            //check if there is a new face created
            Assert.IsTrue(intersectedBreps.All(_ => _.IsSolid));
            Assert.IsTrue(intersectedBreps.All(_ => _.IsRoom()));


            ////Check if all face names are identical
            var names = intersectedBreps.Select(b => b.Surfaces.Select(_ => _.TryGetFaceEntity().HBObject.Name));

            Assert.IsTrue(names.Sum(_ => _.Count()) == intersectedBreps.Sum(_ => _.Faces.Count()));

        }


        [Test]
        public void Test_SetProgramType()
        {
            var rhinoObj = InitRoomBox();
            var geo = rhinoObj.Brep();

            var constructionset = EnergyLibrary.DefaultConstructionSets.First();
            var programtype = EnergyLibrary.DefaultProgramTypes.First();
            var hvac = EnergyLibrary.DefaultHVACs.First();

            var enertyProp = new HB.RoomEnergyPropertiesAbridged
                (
                constructionSet: constructionset.Name, 
                programType: programtype.Name, 
                hvac: hvac.Name
                );

            geo = HoneybeeRhino.SetRoomEnergyProperties(geo, enertyProp);

            var checkEnergyProp = geo.TryGetRoomEntity().GetEnergyProp();
            var constName = checkEnergyProp.ConstructionSet;
            var typeName = checkEnergyProp.ProgramType;
            var hvacName = checkEnergyProp.Hvac;
            Assert.IsTrue(!string.IsNullOrEmpty(constName));
            Assert.IsTrue(!string.IsNullOrEmpty(typeName));
            Assert.IsTrue(!string.IsNullOrEmpty(hvacName));


        }



        //[Test]
        //public void Test_GroupEntity()
        //{
        //    var file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\SingleRoomAndWindow.json";
        //    var breps = LoadBrepsFromJson(file);


        //    var roomObj = InitBrepObject(breps.First(_ => _.IsSolid));
        //    var windowObj = InitBrepObject(breps.First(_ => _.IsSurface));

        //    //make room
        //    var roomBrepObj = roomObj.ToRoomBrepObj((Brep b) => _doc.Objects.Replace(roomObj.ObjectId, b));

        //    Assert.IsTrue(groupID != Guid.Empty);
        //    Assert.IsTrue(groupEnt != null);

        //    //make window //add to groupEntity
        //    var processedObj = roomBrepObj.AddAperture(windowObj);
        //    Assert.IsTrue(processedObj.apertures.Any());

        //    var apt = processedObj.apertures.First();

        //    var done = _doc.Objects.Replace(roomObj.ObjectId, processedObj.room);
        //    done &= _doc.Objects.Replace(apt.id, apt.brep);
        //    Assert.IsTrue(done);
        //    Assert.IsTrue(roomObj.Object().IsDeleted == false);
        //    Assert.IsTrue(windowObj.Object().IsDeleted == false);

        //    var newRoom = new ObjRef(roomObj.ObjectId).Object() as BrepObject;
        //    if (!newRoom.BrepGeometry.Surfaces.Where(_ => _.TryGetFaceEntity().ApertureObjRefs.Any()).Any())
        //        throw new ArgumentException("some thing wrong with assigning aperture!");

        //    //recheck if aperture is added to groupEntity
        //    groupEnt = GroupEntityTable[groupID];
        //    Assert.IsTrue(groupEnt.Apertures.First().ObjectId == windowObj.ObjectId);
        //    Assert.IsTrue(groupEnt.Room.Object().IsDeleted == false);
        //    Assert.IsTrue(groupEnt.Apertures.All(_=>_.Object().IsDeleted == false));

        //}

        [Test]
        public void Test_SelectGroupEntity()
        {
            var file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\SingleRoomAndWindow.json";
            var breps = LoadBrepsFromJson(file);


            var roomObj = InitBrepObject(breps.First(_ => _.IsSolid));
            var windowObj = InitBrepObject(breps.First(_ => _.IsSurface));

            //make room
            var roomBrep = EntityHelper.ToRoomBrepObj(roomObj);
            var roomBrepObj = new ObjRef(roomObj.ObjectId);
            _doc.Objects.Replace(roomBrepObj, roomBrep);
         

            //make window //add to groupEntity
            var processedObj = roomBrepObj.AddAperture(windowObj);
            var apt = processedObj.apertures.First();
            var done = _doc.Objects.Replace(roomObj.ObjectId, processedObj.room);
            done &= _doc.Objects.Replace(apt.id, apt.brep);


            var newRoom = new ObjRef(roomObj.ObjectId);
            _doc.Objects.Select(newRoom, true, true);
            var selectedObjs = _doc.Objects.GetSelectedObjects(false, false);
            var counts = selectedObjs.Count();

            _doc.Objects.UnselectAll();
            selectedObjs = _doc.Objects.GetSelectedObjects(false, false);
            foreach (var item in selectedObjs)
            {
                _doc.Objects.Purge(item);
            }
            Assert.IsTrue(counts == 2);

        }

        [Test]
        public void Test_CopyGroupEntity()
        {
            throw new ArgumentException("Cannot implement Ctrl+C and Ctrl+V, and also cannot do RhinoApp.ExecuteCommand()");
            //var file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\SingleRoomAndWindow.json";
            //var breps = LoadBrepsFromJson(file);


            //var roomObj = InitBrepObject(breps.First(_ => _.IsSolid));
            //var windowObj = InitBrepObject(breps.First(_ => _.IsSurface));

            ////make room
            //var roomBrepObj = roomObj.ToRoomBrepObj((Brep b) => _doc.Objects.Replace(roomObj.ObjectId, b), GroupEntityTable);
            //var groupID = roomBrepObj.Geometry().TryGetRoomEntity().GroupEntityID;
            //var groupEnt = GroupEntityTable[groupID];

            ////make window //add to groupEntity
            //var processedObj = roomBrepObj.AddAperture(windowObj);
            //var done = _doc.Objects.Replace(roomObj.ObjectId, processedObj.room);
            //done &= _doc.Objects.Replace(windowObj.ObjectId, processedObj.aperture);


            //var newRoom = new ObjRef(roomObj.ObjectId);
            //_doc.Objects.Select(newRoom, true, true);
            //var selectedObjs = _doc.Objects.GetSelectedObjects(false, false);
            //_doc.Views.Redraw();
           
            ////var copied = RhinoApp.RunScript("_-CopyToClipboard", false);
            ////var pasted = RhinoApp.RunScript("_-Paste", false);
            ////var a =  RhinoApp.RunScript("_-Line 0,0,0 10,10,10", false);
            //////var counts = selectedObjs.Count();
            ////var rs = RhinoApp.ExecuteCommand(this._doc, "_CopyToClipboard");

            //var importFile = @"D:\Dev\Test\boxWithHbData.3dm";
            ////var command = $"_-Import {importFile} _Enter";
            ////var rs = RhinoApp.ExecuteCommand(this._doc, command);
            ////rs = RhinoApp.ExecuteCommand(this._doc, "Line 0,0,0 10,10,10");

            //var fileReadMode = new Rhino.FileIO.FileReadOptions();
            //fileReadMode.ImportMode = true;
            ////Rhino.RhinoDoc.ReadFile(importFile, fileReadMode);
            //var dddoc = RhinoDoc.Open(importFile, out bool isOpened);
            ////command = "_-Line 0,0,0 10,10,10";
            ////var ranResults = RhinoApp.RunScript(command, false);

        }
  


        [Test]
        public void Test_AddSingleWindow()
        {
            var file = @"D:\Dev\honeybee-rhino-plugin\src\HoneybeeRhino.Test\TestModels\SingleRoomAndWindow.json";
            var breps = LoadBrepsFromJson(file);

            var roomBrep = breps.First(_ => _.IsSolid);
            var windowBrep = breps.First(_ => _.IsSurface);

            var roomObj = InitRoomBrepObject(roomBrep);
            var roomID = roomObj.ObjectId;
            var windObj = InitBrepObject((windowBrep));
            var windID = windObj.ObjectId;

            var matchedRoomWindow = roomObj.AddAperture(windObj);

            var faceEnts = matchedRoomWindow.room.Surfaces.Select(_ => _.TryGetFaceEntity());
            if (!faceEnts.Where(_=>_.ApertureObjRefs.Any()).Any())
                throw new ArgumentException("some thing wrong with assigning aperture!");

            faceEnts = matchedRoomWindow.room.DuplicateBrep().Surfaces.Select(_ => _.TryGetFaceEntity());
            if (!faceEnts.Where(_ => _.ApertureObjRefs.Any()).Any())
                throw new ArgumentException("some thing wrong with assigning aperture!");
            //replace the old RhinoObject.
            var success = _doc.Objects.Replace(roomID, matchedRoomWindow.room);

            Assert.IsTrue(success);
            var newFoundGeo = _doc.Objects.FindId(roomID).Geometry;
            faceEnts = Brep.TryConvertBrep(newFoundGeo).Surfaces.Select(_ => _.TryGetFaceEntity());
            Assert.IsTrue(faceEnts.Where(_ => _.HBObject.Apertures.Any()).Any());
            Assert.IsTrue(faceEnts.Where(_ => _.ApertureObjRefs.Any()).Any());

            var newRoomId = _doc.Objects.Add(matchedRoomWindow.room);
            var newWindId = _doc.Objects.Add(matchedRoomWindow.apertures.First().brep);

            var foundGeo = _doc.Objects.FindId(newRoomId).Geometry;
            faceEnts = Brep.TryConvertBrep(foundGeo).Surfaces.Select(_ => _.TryGetFaceEntity());
            Assert.IsTrue(faceEnts.Where(_ => _.HBObject.Apertures.Any()).Any());
            Assert.IsTrue(faceEnts.Where(_ => _.ApertureObjRefs.Any()).Any());

        }

    }
}
