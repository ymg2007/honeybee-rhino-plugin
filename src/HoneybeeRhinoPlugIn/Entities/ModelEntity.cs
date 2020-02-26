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
    public class ModelEntity
    {
        public string ModelNameID { get; private set; }
        public HB.Model HBObject { get; private set; }
        public ModelEntity() { }

        public ModelEntity(HB.Model hbObject) 
        {
            this.HBObject = hbObject;
            this.ModelNameID = hbObject.Name;
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


        public GroupEntityTable RoomGroupEntities { get; private set; } = new GroupEntityTable();
        public List<ObjRef> OrphanedFaces { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedShades { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedApertures { get; private set; } = new List<ObjRef>();
        public List<ObjRef> OrphanedDoors { get; private set; } = new List<ObjRef>();

        public bool IsValid => true;

        public ModelEntity Duplicate()
        {
            var json = this.HBObject.ToJson();
            var model = HB.Model.FromJson(json);
            var newEnt = new ModelEntity(model);
            newEnt.RoomGroupEntities = this.RoomGroupEntities.Duplicate();
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
                var dic = archive.ReadDictionary();
                Deserialize(dic);

                //Takes care of GroupEntityTable
                var t = new GroupEntityTable();
                t.ReadDocument(archive);
                this.RoomGroupEntities = t;
            }
            return !archive.ReadErrorOccured;
        }

        public bool WriteToArchive(BinaryArchiveWriter archive)
        {
            archive.Write3dmChunkVersion(1, 0);

            var dic = Serialize();
            archive.WriteDictionary(dic);

            //Takes care of GroupEntityTable
            RoomGroupEntities.WriteDocument(archive);
            return !archive.WriteErrorOccured;
        }

        private ArchivableDictionary Serialize()
        {
            var dic = new ArchivableDictionary();
            dic.Set(nameof(OrphanedApertures), OrphanedApertures);
            dic.Set(nameof(OrphanedDoors), OrphanedDoors);
            dic.Set(nameof(OrphanedFaces), OrphanedFaces);
            dic.Set(nameof(OrphanedShades), OrphanedShades);
            return dic;
        }

        private void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            this.OrphanedApertures = (dic[nameof(OrphanedApertures)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedDoors = (dic[nameof(OrphanedDoors)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedFaces = (dic[nameof(OrphanedFaces)] as IEnumerable<ObjRef>).ToList();
            this.OrphanedShades = (dic[nameof(OrphanedShades)] as IEnumerable<ObjRef>).ToList();
            
        }

        //========================= Helpers ===================================

       
    }
}
