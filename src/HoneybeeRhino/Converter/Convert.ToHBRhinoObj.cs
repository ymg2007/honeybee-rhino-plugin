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
        public static RH.Brep ToRoomGeo(this RH.GeometryBase roomGeo, Guid hostID, double maxRoofFloorAngle = 30)
        {
            //var geo = roomGeo;
            var brep = RH.Brep.TryConvertBrep(roomGeo);
            if (brep != null)
            {
                var hbobj = brep.ToRoom();
                var ent = new Entities.RoomEntity(hbobj, hostID);
                brep.UserData.Add(ent);
                return brep;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");
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
