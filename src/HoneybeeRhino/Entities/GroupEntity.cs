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

        public GroupEntity(Guid roomRhinoID)
        {
            this.RoomID = roomRhinoID;
        }
        public GroupEntity(ObjRef roomRhinoObjRef): this(roomRhinoObjRef.ObjectId)
        {
        }

        public GroupEntity(RhinoObject roomRhinoObj) : this(roomRhinoObj.Id)
        {
        }


        public List<Guid> ApertureIDs { get; set; } = new List<Guid>();

        public List<Guid> ShadeIDs { get; set; } = new List<Guid>();

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


        public override bool ShouldWrite => this.IsValid;

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
                var obj = RhinoDoc.ActiveDoc.Objects.Find(item);
                if (obj.IsSelected(checkSubObjects:false) == 2)
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


        protected override bool Read(BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out var major, out var minor);
            if (major == 1 &&  minor == 0)
            {
                var dic = archive.ReadDictionary();
                Deserialize(dic);
            }
            return !archive.ReadErrorOccured;
        }
        protected override bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
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

        public static GroupEntity TryGet(RhinoObject obj)
        {
            GroupEntity rc = null;
            if (obj == null)
                return rc;

            var objData = obj.Geometry.UserData;
            rc = objData.Find(typeof(GroupEntity)) as GroupEntity;

            return rc;
        }
        /// <summary>
        /// This removes the old GroupEntity from the host object, 
        /// and creates a new one to attach to the host object.
        /// Returns the new Entity if host object is a valid honeybee object. Otherwise, returns null
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>GroupEntity</returns>
        public static GroupEntity RenewGroupEntity(RhinoObject obj)
        {
            GroupEntity rc = TryGet(obj);

            if (rc == null)
                return rc;

            var objData = obj.Geometry.UserData;
            objData.Remove(rc);
            //TODO: maybe Dispose it??
            //rc.Dispose();

            var newEnt = new GroupEntity(obj);
            objData.Add(newEnt);

            return newEnt;

        }
        public static GroupEntity GetFromRhinoObject(RhinoObject obj)
        {
            GroupEntity rc = TryGet(obj);

            if (rc == null)
                return rc;

            //Only return the entity associated with obj, 
            //sometime this is not true when entity is duplicated alone with obj, but not updated!
            if (rc.RoomID == obj.Id)
            {
                return rc;
            }
            else
            {
                //TODO: flag the entity in obj to make it invalid.
                //Or remove it, since there is no way to track children's ids
                obj.Geometry.UserData.Remove(rc);

                //TODO: maybe Dispose it??
                //rc.Dispose();

            }
            return rc;

        }



    }
}
