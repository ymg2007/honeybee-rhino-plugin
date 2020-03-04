using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.DocObjects.Custom;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.FileIO;
using System.Runtime.InteropServices;
using Rhino.Collections;
using Rhino.Geometry;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    [Guid("B0508C74-707F-4D5C-B218-AEF3B4EEF06B")]
    public class GroupEntity : UserData
    {
        public ObjRef Room { get; private set; }
        public List<ObjRef> Apertures { get; private set; } = new List<ObjRef>();
        public List<ObjRef> Shades { get; private set; } = new List<ObjRef>();

        public Guid Guid => Room.ObjectId;
        
        public GroupEntity() { }

        public GroupEntity(ObjRef room) 
        {
            this.Room = room;
        }
        //public GroupEntity(Guid roomId)
        //{
        //    var obj = RhinoDoc.ActiveDoc.Objects.FindId(roomId);

        //    this.Room = obj as BrepObject;
        //}


        //public void AddToDocument(GroupEntityTable documentGroupEntityTable)
        //{
        //    var table = documentGroupEntityTable;
        //    var found = table.TryGetValue(this.Guid, out GroupEntity ent);
        //    var exist = table.Keys.Any(_ => _ == this.Guid);
        //    if (!found)
        //    {
        //        table.Add(this.Guid, this);
        //    }
        //    else
        //    {
            
        //        ent = new GroupEntity(this.Room);
        //        ent.Shades.Clear();
        //        ent.Apertures.Clear();
        //        table[this.Guid] = ent;

        //    }
        //}


        //public bool IsValid
        //{
        //    get 
        //    {
        //        if (this.Room == null)
        //            return false;
        //        if (this.Room.Object() == null)
        //            return false;
        //        if (this.Room.Object().IsDeleted)
        //            return false;
        //        return this.Room.Brep().IsValid; 
        //    }
        //}
        /// <summary>
        /// Use this method to update honeybee geometry based on its rhino object holder.
        ///// </summary>
        //public HB.Room GetCompleteHBRoom()
        //{
        //    var doc = RhinoDoc.ActiveDoc.Objects;

        //    //Get room 
        //    //Get read rhino brep
        //    var roomObj = this.Room.Brep();
        //    if (roomObj == null)
        //        throw new ArgumentNullException("Room object has been deleted!, this group entity is not valid");

        //    var roomEnt = roomObj.TryGetRoomEntity();

        //    //Update room geometry
        //    var room = roomEnt.GetHBRoom(recomputeGeometry:true);
           
        //    //TODO: get apertures
        //    //TODO: add apertures to room.
        //    //TODO: get shades
        //    //TODO: add shades to room.

        //    return room;

        //}

        protected override void OnDuplicate(UserData source)
        {
            throw new ArgumentException("this shouldn't happen as this entity is not saved under any geometry.");
      
        }

        //public int ApertureCount => this.Apertures.Count(_=>_.TryGetApertureEntity().IsValid);

        //public void AddApertures(IEnumerable<(Brep brep, ObjRef hostObj)> apertures)
        //{
        //    //var docObjs = RhinoDoc.ActiveDoc.Objects;

        //    foreach (var apt in apertures)
        //    {
        //        //var aperture = docObjs.FindId(id) as BrepObject;
        //        var aptEnt = apt.brep.TryGetApertureEntity();
        //        if (!aptEnt.IsValid)
        //            throw new ArgumentException("Some input geometries are not valid aperture object!");

        //        aptEnt.GroupEntityID = this.Room.ObjectId;
        //        this.Apertures.Add(apt.hostObj);
        //    }

        //}
        public void AddApertures(IEnumerable<ObjRef> apertures)
        {
            foreach (var apt in apertures)
            {
                var aptEnt = apt.TryGetApertureEntity();
                if (!aptEnt.IsValid)
                    throw new ArgumentException("Some input geometries are not valid aperture object!");

                aptEnt.GroupEntityID = this.Room.ObjectId;
                this.Apertures.Add(apt);
            }

        }

        ////========================= Select and highlight ========================
        //#region Select and highlight

        //public bool SelectRoom() => SelectHighlight(new ObjRef[] { this.Room });

        //public bool SelectApertures() => SelectHighlight(this.Apertures);

        //public bool SelectShades() => SelectHighlight(this.Shades);

        //public bool SelectEntireEntity()
        //{
        //    return this.SelectRoom() &&
        //        this.SelectApertures() &&
        //        this.SelectShades();
        //}

      
        //#endregion

        ////========================= Read/Write ==================================
        //#region Read/Write
        //public override bool ShouldWrite => this.IsValid;


        //protected override bool Read(BinaryArchiveReader archive)
        //{
        //    return this.ReadArchive(archive);
        //}
        //protected override bool Write(BinaryArchiveWriter archive)
        //{
        //    return WriteToArchive(archive);
        //}

        //public bool ReadArchive(BinaryArchiveReader archive)
        //{
        //    archive.Read3dmChunkVersion(out var major, out var minor);
        //    if (major == 1 && minor == 0)
        //    {
        //        var dic = archive.ReadDictionary();
        //        Deserialize(dic);
        //    }
        //    return !archive.ReadErrorOccured;
        //}

        //public bool WriteToArchive(BinaryArchiveWriter archive)
        //{
        //    archive.Write3dmChunkVersion(1, 0);

        //    var dic = Serialize();
        //    archive.WriteDictionary(dic);
        //    return !archive.WriteErrorOccured;
        //}

        //private ArchivableDictionary Serialize()
        //{
        //    var dic = new ArchivableDictionary();
        //    dic.Set(nameof(this.Room), Room);
        //    dic.Set(nameof(this.Apertures), this.Apertures);
        //    dic.Set(nameof(this.Shades), this.Shades);
        //    return dic;
        //}

        //private void Deserialize(ArchivableDictionary dictionary)
        //{
        //    var dic = dictionary;
        //    this.Room = dic[nameof(this.Room)] as ObjRef;
        //    this.Apertures = (dic[nameof(this.Apertures)] as IEnumerable<ObjRef>).ToList();
        //    this.Shades = (dic[nameof(this.Shades)] as IEnumerable<ObjRef>).ToList();
        //}
        //#endregion


        //========================= Helpers ===================================

        //public static GroupEntity SetToRoom(RhinoObject roomRhinoObject)
        //{
        //    var ent = new GroupEntity(roomRhinoObject);
        //    var guid = roomRhinoObject.Id;
        //    roomRhinoObject.Geometry.UserDictionary.Set("HBGroupEntity", guid);
        //    HoneybeeRhinoPlugIn.Instance.GroupEntityTable.Add(guid, ent);
        //    return ent;
        //}

        //public static GroupEntity TryGetFromID(Guid roomID, GroupEntityTable groupEntityTable)
        //{
        //    GroupEntity rc = new GroupEntity();
        //    var found = groupEntityTable.TryGetValue(roomID, out GroupEntity ent);
        //    return found ? ent : rc;
        //}

        //public static GroupEntity TryGetFrom(GeometryBase obj, GroupEntityTable groupEntityTable)
        //{
        //    GroupEntity rc = new GroupEntity();
        //    if (obj == null)
        //        return rc;

        //    Guid groupEntityId = Guid.Empty;

        //    if (obj.IsRoom())
        //    {
        //        var roomEnt = RoomEntity.TryGetFrom(obj);
        //        if (!roomEnt.IsValid)
        //            return rc;

        //        groupEntityId = roomEnt.HostObjRef.ObjectId;

        //        //TODO: check if this saved Id == obj.GeometryID
        //    }
        //    else if (obj.IsAperture())
        //    {
        //        var roomEnt = ApertureEntity.TryGetFrom(obj);
        //        if (!roomEnt.IsValid)
        //            return rc;

        //        groupEntityId = roomEnt.GroupEntityID;
        //        //get aperture entity here
        //    }


        //    var entt = HBObjEntity.TryGetFrom(obj);

          
        //    //if object is copied, this saved Entity ID will not be valid.
        //    var found = groupEntityTable.TryGetValue(groupEntityId, out GroupEntity ent);
        //    return found ? ent : rc;


            
        //}
       

    }
}
