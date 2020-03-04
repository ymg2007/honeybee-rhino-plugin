using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    [Guid("FC5517EF-9AFF-4D02-A26D-80E1FEC4B6F1")]
    public class ApertureEntity : HBObjEntity
    {
        public HB.Aperture HBObject { get; private set; }

        //TODO: override isValid to check if hostID exists
        public override string Description => this.IsValid ? $"HBApertureEntity: {HBObject.Name}" : base.Description;

        public override bool IsValid
        {
            get
            {
                if (this.HostObjRef == null)
                    return false;
                if (this.HostObjRef.Brep() == null)
                    return false;

                return this.HostObjRef.Brep().IsValid
                    && this.HBObject != null;
            }
        }
        public ApertureEntity()
        {
        }

        public ApertureEntity(HB.Aperture hbObj)
        {
            this.HBObject = hbObj;
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is ApertureEntity src)
            {
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Aperture.FromJson(json);
            }
        }

        public ApertureEntity UpdateHostFrom(ObjRef newApertureObj)
        {
            //update HBObject name (ID)
            this.HBObject.Name = $"Aperture_{newApertureObj.ObjectId}";
            this.HBObject.DisplayName = null;
            //update hostRef
            this.HostObjRef = newApertureObj;
            return this;
        }


        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            base.Deserialize(dic);
            var json = dic.GetString("HBData");
            this.HBObject = HB.Aperture.FromJson(json);
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            return dic;
        }

        //public static ApertureEntity TryGetFrom(RhinoObject obj)
        //{
        //    return TryGetFrom(obj.Geometry);
        //}

        public static ApertureEntity TryGetFrom(Rhino.Geometry.GeometryBase rhinoGeo)
        {
            var rc = new ApertureEntity();
            if (rhinoGeo == null)
                return rc;
            if (!rhinoGeo.IsValid)
                return rc;

            var ent = rhinoGeo.UserData.Find(typeof(ApertureEntity)) as ApertureEntity;

            return ent == null ? rc : ent;
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
    }
}
