using System;
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
        public Guid HostGeoID { get; set; } = Guid.Empty;
        public Guid GroupEntityID { get; set; } = Guid.Empty;
        public virtual bool IsValid => this.HostGeoID != Guid.Empty;
        public override bool ShouldWrite => IsValid;

        protected override void OnDuplicate(UserData source)
        {
            if (source is HBObjEntity src)
            {
                this.HostGeoID = src.HostGeoID;
                this.GroupEntityID = src.GroupEntityID;
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
            dic.Set(nameof(HostGeoID), HostGeoID);
            dic.Set(nameof(GroupEntityID), GroupEntityID);
            return dic;
        }
        private protected virtual void Deserialize(ArchivableDictionary dictionary) 
        {
            var dic = dictionary;
            this.HostGeoID = dic.GetGuid(nameof(HostGeoID));
            this.GroupEntityID = dic.GetGuid(nameof(GroupEntityID));

        }
        public static HBObjEntity TryGetFrom(Rhino.Geometry.GeometryBase roomGeo)
        {
            var ee = roomGeo.UserData.FirstOrDefault(_ => _ is HBObjEntity) as HBObjEntity;
            var ent = roomGeo.UserData.Find(typeof(HBObjEntity)) as HBObjEntity;

            return ee;
        }

    }

}