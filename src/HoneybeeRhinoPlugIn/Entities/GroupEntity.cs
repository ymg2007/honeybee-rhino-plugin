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
        public BrepObject Room { get; private set; }
        public List<BrepObject> Apertures { get; private set; } = new List<BrepObject>();
        public List<BrepObject> Shades { get; private set; } = new List<BrepObject>();

        public Guid Guid => Room.Id;
        public GroupEntity() { }

        public GroupEntity(BrepObject room) 
        {
            this.Room = room;
        }
        //public GroupEntity(Guid roomId)
        //{
        //    var obj = RhinoDoc.ActiveDoc.Objects.FindId(roomId);

        //    this.Room = obj as BrepObject;
        //}


        public void AddToDocument(GroupEntityTable documentGroupEntityTable)
        {
            var table = documentGroupEntityTable;
            var exist = table.Keys.Any(_ => _ == this.Guid);
            if (!exist)
            {
                table.Add(this.Guid, this);
            }
            else
            {
                //TODO: maybe need to clear all child ids.
                throw new NotImplementedException();
            }
        }


        public bool IsValid
        {
            get 
            {
                if (this.Room == null)
                    return false;

                return this.Room.IsValid; 
            }
        }
        /// <summary>
        /// Use this method to update honeybee geometry based on its rhino object holder.
        /// </summary>
        public HB.Room GetCompleteHBRoom()
        {
            var doc = RhinoDoc.ActiveDoc.Objects;

            //Get room 
            //Get read rhino brep
            var roomObj = this.Room;
            if (roomObj == null)
                throw new ArgumentNullException("Room object has been deleted!, this group entity is not valid");

            var roomEnt = roomObj.TryGetRoomEntity();

            //Update room geometry
            var room = roomEnt.GetHBRoom(recomputeGeometry:true);
           
            //TODO: get apertures
            //TODO: add apertures to room.
            //TODO: get shades
            //TODO: add shades to room.

            return room;

        }

        protected override void OnDuplicate(UserData source)
        {
            throw new ArgumentException("this shouldn't happen as this entity is not saved under any geometry.");
      
        }

        public int ApertureCount => this.Apertures.Count;

        public void AddApertures(IEnumerable<BrepObject> apertureObjs)
        {
            //var docObjs = RhinoDoc.ActiveDoc.Objects;
            foreach (var aperture in apertureObjs)
            {
                var aptEnt = aperture.TryGetApertureEntity();
                if (!aptEnt.IsValid)
                    throw new ArgumentException("Some input geometries are not valid aperture object!");

                aptEnt.GroupEntityID = this.Room.Id;
                this.Apertures.Add(aperture);
            }

        }


        //========================= Select and highlight ========================
        #region Select and highlight

        public bool SelectRoom() => SelectHighlight(new BrepObject[] { this.Room });

        public bool SelectApertures() => SelectHighlight(this.Apertures);

        public bool SelectShades() => SelectHighlight(this.Shades);

        public bool SelectEntireEntity()
        {
            return this.SelectRoom() &&
                this.SelectApertures() &&
                this.SelectShades();
        }

        private bool SelectHighlight(IEnumerable<BrepObject> brepObjects)
        {
            var rc = true;
            foreach (var item in brepObjects)
            {
                //Check if object is visible or locked. deleted
                var obj = item;
                if (obj.IsValid || obj.IsLocked || obj.IsDeleted || obj.IsHidden)
                    continue;

                //the entire object (including subobjects) is already selected
                //Do nothing
                if (obj.IsSelected(false) == 2)
                    continue;

                //Select and highlight obj
                rc = rc && RhinoDoc.ActiveDoc.Objects.Select(item.Id, true, true);

            }
            return rc;
        }
        #endregion

        //========================= Read/Write ==================================
        #region Read/Write
        public override bool ShouldWrite => this.IsValid;


        protected override bool Read(BinaryArchiveReader archive)
        {
            return this.ReadArchive(archive);
        }
        protected override bool Write(BinaryArchiveWriter archive)
        {
            return WriteToArchive(archive);
        }

        public bool ReadArchive(BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out var major, out var minor);
            if (major == 1 && minor == 0)
            {
                var dic = archive.ReadDictionary();
                Deserialize(dic);
            }
            return !archive.ReadErrorOccured;
        }

        public bool WriteToArchive(BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);

            var dic = Serialize();
            archive.WriteDictionary(dic);
            return !archive.WriteErrorOccured;
        }

        private ArchivableDictionary Serialize()
        {
            var dic = new ArchivableDictionary();
            dic.Set(nameof(this.Room), new ObjRef(Room));
            dic.Set(nameof(this.Apertures), this.Apertures.Select(_ => new ObjRef(_)));
            dic.Set(nameof(this.Shades), this.Shades.Select(_ => new ObjRef(_)));
            return dic;
        }

        private void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            this.Room = (dic[nameof(this.Room)] as ObjRef).Object() as BrepObject;
            this.Apertures = (dic[nameof(this.Apertures)] as IEnumerable<ObjRef>).Select(_ => _.Object() as BrepObject).ToList();
            this.Shades = (dic[nameof(this.Shades)] as IEnumerable<ObjRef>).Select(_ => _.Object() as BrepObject).ToList();
        }
        #endregion


        //========================= Helpers ===================================

        //public static GroupEntity SetToRoom(RhinoObject roomRhinoObject)
        //{
        //    var ent = new GroupEntity(roomRhinoObject);
        //    var guid = roomRhinoObject.Id;
        //    roomRhinoObject.Geometry.UserDictionary.Set("HBGroupEntity", guid);
        //    HoneybeeRhinoPlugIn.Instance.GroupEntityTable.Add(guid, ent);
        //    return ent;
        //}

        public static GroupEntity TryGetFromID(Guid roomID, GroupEntityTable groupEntityTable)
        {
            GroupEntity rc = new GroupEntity();
            var found = groupEntityTable.TryGetValue(roomID, out GroupEntity ent);
            return found ? ent : rc;
        }

        public static GroupEntity TryGetFrom(GeometryBase obj, GroupEntityTable groupEntityTable)
        {
            GroupEntity rc = new GroupEntity();
            if (obj == null)
                return rc;

            Guid groupEntityId = Guid.Empty;

            if (obj.IsRoom())
            {
                var roomEnt = RoomEntity.TryGetFrom(obj);
                if (!roomEnt.IsValid)
                    return rc;

                groupEntityId = roomEnt.HostRhinoObject.Id;

                //TODO: check if this saved Id == obj.GeometryID
            }
            else if (obj.IsAperture())
            {
                var roomEnt = ApertureEntity.TryGetFrom(obj);
                if (!roomEnt.IsValid)
                    return rc;

                groupEntityId = roomEnt.GroupEntityID;
                //get aperture entity here
            }


            var entt = HBObjEntity.TryGetFrom(obj);

          
            //if object is copied, this saved Entity ID will not be valid.
            var found = groupEntityTable.TryGetValue(groupEntityId, out GroupEntity ent);
            return found ? ent : rc;


            
        }
       

    }
}
