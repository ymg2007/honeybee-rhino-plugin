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
        /// <summary>
        /// This object doesn't always has the most updated geometry data, this is mainly used for keeping honeybee data. 
        /// Use GetHBModel to get real recalculated HBModel with all geometry data.
        /// </summary>
        public HB.Model HBObject { get; private set; }

        public List<ObjRef> Rooms { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedFaces { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedShades { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedApertures { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedDoors { get; private set; } = new List<ObjRef>();

        public List<ObjRef> RoomEntitiesWithoutHistory
        {
            get 
            {
                return this.Rooms.Where(_ => _.TryGetRoomEntity().IsValid).ToList();
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



        public ModelEntity() 
        {
        }

        public ModelEntity(HB.Model hbModel = default, bool newID = false) 
        {
            var id = System.Guid.NewGuid();
            var modelName = $"Model_{id}";
            var hbObj = hbModel ?? new HoneybeeSchema.Model(modelName, new HoneybeeSchema.ModelProperties(energy: HoneybeeSchema.ModelEnergyProperties.Default));
            if (newID)
            {
                hbObj.Name = modelName;
                hbObj.DisplayName = $"My Honeybee Model {id.ToString().Substring(0, 5)}";
            }

            this.HBObject = hbObj;

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
            newEnt.Rooms = new List<ObjRef>(this.RoomEntitiesWithoutHistory);
            newEnt.OrphanedFaces = new List<ObjRef>(this.OrphanedFacesWithoutHistory);
            newEnt.OrphanedShades = new List<ObjRef>(this.OrphanedShadesWithoutHistory);
            newEnt.OrphanedApertures = new List<ObjRef>(this.OrphanedAperturesWithoutHistory);
            newEnt.OrphanedDoors = new List<ObjRef>(this.OrphanedDoorsWithoutHistory);

            return newEnt;
        }
    


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
            dic.Set(nameof(Rooms), RoomEntitiesWithoutHistory);
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
            this.Rooms = (dic[nameof(Rooms)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedApertures = (dic[nameof(OrphanedApertures)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedDoors = (dic[nameof(OrphanedDoors)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedFaces = (dic[nameof(OrphanedFaces)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedShades = (dic[nameof(OrphanedShades)] as IEnumerable<ObjRef>).ToList();
            
        }

        //========================= Helpers ===================================
        /// <summary>
        /// Get real recalculated HBModel with all geometry data.
        /// </summary>
        public HB.Model GetHBModel()
        {
            var model = this.HBObject; 
            model.Properties = model.Properties?? new HB.ModelProperties(energy: HB.ModelEnergyProperties.Default);
            model.Rooms = this.RoomEntitiesWithoutHistory.Select(_ => _.TryGetRoomEntity().GetHBRoom()).ToList();
            model.OrphanedShades = this.OrphanedShadesWithoutHistory.SelectMany(_ => _.TryGetShadeEntity().GetHBShades()).ToList();
            //DODO: need to double check
            model.OrphanedApertures = this.OrphanedAperturesWithoutHistory.Select(_ => _.TryGetApertureEntity().HBObject).ToList();
            model.OrphanedDoors = this.OrphanedDoorsWithoutHistory.Select(_ => _.TryGetDoorEntity().HBObject).ToList();
            model.OrphanedFaces = this.OrphanedFacesWithoutHistory.Select(_ => _.TryGetOrphanedFaceEntity().HBObject).ToList();
            return model;
        }

       
    }
}
