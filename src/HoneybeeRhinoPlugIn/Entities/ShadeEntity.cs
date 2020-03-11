using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using System;
using System.Runtime.InteropServices;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    [Guid("E312BB8B-4824-496D-B0E8-13E8D1B7304D")]
    public class ShadeEntity : HBObjEntity
    {
        public HB.Shade HBObject { get; private set; }

        public override string Description => this.IsValid ? $"ShadeEntity: {HBObject.Name}" : base.Description;

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
        public ShadeEntity()
        {
        }

        public ShadeEntity(HB.Shade hbObj)
        {
            this.HBObject = hbObj;
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is ShadeEntity src)
            {
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Shade.FromJson(json);
            }
        }

        public ShadeEntity UpdateHostFrom(ObjRef newShadeObj)
        {
            //update HBObject name (ID)
            var id = newShadeObj.ObjectId;
            this.HBObject.Name = $"Shade_{id}";
            this.HBObject.DisplayName = $"My Shade {id.ToString().Substring(0, 5)}";
            //update hostRef
            this.HostObjRef = newShadeObj;
            return this;
        }


        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            base.Deserialize(dic);
            var json = dic.GetString("HBData");
            this.HBObject = HB.Shade.FromJson(json);
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            return dic;
        }


        public new static ShadeEntity TryGetFrom(Rhino.Geometry.GeometryBase rhinoGeo)
        {
            var rc = new ShadeEntity();
            if (rhinoGeo == null)
                return rc;
            if (!rhinoGeo.IsValid)
                return rc;

            var ent = rhinoGeo.UserData.Find(typeof(ShadeEntity)) as ShadeEntity;

            return ent == null ? rc : ent;
        }


        #region Helper

      
        #endregion
    }
}
