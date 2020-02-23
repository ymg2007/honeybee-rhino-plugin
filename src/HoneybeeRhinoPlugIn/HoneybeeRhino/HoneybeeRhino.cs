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
        public static (Brep room, Brep aperture) AddAperture(this Brep roomBrep, (Brep Brep, Guid HostID) apertureObject)
        {
            Brep validApertureBrep = null;
            var tol = 0.0001;

            //Only add to valid room obj
            if (!roomBrep.TryGetRoomEntity().IsValid)
                throw new ArgumentException("Cannot assign aperture to non-room object!");

            //Check aperture surface
            var apertureBrep = apertureObject.Brep.DuplicateBrep();
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
                validApertureBrep = apertureBrep.ToApertureBrep(apertureObject.HostID);
                //add to room face brep
                var roomSrfEnt = roomSrf.TryGetFaceEntity();
                roomSrfEnt.AddAperture(validApertureBrep);
           
            }

#if DEBUG
            if (!dupRoom.Surfaces.Where(_ => _.TryGetFaceEntity().Apertures.Any()).Any())
                throw new ArgumentException("some thing wrong with assigning aperture!");
#endif

            return (dupRoom, validApertureBrep);
        }
    }
}
