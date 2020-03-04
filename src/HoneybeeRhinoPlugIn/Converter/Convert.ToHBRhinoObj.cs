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
        public static ObjRef ToRoomBrepObj(this ObjRef roomBrepObj, Func<RH.Brep, bool> objectReplaceFunc, double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
            var roomEnt = new Entities.RoomEntity(roomBrepObj, objectReplaceFunc);

            return roomEnt.HostObjRef;
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

        public static RH.Brep ToApertureBrep(this RH.GeometryBase apertureGeo, Guid hostID)
        {
            var geo = Rhino.Geometry.Brep.TryConvertBrep(apertureGeo);

            var faces = geo.Faces;
            if (faces.Count ==1 && faces.First().UnderlyingSurface().IsPlanar())
            {
                var hbobj = faces.First().ToAperture(hostID);
                var ent = new Entities.ApertureEntity(hbobj);
                ent.HostObjRef = new ObjRef( hostID);
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
