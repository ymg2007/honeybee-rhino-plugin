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

        public List<ObjRef> RoomEntities { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedFaces { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedShades { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedApertures { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedDoors { get; private set; } = new List<ObjRef>();

        public List<ObjRef> RoomEntitiesWithoutHistory
        {
            get 
            {
                return this.RoomEntities.Where(_ => _.TryGetRoomEntity().IsValid).ToList();
            }
        }
        public List<ObjRef> OrphanedFacesWithoutHistory
        {
            get 
            { 
                return this.OrphanedFaces.Where(_ => _.TryGetOrphanedFaceEntity().IsValid).ToList();
            }
        }
        public List<ObjRef> OrphanedShadesWithoutHistory
        {
            //TODO: add validation later
            get { return this.OrphanedShades; }
        }
        public List<ObjRef> OrphanedAperturesWithoutHistory
        {
            get 
            {
                return this.OrphanedApertures.Where(_ => _.TryGetApertureEntity().IsValid).ToList();
            }
        }
        public List<ObjRef> OrphanedDoorsWithoutHistory
        {
            //TODO: add validation later
            get { return this.OrphanedDoors; }
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
            newEnt.RoomEntities = new List<ObjRef>(this.RoomEntitiesWithoutHistory);
            newEnt.OrphanedFaces = new List<ObjRef>(this.OrphanedFacesWithoutHistory);
            newEnt.OrphanedShades = new List<ObjRef>(this.OrphanedShadesWithoutHistory);
            newEnt.OrphanedApertures = new List<ObjRef>(this.OrphanedAperturesWithoutHistory);
            newEnt.OrphanedDoors = new List<ObjRef>(this.OrphanedDoorsWithoutHistory);

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
            dic.Set(nameof(OrphanedApertures), OrphanedAperturesWithoutHistory);
            dic.Set(nameof(OrphanedDoors), OrphanedDoorsWithoutHistory);
            dic.Set(nameof(OrphanedFaces), OrphanedFacesWithoutHistory);
            dic.Set(nameof(OrphanedShades), OrphanedShadesWithoutHistory);
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
