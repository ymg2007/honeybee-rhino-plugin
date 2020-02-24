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
        public static (Brep room, Brep aperture) AddAperture(this ObjRef roomObjRef, ObjRef apertureObjRef)
        {
            Brep validApertureBrep = null;
            var tol = 0.0001;
            var roomBrep = roomObjRef.Brep();
            var apertureObject = apertureObjRef.Brep();
            var apertureHostID = apertureObjRef.ObjectId;

            //Only add to valid room obj
            if (!roomBrep.IsRoom())
                throw new ArgumentException("Cannot assign aperture to non-room object!");

            //Check aperture surface
            var apertureBrep = apertureObject.DuplicateBrep();
            apertureBrep.Faces.ShrinkFaces();
            //TODO: make sure this apertureBrep is single surface
            var apertureSrf = apertureBrep.Surfaces.First() as PlaneSurface;
            apertureSrf.TryGetPlane(out Plane aptPlane);
            var apertureBBox = apertureBrep.GetBoundingBox(false);

            //Get room surfaces
            var dupRoom = roomBrep.DuplicateBrep();
            var isRoomContainsAperture = dupRoom.GetBoundingBox(false).Contains(apertureBBox);
            if (!isRoomContainsAperture)
                return (null, null);

            //Check with room surface contains this aperture.
            var roomSrfs = dupRoom.Surfaces;
            foreach (var roomSrf in roomSrfs)
            {
                var srfBBox = roomSrf.GetBoundingBox(false);
                //TODO: need to test following method performance
                var isInside = srfBBox.Contains(apertureBBox, false);
                var isIntersected = srfBBox.Contains(apertureBBox.Max, false) || srfBBox.Contains(apertureBBox.Min, false);
                var isCoPlanner = roomSrf.IsCoplanar(apertureSrf, tol, true, true);
                if (!isInside)
                    continue;

                //Convert to Aperture Brep
                validApertureBrep = apertureBrep.ToApertureBrep(apertureHostID);
                //add to room face brep
                var roomSrfEnt = roomSrf.TryGetFaceEntity();
                roomSrfEnt.AddAperture(validApertureBrep);

                //add to groupEntity.
                var groupEntity = dupRoom.TryGetGroupEntity(HoneybeeRhinoPlugIn.Instance.GroupEntityTable);

                //this shouldn't be happening, because all honeybee room must have to be part of a group entity.
                if (!groupEntity.IsValid)
                    throw new ArgumentException("Failed to get valid group entity from room!");

                groupEntity.AddApertures(new (Brep, ObjRef)[] {(validApertureBrep, new ObjRef(apertureHostID)) });
            }


#if DEBUG
            if (!dupRoom.Surfaces.Where(_ => _.TryGetFaceEntity().Apertures.Any()).Any())
                throw new ArgumentException("some thing wrong with assigning aperture!");

            //ensure aperture's id has been added to group
            if (validApertureBrep.TryGetApertureEntity().GroupEntityID == Guid.Empty)
                throw new ArgumentException("some thing wrong with assigning aperture!");

            //ensure aperture's hostId is valid
            if (validApertureBrep.TryGetApertureEntity().HostObjRef == null)
                throw new ArgumentException("some thing wrong with assigning aperture!");

#endif

            return (dupRoom, validApertureBrep);
        }
    }
}
