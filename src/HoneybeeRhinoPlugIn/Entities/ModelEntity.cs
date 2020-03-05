using System.Collections.Generic;
using System.Linq;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Collections;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    public class ModelEntity
    {
        public string ModelNameID => this.HBObject.Name;
        public HB.Model HBObject { get; private set; }

        private List<ObjRef> _roomEntities = new List<ObjRef>();
        private List<ObjRef> _orphanedFaces = new List<ObjRef>();
        private List<ObjRef> _orphanedShades = new List<ObjRef>();
        private List<ObjRef> _orphanedApertures = new List<ObjRef>();
        private List<ObjRef> _orphanedDoors = new List<ObjRef>();

        public List<ObjRef> RoomEntities
        {
            get { return this._roomEntities.Where(_ => _.TryGetRoomEntity().IsValid).ToList(); }
            private set { _roomEntities = value; }
        }
        public List<ObjRef> OrphanedFaces
        {
            get { return this._orphanedFaces.Where(_ => _.TryGetOrphanedFaceEntity().IsValid).ToList(); }
            private set { _orphanedFaces = value; }
        }
        public List<ObjRef> OrphanedShades
        {
            //TODO: add validation later
            get { return this._orphanedShades; }
            private set { _orphanedShades = value; }
        }
        public List<ObjRef> OrphanedApertures
        {
            get { return this._orphanedApertures.Where(_ => _.TryGetApertureEntity().IsValid).ToList(); }
            private set { _orphanedApertures = value; }
        }
        public List<ObjRef> OrphanedDoors
        {
            //TODO: add validation later
            get { return this._orphanedDoors; }
            private set { _orphanedDoors = value; }
        }



        public ModelEntity() { }

        public ModelEntity(HB.Model hbObject) 
        {
            this.HBObject = hbObject;
        }

        public void AddToDocument(ModelEntityTable documentModelEntityTable)
        {
            var table = documentModelEntityTable;
            var exist = table.Keys.Any(_ => _ == this.ModelNameID);
            if (!exist)
            {
                table.Add(this.ModelNameID, this);
            }
            else
            {
                //TODO: maybe need to clear all child ids.
            }
        }


        public bool IsValid => true;

        public ModelEntity Duplicate()
        {
            var json = this.HBObject.ToJson();
            var model = HB.Model.FromJson(json);
            var newEnt = new ModelEntity(model);
            newEnt.RoomEntities = new List<ObjRef>(this.RoomEntities);
            newEnt.OrphanedFaces = new List<ObjRef>(this.OrphanedFaces);
            newEnt.OrphanedShades = new List<ObjRef>(this.OrphanedShades);
            newEnt.OrphanedApertures = new List<ObjRef>(this.OrphanedApertures);
            newEnt.OrphanedDoors = new List<ObjRef>(this.OrphanedDoors);

            return newEnt;
        }
        //protected override void OnDuplicate(UserData source)
        //{
        //    var s = source as GroupEntity;
        //    if (s != null)
        //    {
        //        this.RoomID = s.RoomID;
        //        this.ApertureIDs = s.ApertureIDs.GetRange(0, s.ApertureIDs.Count);
        //        this.ShadeIDs = s.ShadeIDs.GetRange(0, s.ShadeIDs.Count);
        //    }
            
        //}

        //public int ApertureCount => this.ApertureIDs.Count;

   
        //public void AddApertures(IEnumerable<Brep> apertureObjs)
        //{
        //    foreach (var win in apertureObjs)
        //    {
        //        var ent = Entities.ApertureEntity.TryGetFrom(win);
        //        if (ent.IsValid)
        //        {
        //            ent.GroupEntityID = this.RoomID;
        //            this.ApertureIDs.Add(ent.HostGeoID);
        //        }
        //        else
        //        {
        //            throw new ArgumentException("Some input geometries are not valid aperture object!");
        //        }
        //    }

        //}
        //public void AddApertures(IEnumerable<ApertureEntity> apertureEntities)
        //{
        //    foreach (var ent in apertureEntities)
        //    {
        //        ent.GroupEntityID = this.RoomID;
        //        this.ApertureIDs.Add(ent.HostGeoID);
        //    }

        //}


        //=========================== Select and highlight =================================

        //public bool SelectRoom() => SelectByIDs(new Guid[] { this.RoomID });

        //public bool SelectApertures() => SelectByIDs(this.ApertureIDs);

        //public bool SelectShades() => SelectByIDs(this.ShadeIDs);

        //public bool SelectEntireEntity()
        //{
        //    return this.SelectRoom() &&
        //        this.SelectApertures() &&
        //        this.SelectShades();
        //}

        //private bool SelectByIDs(IEnumerable<Guid> guids)
        //{
        //    var ids = guids;
        //    var rc = true;
        //    foreach (var item in ids)
        //    {
        //        //TODO: may need to check if object is visible or locked. deleted
        //        var obj = RhinoDoc.ActiveDoc.Objects.FindId(item);
        //        if (obj == null)
        //            continue;

        //        //Object might have been deleted
        //        if (this.RoomID == item)
        //        {
                    
        //        }
        //        else if (!this.ApertureIDs.Any(_ => _ == item))
        //        {
        //            this.ApertureIDs.Remove(item);
        //        }

        //        if (obj.IsSelected(checkSubObjects: false) == 2)
        //        {
        //            //the entire object (including subobjects) is already selected
        //            //Do nothing
        //        }
        //        else
        //        {
        //            rc = rc && RhinoDoc.ActiveDoc.Objects.Select(item, true, true);
        //        }


        //    }
        //    return rc;
        //}

        //========================= Read/Write ===================================

        //public bool ShouldWrite => this.IsValid;


        public bool ReadArchive(BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out var major, out var minor);
            if (major == 1 && minor == 0)
            {
                //HBObject, orphaned objects 
                var dic = archive.ReadDictionary();
                Deserialize(dic);

            }
            return !archive.ReadErrorOccured;
        }

        public bool WriteToArchive(BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);

            //HBObject, orphaned objects 
            var dic = Serialize();
            archive.WriteDictionary(dic);

            return !archive.WriteErrorOccured;
        }

        private ArchivableDictionary Serialize()
        {
            var dic = new ArchivableDictionary();
            dic.Set(nameof(HBObject), HBObject.ToJson());
            dic.Set(nameof(OrphanedApertures), OrphanedApertures);
            dic.Set(nameof(OrphanedDoors), OrphanedDoors);
            dic.Set(nameof(OrphanedFaces), OrphanedFaces);
            dic.Set(nameof(OrphanedShades), OrphanedShades);
            return dic;
        }

        private void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            this.HBObject = HB.Model.FromJson(dic[nameof(HBObject)].ToString());
            this.OrphanedApertures = (dic[nameof(OrphanedApertures)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedDoors = (dic[nameof(OrphanedDoors)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedFaces = (dic[nameof(OrphanedFaces)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedShades = (dic[nameof(OrphanedShades)] as IEnumerable<ObjRef>).ToList();
            
        }

        //========================= Helpers ===================================

       
    }
}
