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
    public class GroupEntity : BaseEntity
    {
        public Guid RoomID { get; private set; }
        public GroupEntity() { }

        
        private GroupEntity(Guid roomRhinoID)
        {
            this.RoomID = roomRhinoID;
        }
        public GroupEntity(ObjRef roomRhinoObjRef): this(roomRhinoObjRef.Object())
        {
        }

        public GroupEntity(RhinoObject roomRhinoObj) : this(roomRhinoObj.Id)
        {
            var guid = roomRhinoObj.Id;
            roomRhinoObj.Geometry.UserDictionary.Set("HBGroupEntity", guid);
            HoneybeeRhinoPlugIn.Instance.GroupEntityTable.Add(guid, this);
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

        public void AddApertures(IEnumerable<RhinoObject> apertureObjs)
        {
            //TODO: save group entity to document, instead of in geometries.
            //Rhino will remove UserData if it has been added to other geometry,
            //so we need to make a copy first for now.
            this.ApertureIDs.AddRange(apertureObjs.Select(_ => _.Id));

            foreach (var win in apertureObjs)
            {
                win.Geometry.UserDictionary.Set("HBGroupEntity", this.RoomID);
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
        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
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

        public bool WriteToArchive(Rhino.FileIO.BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);

            var dic = Serialize();
            archive.WriteDictionary(dic);
            return !archive.WriteErrorOccured;
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = new ArchivableDictionary();
            dic.Set(nameof(RoomID), RoomID);
            dic.Set(nameof(ApertureIDs), ApertureIDs);
            dic.Set(nameof(ShadeIDs), ShadeIDs);
            return dic;
        }

        private protected override void Deserialize(ArchivableDictionary dictionary)
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

  
        public static GroupEntity TryGet(RhinoObject obj)
        {
            GroupEntity rc = new GroupEntity();
            if (obj == null)
                return rc;

            var hasEntityID = obj.Geometry.UserDictionary.TryGetGuid("HBGroupEntity", out Guid guid);
            if (!hasEntityID)
                return rc;

            //if object is copied, this saved Entity ID will not be valid.
            var found = HoneybeeRhinoPlugIn.Instance.GroupEntityTable.TryGetValue(guid, out GroupEntity ent);
            return found ? ent : rc;

            //if (obj.IsRoom())
            //{
            //    var found = HoneybeeRhinoPlugIn.Instance.GroupEntityTable.TryGetValue(obj.Id, out GroupEntity ent);
            //    return found ? ent : rc;
            //}
            //else if (obj.IsAperture())
            //{
            //    var found = HoneybeeRhinoPlugIn.Instance.GroupEntityTable.TryGetValue(guid, out GroupEntity ent);
            //    return found ? ent : rc;

            //}
            //else
            //{
            //    //input geometry is not a valid honeybee object.
            //    //just return an empty group entity.
                
            //    return rc;
            //}
            

            
        }
       
        public static GroupEntity GetFromRhinoObject(RhinoObject obj)
        {
            GroupEntity rc = TryGet(obj);

            if (rc.IsValid)
            {
                return rc;
            }
            else
            {
                return new GroupEntity();
            }
                
        }


    }
}
