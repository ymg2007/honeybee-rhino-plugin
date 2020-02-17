using HoneybeeRhino;
using HoneybeeRhino.Entities;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB = HoneybeeSchema;
using RH = Rhino.Geometry;
using RHDoc = Rhino.DocObjects;

namespace HoneybeeRhino
{
    public static partial class Convert
    {

        public static RH.Brep ToRoomBrep(this RH.GeometryBase roomGeo, Guid hostID, GroupEntityTable documentGroupEntityTable,  double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
            //var geo = roomGeo;
            var brep = RH.Brep.TryConvertBrep(roomGeo);
            if (brep != null)
            {
                return brep.ToRoomBrep(hostID, documentGroupEntityTable, maxRoofFloorAngle, tolerance);
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");
            }

        }

        private static RH.Brep ToRoomBrep(this RH.Brep closedBrep, Guid hostID, GroupEntityTable documentGroupEntityTable, double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
            if (closedBrep.IsSolid)
            {
                var dupBrep = closedBrep.ToAllPlaneBrep(tolerance);
                var subFaces = dupBrep.Faces;
                subFaces.ShrinkFaces();

                var hbFaces = subFaces.Select(_ => _.ToHBFace(maxRoofFloorAngle)).ToList();
                //var testEnt = new Entities.FaceEntity(hbFaces[0]);

                for (int i = 0; i < hbFaces.Count; i++)
                {
                    var faceEnt = new Entities.FaceEntity(hbFaces[i]);
                    var bFace = dupBrep.Surfaces[i];
                    bFace.UserData.Add(faceEnt);
                }

                var room = new HB.Room($"Room_{Guid.NewGuid()}".ToString(), hbFaces, new HB.RoomPropertiesAbridged());
                var roomEnt = new Entities.RoomEntity(room, hostID, documentGroupEntityTable);
                dupBrep.UserData.Add(roomEnt);

                return dupBrep;

            }
            else
            {
                throw new ArgumentException("This rhino object is not a water-tight solid!");
            }

        }
        //public static RH.GeometryBase ToRoomGeo(this RhinoObject roomObj, double maxRoofFloorAngle = 30)
        //{
        //    var id = roomObj.Id;
        //    var geo = roomObj.Geometry.ToRoomGeo(id, maxRoofFloorAngle);
        //    return geo;

        //}

        //public static RhinoObject ToApertureObj(this RhinoObject apertureObj)
        //{
        //    apertureObj.Geometry.ToApertureGeo(apertureObj.Id);
        //    return apertureObj;
        //}

        public static RH.Brep ToApertureGeo(this RH.GeometryBase apertureGeo, Guid hostID)
        {
            var geo = Rhino.Geometry.Brep.TryConvertBrep(apertureGeo);
           
            if (geo.IsSurface && geo.Faces.First().UnderlyingSurface().IsPlanar())
            {
                //TODO: shinkFace
                var hbobj = geo.Faces.First().UnderlyingSurface().ToAperture();
                var ent = new Entities.ApertureEntity(hbobj);
                ent.HostGeoID = hostID;
                geo.UserData.Add(ent);
                return geo;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid planar object to convert to honeybee aperture!");
            }

        }

    }
}
