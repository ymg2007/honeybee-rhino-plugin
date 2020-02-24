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
        public static BrepObject ToRoomBrepObj(this BrepObject roomBrepObj, Func<RH.Brep, bool> objectReplaceFunc, GroupEntityTable documentGroupEntityTable, double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
           
            var roomEnt = new Entities.RoomEntity(roomBrepObj, objectReplaceFunc);

            //Create new Group Entity
            var ent = new GroupEntity(roomBrepObj);
            ent.AddToDocument(documentGroupEntityTable);
            
            return roomEnt.HostRhinoObject;
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
