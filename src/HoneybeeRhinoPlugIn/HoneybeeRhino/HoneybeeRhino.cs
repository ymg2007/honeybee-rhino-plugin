using HoneybeeRhino.Entities;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneybeeRhino
{
    public static class HoneybeeRhinoExtension
    {
        public static List<(Brep room, Guid roomId, List<(Brep brep, Guid id)> apertures) > AddApertures(this IEnumerable<ObjRef> roomObjRefs, List<ObjRef> apertureObjRefs)
        {
            var processed = new List<(Brep room, Guid roomId, List<(Brep aperture, Guid apertureId)>)>();
            foreach (var room in roomObjRefs)
            {
                var match = room.AddApertures(apertureObjRefs);
                if (match.apertures.Any())
                    processed.Add(match);

            }
            return processed;
        }
        public static List<(Brep room, Guid roomId, List<(Brep brep, Guid id)> doors)> AddDoors(this IEnumerable<ObjRef> roomObjRefs, List<ObjRef> doorObjRefs)
        {
            var processed = new List<(Brep room, Guid roomId, List<(Brep brep, Guid doorId)>)>();
            foreach (var room in roomObjRefs)
            {
                var match = room.AddDoors(doorObjRefs);
                if (match.doors.Any())
                    processed.Add(match);

            }
            return processed;
        }

        public static (Brep room, Guid roomId, List<(Brep brep, Guid id)> apertures) AddApertures(this ObjRef roomObjRef, List<ObjRef> apertureObjRefs)
        {
            return roomObjRef.AddApertureDoor(apertureObjRefs, false);
        }

        public static (Brep room, Guid roomId, List<(Brep brep, Guid id)> doors) AddDoors(this ObjRef roomObjRef, List<ObjRef> doorObjRefs)
        {
            return roomObjRef.AddApertureDoor(doorObjRefs, true);
        }

        public static (Brep room, Guid roomId, List<(Brep brep, Guid id)> apertures) AddApertureDoor(this ObjRef roomObjRef, List<ObjRef> apertureObjRefs, bool isDoor = false)
        {
            //Only add to valid room obj
            if (!roomObjRef.IsRoom())
                throw new ArgumentException("Cannot assign aperture to non-room object!");


            var apertures = new List<(Brep brep, Guid id)>();

            //var tol = 0.0001;
            var roomBrep = roomObjRef.Brep().DuplicateBrep();
            var checkedApertureBreps = CheckApertureBreps(roomBrep, apertureObjRefs);

            //return empty aperture list;
            if (!checkedApertureBreps.Any())
                return (roomBrep, roomObjRef.ObjectId, apertures);

            var roomSrfs = roomBrep.Faces;
            foreach (var roomSrf in roomSrfs)
            {
                //Get intersected-coplanar matched aperture for this room face
                var matchedAptertures = GetMatchedBreps(roomSrf, checkedApertureBreps);

                if (!matchedAptertures.Any())
                    continue;

                var roomSrfEnt = roomSrf.TryGetFaceEntity();

                //Install all matched apertures to this room.
                foreach (var matchedApterture in matchedAptertures)
                {
                    (Brep brep, Guid id) processed;
                    if (isDoor)
                    {
                        processed = AddDoor(matchedApterture, roomSrfEnt);
                    }
                    else
                    {
                        processed = AddAperture(matchedApterture, roomSrfEnt);
                    }
                    apertures.Add(processed);

                }

            }

            return (roomBrep, roomObjRef.ObjectId, apertures);

            (Brep brep, Guid id) AddAperture((Brep brep, Guid id) matchedFace, FaceEntity roomFaceEnt)
            {
                var hostId = matchedFace.id;
                var aptBrep = matchedFace.brep;

                var newObjRef = new ObjRef(hostId);
                //Convert to Aperture Brep, and replace current rhino object
                aptBrep = EntityHelper.ToApertureBrep(aptBrep, hostId);

                //add to room face brep
                roomFaceEnt.AddAperture(newObjRef, aptBrep);

                //link host room objref to aperture entity
                aptBrep.TryGetApertureEntity().HostRoomObjRef = new ObjRef(roomObjRef.ObjectId);
                return (aptBrep, hostId);
            }

            (Brep brep, Guid id) AddDoor((Brep brep, Guid id) matchedFace, FaceEntity roomFaceEnt)
            {
                var hostId = matchedFace.id;
                var doorBrep = matchedFace.brep;

                var newObjRef = new ObjRef(hostId);
                //Convert to Aperture Brep, and replace current rhino object
                doorBrep = EntityHelper.ToDoorBrep(doorBrep, hostId);

                //add to room face brep
                roomFaceEnt.AddDoor(newObjRef, doorBrep);

                //link host room objref to aperture entity
                doorBrep.TryGetDoorEntity().HostRoomObjRef = new ObjRef(roomObjRef.ObjectId);
                return (doorBrep, hostId);
            }



            //Local method ===========================================================================================================
            IEnumerable<(Brep brep, Guid id)> CheckApertureBreps(Brep _roomBrep , List<ObjRef> _apertureObjRefs)
            {
                var tol = 0.001;
                //Duplicate
                var roomBbox = _roomBrep.GetBoundingBox(false);
                var checkedApertures = new List<(Brep, Guid)>();
                //Check if is intersected with room.
                foreach (var objRef in _apertureObjRefs)
                {
                    var apt = objRef.Brep().DuplicateBrep();
                    apt.Faces.ShrinkFaces();
                    var isInterseted = roomBbox.isIntersected(apt.GetBoundingBox(false), tol);
                    if (isInterseted)
                        checkedApertures.Add((apt, objRef.ObjectId));
                        
                }

                return checkedApertures;
            }

            IEnumerable<(Brep brep, Guid id)> GetMatchedBreps(BrepFace _bFace, IEnumerable<(Brep brep, Guid id)> _apertureBreps)
            {
                //Check intersection, maybe provide an option for use to split window surfaces for zones.
                //TODO: do this later

                var tol = 0.001;
                var srfBBox = _bFace.GetBoundingBox(false);
                var intersected = _apertureBreps.Where(_ => _.brep.GetBoundingBox(false).isIntersected(srfBBox, tol));
                var coplanared = intersected.Where(_ => _.brep.Faces[0].IsCoplanar(_bFace, tol));
                //TODO: Check if inside

                return coplanared;
            }

        }
        public static (Brep room, Guid roomId, List<(Brep brep, Guid id)> apertures) AddAperture(this ObjRef roomObjRef, ObjRef apertureObjRef) 
        {
            return roomObjRef.AddApertures(new List<ObjRef>() { apertureObjRef });
        }









//        public static (Brep room, Brep aperture) AddAperture(this ObjRef roomObjRef, ObjRef apertureObjRef)
//        {
//            Brep validApertureBrep = null;
//            //var tol = 0.0001;
//            var roomBrep = roomObjRef.Brep();
//            var apertureObject = apertureObjRef.Brep();
//            var apertureHostID = apertureObjRef.ObjectId;

        //            //Only add to valid room obj
        //            if (!roomBrep.IsRoom())
        //                throw new ArgumentException("Cannot assign aperture to non-room object!");

        //            //Check aperture surface
        //            var apertureBrep = apertureObject.DuplicateBrep();
        //            apertureBrep.Faces.ShrinkFaces();
        //            //TODO: make sure this apertureBrep is single surface
        //            var apertureBBox = apertureBrep.GetBoundingBox(false);

        //            //Get room surfaces
        //            var dupRoom = roomBrep.DuplicateBrep();
        //            var isRoomContainsAperture = dupRoom.GetBoundingBox(false).isIntersected(apertureBBox, 0.001);
        //            if (!isRoomContainsAperture)
        //                return (null, null);

        //            //Check with room surface contains this aperture.
        //            var roomSrfs = dupRoom.Faces;
        //            foreach (var roomSrf in roomSrfs)
        //            {
        //                var srfBBox = roomSrf.GetBoundingBox(false);
        //                //TODO: need to test following method performance
        //                var isInside = srfBBox.Contains(apertureBBox, false);
        //                var isIntersected = srfBBox.Contains(apertureBBox.Max, false) || srfBBox.Contains(apertureBBox.Min, false);
        //                var isCoPlanner = roomSrf.IsCoplanar(apertureBrep.Faces[0], 0.001, true, true);
        //                //TODO: need to take care of is not inside but isIntersected.
        //                if (!isCoPlanner)
        //                    continue;

        //                //Convert to Aperture Brep, and replace current rhino object
        //                validApertureBrep = apertureBrep.ToApertureBrep(apertureHostID);

        //                //add to room face brep
        //                var roomSrfEnt = roomSrf.TryGetFaceEntity();
        //                roomSrfEnt.AddAperture(new ObjRef(apertureHostID), validApertureBrep);

        //                //add to groupEntity.
        //                var groupEntity = dupRoom.TryGetGroupEntity(HoneybeeRhinoPlugIn.Instance.GroupEntityTable);

        //                //this shouldn't be happening, because all honeybee room must have to be part of a group entity.
        //                if (!groupEntity.IsValid)
        //                    throw new ArgumentException("Failed to get valid group entity from room!");

        //                groupEntity.AddApertures(new (Brep, ObjRef)[] {(validApertureBrep, new ObjRef(apertureHostID)) });
        //            }


        //#if DEBUG
        //            if (!dupRoom.Surfaces.Where(_ => _.TryGetFaceEntity().ApertureObjRefs.Any()).Any())
        //                throw new ArgumentException("some thing wrong with assigning aperture!");

        //            //ensure aperture's id has been added to group
        //            if (validApertureBrep.TryGetApertureEntity().GroupEntityID == Guid.Empty)
        //                throw new ArgumentException("some thing wrong with assigning aperture!");

        //            //ensure aperture's hostId is valid
        //            if (validApertureBrep.TryGetApertureEntity().HostObjRef == null)
        //                throw new ArgumentException("some thing wrong with assigning aperture!");

        //#endif

        //            return (dupRoom, validApertureBrep);
        //        }



    }
}
