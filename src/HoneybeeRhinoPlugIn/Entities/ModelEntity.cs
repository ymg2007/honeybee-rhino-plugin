﻿using System;
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
    [Guid("52E4D8D9-C8F2-46F0-89A3-7441BC418020")]
    public class ModelEntity : UserData
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


        public List<Guid> RoomGroupEntityIDs { get; private set; } = new List<Guid>();
        public List<Guid> OrphanedFaceIDs { get; private set; } = new List<Guid>();
        public List<Guid> OrphanedShadeIDs { get; private set; } = new List<Guid>();
        public List<Guid> OrphanedApertureIDs { get; private set; } = new List<Guid>();
        public List<Guid> OrphanedDoorIDs { get; private set; } = new List<Guid>();

        public bool IsValid => true;


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
            dic.Set(nameof(RoomGroupEntityIDs), RoomGroupEntityIDs);
            dic.Set(nameof(OrphanedApertureIDs), OrphanedApertureIDs);
            dic.Set(nameof(OrphanedDoorIDs), OrphanedDoorIDs);
            dic.Set(nameof(OrphanedFaceIDs), OrphanedFaceIDs);
            dic.Set(nameof(OrphanedShadeIDs), OrphanedShadeIDs);
            return dic;
        }

        private void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            this.RoomGroupEntityIDs = (dic[nameof(RoomGroupEntityIDs)] as IEnumerable<Guid>).ToList();
            this.OrphanedApertureIDs = (dic[nameof(OrphanedApertureIDs)] as IEnumerable<Guid>).ToList();
            this.OrphanedDoorIDs = (dic[nameof(OrphanedDoorIDs)] as IEnumerable<Guid>).ToList();
            this.OrphanedFaceIDs = (dic[nameof(OrphanedFaceIDs)] as IEnumerable<Guid>).ToList();
            this.OrphanedShadeIDs = (dic[nameof(OrphanedShadeIDs)] as IEnumerable<Guid>).ToList();
        }

        //========================= Helpers ===================================

       
    }
}