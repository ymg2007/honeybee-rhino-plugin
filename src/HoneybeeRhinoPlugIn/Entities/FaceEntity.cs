using Rhino.Collections;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.Entities
{
    [Guid("88EC0885-1ACF-4D40-B53E-11D4AE174987")]
    public class FaceEntity : HBObjEntity
    {
        public HB.Face HBObject { get; private set; }

        public override bool IsValid => HBObject != null;
        public override string Description => this.IsValid ? $"HBFaceEntity: {HBObject.Name}" : base.Description;
        public FaceEntity()
        {
        }

        public FaceEntity(HB.Face hbObj)
        {
            this.HBObject = hbObj;
        }

        public void Duplicate(FaceEntity otherFaceEntityToCopyFrom)
        {
            this.OnDuplicate(otherFaceEntityToCopyFrom);
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is FaceEntity src)
            {
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Face.FromJson(json);
            }
        }

        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            base.Deserialize(dic);
            var json = dic.GetString("HBData");
            this.HBObject = HB.Face.FromJson(json);
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            return dic;
        }

        public void UpdateID_CopyFrom(FaceEntity otherFaceEntity)
        {
            if (otherFaceEntity.IsValid)
            {
                this.Duplicate(otherFaceEntity);
                this.HBObject.Name = "Face_" + Guid.NewGuid().ToString();
            }
            else
            {
                //throw new ArgumentNullException("OtherFaceEntity is null");
            }
     
        }

        public static FaceEntity TryGetFrom(Rhino.Geometry.GeometryBase rhinoGeo)
        {
            var rc = new FaceEntity();
            if (!rhinoGeo.IsValid)
                return rc;

            if (rhinoGeo is BrepFace brepFace)
            {
                //var ent = brepFace.Brep.Surfaces[brepFace.SurfaceIndex].UserData.Find(typeof(FaceEntity)) as FaceEntity;
                var ent = brepFace.UnderlyingSurface().TryGetFaceEntity();
                return ent == null ? rc : ent;
            }
            else if (rhinoGeo is Surface surface)
            {
                var ent = surface.UserData.Find(typeof(FaceEntity)) as FaceEntity;
                return ent == null ? rc : ent;
            }
            else
            {
                var ent = (rhinoGeo as Rhino.Geometry.Brep).Surfaces[0].UserData.Find(typeof(FaceEntity)) as FaceEntity;
                return ent == null ? rc : ent;
            }
          
        }
    }
}
