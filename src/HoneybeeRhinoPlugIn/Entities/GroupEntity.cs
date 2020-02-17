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

namespace HoneybeeRhino.Entities
{
    [Guid("B0508C74-707F-4D5C-B218-AEF3B4EEF06B")]
    public class GroupEntity : UserData
    {
        public Guid RoomID { get; private set; } = Guid.Empty;
        public GroupEntity() { }

        public GroupEntity(Guid roomID) 
        {
            this.RoomID = roomID;
        }

        public void AddToDocument(GroupEntityTable documentGroupEntityTable)
        {
            var table = documentGroupEntityTable;
            var exist = table.Keys.Any(_ => _ == this.RoomID);
            if (!exist)
            {
                table.Add(this.RoomID, this);
            }
            else
            {
                //TODO: maybe need to clear all child ids.
            }
        }

        
        private List<Guid> ApertureIDs { get; set; } = new List<Guid>();

        public List<Guid> ShadeIDs { get; private set; } = new List<Guid>();

        public bool IsValid => RhinoDoc.ActiveDoc.Objects.FindId(this.RoomID) != null;


        protected override void OnDuplicate(UserData source)
        {
            var s = source as GroupEntity;
            if (s != null)
            {
                this.RoomID = s.RoomID;
                this.ApertureIDs = s.ApertureIDs.GetRange(0, s.ApertureIDs.Count);
                this.ShadeIDs = s.ShadeIDs.GetRange(0, s.ShadeIDs.Count);
            }
            
        }

        public int ApertureCount => this.ApertureIDs.Count;

        //public void AddApertures(IEnumerable<RhinoObject> apertureObjs)
        //{
        //    this.ApertureIDs.AddRange(apertureObjs.Select(_ => _.Id));

        //    foreach (var win in apertureObjs)
        //    {
        //        var ent = Entities.ApertureEntity.TryGetFrom(win.Geometry);
        //        ent.GroupEntityID = this.RoomID;
        //    }
        //}
        public void AddApertures(IEnumerable<Brep> apertureObjs)
        {
            foreach (var win in apertureObjs)
            {
                var ent = Entities.ApertureEntity.TryGetFrom(win);
                if (ent.IsValid)
                {
                    ent.GroupEntityID = this.RoomID;
                    this.ApertureIDs.Add(ent.HostGeoID);
                }
                else
                {
                    throw new ArgumentException("Some input geometries are not valid aperture object!");
                }
            }

        }
        public void AddApertures(IEnumerable<ApertureEntity> apertureEntities)
        {
            foreach (var ent in apertureEntities)
            {
                ent.GroupEntityID = this.RoomID;
                this.ApertureIDs.Add(ent.HostGeoID);
            }

        }


        //=========================== Select and highlight =================================

        public bool SelectRoom() => SelectByIDs(new Guid[] { this.RoomID });

        public bool SelectApertures() => SelectByIDs(this.ApertureIDs);

        public bool SelectShades() => SelectByIDs(this.ShadeIDs);

        public bool SelectEntireEntity()
        {
            return this.SelectRoom() &&
                this.SelectApertures() &&
                this.SelectShades();
        }

        private bool SelectByIDs(IEnumerable<Guid> guids)
        {
            var ids = guids;
            var rc = true;
            foreach (var item in ids)
            {
                //TODO: may need to check if object is visible or locked. deleted
                var obj = RhinoDoc.ActiveDoc.Objects.FindId(item);
                if (obj == null)
                    continue;

                //Object might have been deleted
                if (this.RoomID == item)
                {
                    
                }
                else if (!this.ApertureIDs.Any(_ => _ == item))
                {
                    this.ApertureIDs.Remove(item);
                }

                if (obj.IsSelected(checkSubObjects: false) == 2)
                {
                    //the entire object (including subobjects) is already selected
                    //Do nothing
                }
                else
                {
                    rc = rc && RhinoDoc.ActiveDoc.Objects.Select(item, true, true);
                }


            }
            return rc;
        }

        //========================= Read/Write ===================================

        public override bool ShouldWrite => this.IsValid;


        protected override bool Read(BinaryArchiveReader archive)
        {
            return ReadArchive(archive);
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
            dic.Set(nameof(RoomID), RoomID);
            dic.Set(nameof(ApertureIDs), ApertureIDs);
            dic.Set(nameof(ShadeIDs), ShadeIDs);
            return dic;
        }

        private void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            this.RoomID = dic.GetGuid(nameof(RoomID));
            this.ApertureIDs = (dic[nameof(ApertureIDs)] as IEnumerable<Guid>).ToList();
            this.ShadeIDs = (dic[nameof(ShadeIDs)] as IEnumerable<Guid>).ToList();
        }

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

                groupEntityId = roomEnt.GroupEntityID;

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
