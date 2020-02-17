using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HoneybeeRhino
{
    public class AdjacencySolver
    {
        private IEnumerable<Brep> _rooms;

        //only for holding temp entity data, which will not be saved to file.
        private Dictionary<Guid, HBObjEntity> _tempEntityHolder = new Dictionary<Guid, HBObjEntity>();

        public AdjacencySolver(IEnumerable<Brep> rooms)
        {
            this._rooms = rooms;
        }

        public  IEnumerable<Brep> Execute(double tolerance, bool parallelCompute = false)
        {
            var checkedObjs = ExecuteIntersectMasses(tolerance, parallelCompute).ToList();
            //var matchedObj = checkedObjs.Select(_ => _.DuplicateBrep().MatchInteriorFaces(checkedObjs, tolerance));
            return checkedObjs;

        }

       
        public IEnumerable<Brep> ExecuteIntersectMasses(double tolerance, bool parallelCompute = false)
        {
            var allRooms = this._rooms;
            if (allRooms.Count() <= 1)
                return allRooms;


            //match each room's adjacent rooms before do split.
            var copy = allRooms.Select(_ => _.DuplicateBrep()).ToList();
            var count = copy.Count;
            var roomAndAdjacents = new List<(Brep room, IEnumerable<Brep> adjacentRooms)>();
            var solo = new List<Brep>();
            for (int i = 0; i < count; i++)
            {
                var room = copy[i];
                var roomBBox = room.GetBoundingBox(false);
                var adjacentRooms = copy.Where(_ => _.GetBoundingBox(false).isIntersected(roomBBox)).Select(_ => _.DuplicateBrep().DeleteHBEntity());
                if (adjacentRooms.Any())
                {
                    var dupRoom = room.DuplicateBrep().DetachHBEntityTo(this._tempEntityHolder);
                    roomAndAdjacents.Add((dupRoom, adjacentRooms));
                }
                else
                {
                    solo.Add(room);
                }
            }

            try
            {
                var intersected = new Brep[count];
                if (parallelCompute)
                {
                    intersected = roomAndAdjacents.AsParallel().AsOrdered().Select(_ => IntersectWithMasses(_.room, _.adjacentRooms, tolerance)).ToArray();
                }
                else
                {
                    intersected = roomAndAdjacents.Select(_ => IntersectWithMasses(_.room, _.adjacentRooms, tolerance)).ToArray();
                }
                //add HBObjEntity back to geometry again.
                var intersetedRooms = intersected.Select(_ => _.ReinstallHBEntityFrom(this._tempEntityHolder)).ToList();
                intersetedRooms.AddRange(solo);
                this._tempEntityHolder.Clear();
                //var isRoom = otherRooms.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));
                //var isRoom2 = intersected.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));

                return intersetedRooms;
            }
            catch (Exception)
            {

                throw;
            }


        }

        private static Brep IntersectWithMasses(Brep roomGeo, IEnumerable<Brep> adjacentRooms, double tolerance)
        {
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            //Check bounding boxes first
            var roomBBox = roomGeo.GetBoundingBox(false);

            var currentBrep = roomGeo;
            var allBreps = adjacentRooms;

            //var currentBrepFaces = currentBrep.Faces;
            //var isRoomValid = allBreps.Select(_ => _.Surfaces.All(s => s.TryGetFaceEntity().IsValid));
            //var isThisRoomValid = currentBrep.Surfaces.All(s => s.TryGetFaceEntity().IsValid);
            //var faceAreasBeforeSplit = currentBrepFaces.Select(_ => AreaMassProperties.Compute(_).Area);
            foreach (Brep adjBrep in allBreps)
            {
                var isDup = adjBrep.IsDuplicate(roomGeo, tolerance);
                if (isDup)
                    continue;

                //Get matched faces, and its adjacent cutters.
                var matchAndCutters = currentBrep.GetAdjFaces(adjBrep, tolerance);
                //There is no overlapping area.
                if (!matchAndCutters.Any())
                    continue;


                //Split and Join
                var solidBrep = currentBrep;
                var cuttedElements = new List<Brep>();
                foreach (var matchAndCutter in matchAndCutters)
                {
                    var currentRoomFace = matchAndCutter.roomFace;
                    var cutters = matchAndCutter.matchedCutters.Select(_ => _.DuplicateFace(false)).ToList();
                    //Split the current brep by cutters
                    var newBreps = solidBrep.Split(cutters, tolerance).SkipWhile(_ => _ == null);

                    if (!newBreps.Any())
                        continue;


                    //var ent1 = currentRoomFace.UnderlyingSurface().TryGetFaceEntity();
                    ////var ent2 = currentBrep.Surfaces[currentRoomFace.SurfaceIndex].TryGetFaceEntity();
                    //////var ent3 = currentRoomFace.ToBrep().Surfaces[0].TryGetFaceEntity();
                    ////var ent = currentRoomFace.TryGetFaceEntity();

                    var faceEntID = currentRoomFace.UnderlyingSurface().GetUserString("HBDataID");
                    //assign new name ID to newly split faces.
                    //DO NOT use following Linq expression, because ToList() creates new object, instead of referencing the same one. 
                    //newBreps.Where(_ => _.Faces.Count == 1).ToList().ForEach(_ => _.TryGetFaceEntity().UpdateID_CopyFrom(ent));
                    if (!string.IsNullOrEmpty(faceEntID))
                    {
                        foreach (var item in newBreps)
                        {
                            if (item.Faces.Count == 1)
                            {
                                item.Surfaces[0].SetUserString("HBDataID", faceEntID);
                            }
                        }
                    }


                    //Join back to solid
                    //var roomEnts = newBreps.Select(_ => _.Surfaces.Select(s => s.TryGetFaceEntity()));
                    var newBrep = Brep.CreateSolid(newBreps, tolerance);
                    solidBrep = newBrep.First();
                    solidBrep.Faces.ShrinkFaces();
                }
                //just to make logically clear, but they are essentially the same.
                //solidBrep is only used in above foreach loop
                currentBrep = solidBrep;

            }
            //move over the roomEntity to new geometry.
            //all faceEntities in Brep.surface stays even after split.
            var roomEntID = roomGeo.GetUserString("HBDataID");
            if (!string.IsNullOrEmpty(roomEntID))
            {
                currentBrep.SetUserString("HBDataID", roomEntID);
            }

            //TODO: update subsurfaces geometry data
            //Probably there is no need to update this geometry data until export to simulation engine.
            //No one needs this data.

            return currentBrep;

        }
        public static Brep MatchInteriorFaces(Brep room, IEnumerable<Brep> otherRooms, double tolerance)
        {
            tolerance = Math.Max(tolerance, Rhino.RhinoMath.ZeroTolerance);

            //Check bounding boxes first
            var roomBBox = room.GetBoundingBox(false);
            var adjacentRooms = otherRooms.Where(_ => roomBBox.isIntersected(_.GetBoundingBox(false)));

            var currentBrep = room;
            var currentRoomEnt = currentBrep.TryGetRoomEntity();
            var adjBreps = adjacentRooms;

            //Check sub-faces
            foreach (Brep adjBrep in adjBreps)
            {
                var adjRoomEnt = adjBrep.TryGetRoomEntity();
                //var adjFaces = adjBrep.Faces;
                //var isDup = adjBrep.IsDuplicate(currentBrep, tolerance);
                if (adjRoomEnt.GroupEntityID == currentRoomEnt.GroupEntityID)
                    continue;

                var matches = currentBrep.GetAdjFaces( adjBrep, tolerance);
                //ignore this face, and keep its original outdoor, ground, or surface;
                if (!matches.Any())
                    continue;

                foreach (var match in matches)
                {
                    var matchedSubFace = match.roomFace;
                    var curProp = AreaMassProperties.Compute(matchedSubFace);

                    var matchedadjFaces = match.matchedCutters;
                    var sameAreaFaces = matchedadjFaces.Where(_ => Math.Abs(AreaMassProperties.Compute(_).Area - curProp.Area) < tolerance);
                    if (!sameAreaFaces.Any())
                        continue;

                    //Check if two adjacent faces are really matching.
                    var sameCenterFaces = sameAreaFaces.Where(_ => AreaMassProperties.Compute(_).Centroid.DistanceToSquared(curProp.Centroid) < Math.Pow(tolerance, 2));
                    if (!sameCenterFaces.Any())
                        continue;

                    var matchedAdjFace = sameCenterFaces.First();

                    var adjEnt = adjBrep.Surfaces[matchedAdjFace.SurfaceIndex].TryGetFaceEntity();
                    var curEnt = currentBrep.Surfaces[matchedSubFace.SurfaceIndex].TryGetFaceEntity();
                    adjEnt.HBObject.BoundaryCondition = new HoneybeeSchema.Surface(new List<string>(2) { curEnt.HBObject.Name, currentRoomEnt.HBObject.Name });
                    curEnt.HBObject.BoundaryCondition = new HoneybeeSchema.Surface(new List<string>(2) { adjEnt.HBObject.Name, adjRoomEnt.HBObject.Name });
                }

            }


            return currentBrep;
        }

    }
}
