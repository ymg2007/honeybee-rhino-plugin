﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;

namespace HoneybeeRhino.Entities
{
    [Guid("D9C8832F-EE24-4834-A443-AC981B8D9921")]
    public abstract class HBObjEntity: UserData
    {
        public ObjRef HostObjRef { get; set; } //Object is orphaned must have HostObjRef
        public ObjRef HostRoomObjRef { get; set; } //Object is orphaned if HostRoomObjRef is null
        public virtual bool IsValid => this.HostObjRef != null;
        public override bool ShouldWrite => IsValid;
        public override string Description => this.IsValid ? "Honeybee Object Entity" : "An Invalid Honeybee Object Entity";
        public override string ToString() => this.Description;

        protected override void OnDuplicate(UserData source)
        {
            if (source is HBObjEntity src)
            {
                this.HostObjRef = src.HostObjRef;
                if (src.HostRoomObjRef != null)
                {
                    this.HostRoomObjRef = new ObjRef(src.HostRoomObjRef.ObjectId);
                }
            }

        }
        protected override bool Read(Rhino.FileIO.BinaryArchiveReader archive)
        {
            archive.Read3dmChunkVersion(out var major, out var minor);
            if (major == 1 && minor == 0)
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

        private protected virtual ArchivableDictionary Serialize() 
        {
            var dic = new ArchivableDictionary();
            if (HostObjRef != null)
            {
                //Subsurfaces have no host ObjRef object
                dic.Set(nameof(HostObjRef), HostObjRef);
            }

            if (HostRoomObjRef != null)
                dic.Set(nameof(HostRoomObjRef), HostRoomObjRef);


            return dic;
        }
        private protected virtual void Deserialize(ArchivableDictionary dictionary) 
        {
            var dic = dictionary;
            var hashost = dic.TryGetValue(nameof(HostObjRef), out object obj);
            if (hashost)
            {
                //Subsurfaces have no host ObjRef object
                this.HostObjRef = obj as ObjRef;
            }

            if (dic.TryGetValue(nameof(HostRoomObjRef), out object roomObj))
            {
                this.HostRoomObjRef = roomObj as ObjRef;
            }

        }
        public static HBObjEntity TryGetFrom(Rhino.Geometry.GeometryBase roomGeo)
        {
            var ee = roomGeo.UserData.FirstOrDefault(_ => _ is HBObjEntity) as HBObjEntity;
            var ent = roomGeo.UserData.Find(typeof(HBObjEntity)) as HBObjEntity;

            return ee;
        }

        public bool SelectAndHighlightRoom()
        {
            if (this.HostRoomObjRef == null)
                return false;

            if (this.HostRoomObjRef.Geometry() == null)
                return false;

            var roomEnt = this.HostRoomObjRef.Geometry().TryGetRoomEntity();
            if (!roomEnt.IsValid)
                return false;

            return roomEnt.SelectAndHighlight();
        }

        protected static void CheckResourceForOpaqueConstruction(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            var cLib = ModelEntity.OpaqueConstructions;
            if (cLib == null)
                return;

            //check Resource object
            var cName = name;
            if (cLib.Any(_ => _.Name == cName))
                return;

            var c = EnergyLibrary.GetOpaqueConstructionByName(cName);
            cLib.Add(c);

            var materials = EnergyLibrary.GetConstructionMaterials(c);
            var mLib = ModelEntity.EnergyMaterials;
            foreach (var m in materials)
            {
                if (mLib.Any(_ => _ == m))
                    continue;
                mLib.Add(m);
            }

        }
        protected static void CheckResourceForWindowConstruction(string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            var cLib = ModelEntity.WindowConstructions;
            if (cLib == null)
                return;

            //check Resource object
            var cName = name;
            if (cLib.Any(_ => _.Name == cName))
                return;

            var c = EnergyLibrary.GetWindowConstructionByName(cName);
            cLib.Add(c);

            var materials = EnergyLibrary.GetConstructionMaterials(c);
            var mLib = ModelEntity.EnergyWindowMaterials;
            foreach (var m in materials)
            {
                if (mLib.Any(_ => _ == m))
                    continue;
                mLib.Add(m);
            }

        }


    }

}