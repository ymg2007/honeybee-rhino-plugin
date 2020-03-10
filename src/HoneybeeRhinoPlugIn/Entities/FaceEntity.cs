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

        public List<ObjRef> ApertureObjRefs { get; private set; } = new List<ObjRef>();
        public List<ObjRef> DoorObjRefs { get; private set; } = new List<ObjRef>();
        public List<ObjRef> ApertureObjRefsWithoutHistory => this.ApertureObjRefs.Where(_ => _.TryGetApertureEntity().IsValid).ToList();
        public List<ObjRef> DoorObjRefsWithoutHistory => this.DoorObjRefs.Where(_ => _.TryGetDoorEntity().IsValid).ToList();

        public override bool IsValid => HBObject != null;
        public override string Description => this.IsValid ? $"HBFaceEntity: {HBObject.Name}" : base.Description;

        //below properties doesn't have be serialized, only for runtime.
        public ObjRef RoomHostObjRef { get; private set; }
        public ComponentIndex ComponentIndex { get; private set; }


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
                this.ApertureObjRefs = src.ApertureObjRefsWithoutHistory;
                this.DoorObjRefs = src.DoorObjRefsWithoutHistory;
            }
        }

        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            var dic = dictionary;
            base.Deserialize(dic);
            var json = dic.GetString("HBData");
            this.HBObject = HB.Face.FromJson(json);
            this.ApertureObjRefs = (dic[nameof(ApertureObjRefs)] as IEnumerable<ObjRef>).ToList();
            this.DoorObjRefs = (dic[nameof(DoorObjRefs)] as IEnumerable<ObjRef>).ToList();
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            dic.Set(nameof(ApertureObjRefs), this.ApertureObjRefsWithoutHistory);
            dic.Set(nameof(DoorObjRefs), this.DoorObjRefsWithoutHistory);
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

        //This is used in subselection, in which need to replace entire geometry after property changes
        public static FaceEntity TryGetFrom(ObjRef roomHostObjRef, ComponentIndex componentIndex)
        {
            var roomHostObject = roomHostObjRef.Brep();
            var rc = new FaceEntity();
            if (roomHostObject == null)
                return rc;
            if (componentIndex.ComponentIndexType != ComponentIndexType.BrepFace)
                return rc;

            var face = roomHostObject.Faces[componentIndex.Index];
            var ent = face.TryGetFaceEntity();
            if (!ent.IsValid)
                return rc;

            //updates hostObjRef to its room's host
            ent.RoomHostObjRef = roomHostObjRef;
            ent.ComponentIndex = componentIndex;
            return ent;
        }

        public static FaceEntity TryGetFrom(GeometryBase rhinoGeo)
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

        public void AddAperture(ObjRef apertureObjRef, Brep aperture)
        {
            var apertureEntity = aperture.TryGetApertureEntity();
            if (!apertureEntity.IsValid)
                throw new ArgumentException("Aperture brep is not a valid Honeybee aperture object!");

            this.ApertureObjRefs.Add(apertureObjRef);

            var HBApertures = this.HBObject.Apertures?? new List<HB.Aperture>();
            HBApertures.Add(apertureEntity.HBObject);
            this.HBObject.Apertures = HBApertures;
        }

        public void AddDoor(ObjRef doorObjRef, Brep door)
        {
            var ent = door.TryGetDoorEntity();
            if (!ent.IsValid)
                throw new ArgumentException("Door brep is not a valid Honeybee aperture object!");

            this.DoorObjRefs.Add(doorObjRef);

            var HBDoors = this.HBObject.Doors ?? new List<HB.Door>();
            HBDoors.Add(ent.HBObject);
            this.HBObject.Doors = HBDoors;
        }

        public List<ObjRef> GetApertures()
        {
            return this.ApertureObjRefs;
        }

        public void UpdateApertures(List<(ObjRef newApt, Guid oldID)> apertureMatches)
        {
            if (!apertureMatches.Any())
                return;

            var newApts = this.ApertureObjRefs.Select(_ => apertureMatches.Where(apt => apt.oldID == _.ObjectId).First().newApt);
            this.ApertureObjRefs = newApts.ToList();

        }
      
    }
}
