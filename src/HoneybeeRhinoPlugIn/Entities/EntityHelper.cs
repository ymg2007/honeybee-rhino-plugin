﻿using Rhino.DocObjects;
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

    }
}
