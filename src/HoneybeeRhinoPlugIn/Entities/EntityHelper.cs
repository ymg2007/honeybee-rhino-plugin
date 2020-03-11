using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneybeeRhino.Entities
{
    public static class EntityHelper
    {
        public static bool IsRoom(this ObjRef rhinoRef) => rhinoRef.Geometry().IsRoom();
        public static bool IsRoom(this GeometryBase geometry) => geometry.TryGetRoomEntity().IsValid;

        public static bool IsAperture(this ObjRef rhinoRef) => rhinoRef.Geometry().IsAperture();
        public static bool IsAperture(this GeometryBase geometry) => ApertureEntity.TryGetFrom(geometry).IsValid;

        public static bool IsDoor(this ObjRef rhinoRef) => rhinoRef.Geometry().IsDoor();
        public static bool IsDoor(this GeometryBase geometry) => DoorEntity.TryGetFrom(geometry).IsValid;

        public static RoomEntity TryGetRoomEntity(this ObjRef rhinoRef) => RoomEntity.TryGetFrom(rhinoRef.Geometry());
        public static RoomEntity TryGetRoomEntity(this GeometryBase rhinoRef) => RoomEntity.TryGetFrom(rhinoRef);

        public static ApertureEntity TryGetApertureEntity(this ObjRef rhinoRef) => ApertureEntity.TryGetFrom(rhinoRef.Geometry());
        public static ApertureEntity TryGetApertureEntity(this GeometryBase rhinoRef) => ApertureEntity.TryGetFrom(rhinoRef);

        public static DoorEntity TryGetDoorEntity(this ObjRef rhinoRef) => DoorEntity.TryGetFrom(rhinoRef.Geometry());
        public static DoorEntity TryGetDoorEntity(this GeometryBase rhinoRef) => DoorEntity.TryGetFrom(rhinoRef);

        public static ShadeEntity TryGetShadeEntity(this ObjRef rhinoRef) => ShadeEntity.TryGetFrom(rhinoRef.Geometry());
        public static ShadeEntity TryGetShadeEntity(this GeometryBase rhinoRef) => ShadeEntity.TryGetFrom(rhinoRef);

        public static FaceEntity TryGetOrphanedFaceEntity(this ObjRef roomHostRef) => FaceEntity.TryGetFrom(roomHostRef.Geometry());
        public static FaceEntity TryGetFaceEntity(this ObjRef roomHostRef, ComponentIndex componentIndex) => FaceEntity.TryGetFrom(roomHostRef, componentIndex);
        public static FaceEntity TryGetFaceEntity(this BrepFace rhinoRef) => FaceEntity.TryGetFrom(rhinoRef);
        public static FaceEntity TryGetFaceEntity(this Surface rhinoRef) => FaceEntity.TryGetFrom(rhinoRef);

        public static HBObjEntity TryGetHBObjEntity(this GeometryBase rhinoRef) => HBObjEntity.TryGetFrom(rhinoRef);

        //public static string GetHBJson(this GeometryBase geometry)
        //{
        //    var isHB = geometry.UserDictionary.TryGetString("HBData", out string json);
        //    if (isHB)
        //    {
        //        return json;
        //    }
        //    else
        //    {
        //        throw new ArgumentException("This is not a valid Honeybee geometery!");
        //    }
        //}

        public static bool HasHBObjEntity(this GeometryBase geometry)
        {
            var ent = Entities.HBObjEntity.TryGetFrom(geometry);
            return ent != null;
        }
   
        //public static bool HasGroupEntity(this RhinoObject rhinoRef)
        //{
        //    var ent = Entities.GroupEntity.TryGetFrom(rhinoRef.Geometry);
        //    return ent.IsValid;
        //}
        public static Brep DetachHBEntityTo(this Brep honeybeeObj, Dictionary<Guid, HBObjEntity> tempEntityHolder)
        {
            var hbObj = honeybeeObj.DuplicateBrep();
            if (hbObj.IsRoom())
            {
                var ent = hbObj.TryGetRoomEntity();
                hbObj.UserData.Remove(ent);
                var guid = Guid.NewGuid();
                hbObj.SetUserString("HBDataID", guid.ToString());
                tempEntityHolder.Add(guid, ent);


                foreach (var srf in hbObj.Surfaces)
                {
                    var srfEnt = srf.TryGetFaceEntity();
                    srf.UserData.Remove(srfEnt);
                    var srfGuid = Guid.NewGuid();
                    //Do not add to UserDictionary it would crash Rhino.
                    srf.SetUserString("HBDataID", srfGuid.ToString());
                    tempEntityHolder.Add(srfGuid, srfEnt);

                }
            }
            else if (hbObj.IsAperture())
            {
                //TODO:
            }
            return hbObj;

        }
        public static Brep ReinstallHBEntityFrom(this Brep hbObj, Dictionary<Guid, HBObjEntity> tempEntityHolder)
        {
            if (hbObj == null)
                return hbObj;

            var honeybeeObj = hbObj.DuplicateBrep();
            if (honeybeeObj.Faces.Count > 1 && honeybeeObj.IsSolid)
            {
                //This is a room
                var data = honeybeeObj.GetUserString("HBDataID");
                if (string.IsNullOrEmpty(data))
                    return honeybeeObj;

                var guid = Guid.Parse(data);
                var found = tempEntityHolder.TryGetValue(guid, out Entities.HBObjEntity entity);
                if (!found)
                    return honeybeeObj;
                if (entity is Entities.RoomEntity roomEnt)
                {
                    var dup = new Entities.RoomEntity();
                    dup.Duplicate(roomEnt);
                    honeybeeObj.UserData.Add(dup);
                }

                //Now add subSurfaces
                foreach (var srf in honeybeeObj.Surfaces)
                {
                    var srfdata = srf.GetUserString("HBDataID");
                    if (string.IsNullOrEmpty(srfdata))
                        throw new ArgumentException("Lost Honeybee data after last step!");

                    var srfguid = Guid.Parse(srfdata);
                    var srffound = tempEntityHolder.TryGetValue(srfguid, out Entities.HBObjEntity faceentity);
                    if (!srffound)
                        throw new ArgumentException("This shouldn't be happening, but still lost Honeybee face data after last step!");
                    if (faceentity is Entities.FaceEntity faceEnt)
                    {
                        var dup = new Entities.FaceEntity();
                        dup.Duplicate(faceEnt);
                        srf.UserData.Add(dup);
                    }
                    //clean temp tag for face
                    srf.GetUserStrings().Remove("HBDataID");
                }
                //clean temp tag for room
                honeybeeObj.GetUserStrings().Remove("HBDataID");
                return honeybeeObj;

            }
            else if (honeybeeObj.Faces.Count == 1)
            {
                //TODO: Aperture;
                //Might never be used. 
                return honeybeeObj;
            }
            else
            {
                //TODO: Shading
                //Might never be used. 
                return honeybeeObj;
            }

        }
        public static Brep DeleteHBEntity(this Brep honeybeeObj, bool duplicate = true)
        {
            var hbObj = duplicate? honeybeeObj.DuplicateBrep(): honeybeeObj;
            if (hbObj.IsRoom())
            {
                //Clean Room
                var ent = hbObj.TryGetRoomEntity();
                hbObj.UserData.Remove(ent);
                //clean Faces
                foreach (var srf in hbObj.Surfaces)
                {
                    var srfEnt = srf.TryGetFaceEntity();
               
                    //Clean Apertures
                    foreach (var apt in srfEnt.ApertureObjRefs)
                    {
                        apt.Brep().DeleteHBEntity();
                    }

                    //TODO: SHDs

                    srf.UserData.Remove(srfEnt);
                }
            }
            else if (hbObj.IsAperture())
            {
                var ent = hbObj.TryGetApertureEntity();
                hbObj.UserData.Remove(ent);
            }
            //TODO: shades
            return hbObj;

        }


        //ObjRef to ObjRef
        public static Brep ToRoomBrepObj(ObjRef roomBrepObj, double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
            //check if Null, valid, solid
            if (!CheckIfBrepObjectValid(roomBrepObj))
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");

            //create new room
            //Create honeybee room object here.
            var closedBrep = roomBrepObj.Brep().DuplicateBrep();
            var dupBrep = closedBrep.ToAllPlaneBrep(tolerance);
            var subFaces = dupBrep.Faces;

            var hbFaces = subFaces.Select(_ => _.ToHBFace(maxRoofFloorAngle)).ToList();

            for (int i = 0; i < hbFaces.Count; i++)
            {
                var faceEnt = new FaceEntity(hbFaces[i]);
                var bFace = dupBrep.Surfaces[i];
                bFace.UserData.Add(faceEnt);
            }

            var id = roomBrepObj.ObjectId;
            var newObjRef = new ObjRef(id);
            var room = new HoneybeeSchema.Room($"Room_{id}", hbFaces, new HoneybeeSchema.RoomPropertiesAbridged());
            room.DisplayName = $"My Room {id.ToString().Substring(0, 5)}";
            var ent = new RoomEntity(room, newObjRef);

            //Add this RoomEntity to brep's userdata at the end.
            dupBrep.UserData.Add(ent);


#if DEBUG

            if (!dupBrep.TryGetRoomEntity().IsValid)
                throw new ArgumentException("Failed to convert to honeybee room!");

#endif

            return dupBrep;


            //Local method
            bool CheckIfBrepObjectValid(ObjRef roomObj)
            {
                if (roomObj == null)
                    throw new NullReferenceException();

                var brep = roomObj.Brep();
                if (brep == null)
                    throw new NullReferenceException();
                if (!brep.IsValid)
                    throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");

                if (!brep.IsSolid)
                    throw new ArgumentException("This rhino object is not a water-tight solid!");

                brep.DeleteHBEntity(duplicate: false);

                return true;
            }



        }

        public static Brep ToApertureBrep(GeometryBase apertureGeo, Guid hostID)
        {
            var geo = Rhino.Geometry.Brep.TryConvertBrep(apertureGeo);

            var faces = geo.Faces;
            if (faces.Count == 1 && faces.First().UnderlyingSurface().IsPlanar())
            {
                var hbobj = faces.First().ToAperture(hostID);
                var ent = new Entities.ApertureEntity(hbobj);
                ent.HostObjRef = new ObjRef(hostID);
                geo.UserData.Add(ent);
                return geo;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid planar object to convert to honeybee aperture!");
            }

        }

        public static Brep ToDoorBrep(GeometryBase apertureGeo, Guid hostID)
        {
            var geo = Rhino.Geometry.Brep.TryConvertBrep(apertureGeo);

            var faces = geo.Faces;
            if (faces.Count == 1 && faces.First().UnderlyingSurface().IsPlanar())
            {
                var hbobj = faces.First().ToDoor(hostID);
                var ent = new Entities.DoorEntity(hbobj);
                ent.HostObjRef = new ObjRef(hostID);
                geo.UserData.Add(ent);
                return geo;
            }
            else
            {
                throw new ArgumentException("Input geometry is not a valid planar object to convert to honeybee aperture!");
            }

        }

        public static Brep ToShadeBrep(Brep shades, Guid hostID)
        {
            var geo = shades;

            //for shade brep, it can have more than one faces in one brep
            var faces = geo.Faces;
            if (faces.Any(_=>!_.IsPlanar()))
                throw new ArgumentException("Input geometry has non-planar object which cannot be converted to honeybee shade!");

            //Add honeybee data at Brep level, which assumes all faces has the same property.
            //add a dummy Face3D data 
            var hbobj = faces.First().ToShade(hostID); 
            var ent = new Entities.ShadeEntity(hbobj);
            ent.HostObjRef = new ObjRef(hostID);
            geo.UserData.Add(ent);
            return geo;
        }
    }
}
