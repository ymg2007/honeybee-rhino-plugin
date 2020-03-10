using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.Geometry;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    [Guid("7E348AC8-F8CB-4F83-AFE3-D9C9DFAEC8CB")]
    public class DoorEntity : HBObjEntity
    {
        public HB.Door HBObject { get; private set; }

        public override string Description => this.IsValid ? $"DoorEntity: {HBObject.Name}" : base.Description;

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
        public DoorEntity()
        {
        }

        public DoorEntity(HB.Door hbObj)
        {
            this.HBObject = hbObj;
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is DoorEntity src)
            {
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Door.FromJson(json);
            }
        }

        public DoorEntity UpdateHostFrom(ObjRef newDoorObj)
        {
            //update HBObject name (ID)
            var id = newDoorObj.ObjectId;
            this.HBObject.Name = $"Door_{id}";
            this.HBObject.DisplayName = $"My Door {id.ToString().Substring(0, 5)}";
            //update hostRef
            this.HostObjRef = newDoorObj;
            return this;
        }


        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            base.Deserialize(dic);
            var json = dic.GetString("HBData");
            this.HBObject = HB.Door.FromJson(json);
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            return dic;
        }


        public new static DoorEntity TryGetFrom(Rhino.Geometry.GeometryBase rhinoGeo)
        {
            var rc = new DoorEntity();
            if (rhinoGeo == null)
                return rc;
            if (!rhinoGeo.IsValid)
                return rc;

            var ent = rhinoGeo.UserData.Find(typeof(DoorEntity)) as DoorEntity;

            return ent == null ? rc : ent;
        }


        #region Helper

      
        #endregion
    }
}
