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
    [Guid("D0F6A6F9-0CE0-41B7-8029-AB67F6B922AD")]
    public class RoomEntity : HBObjEntity
    {
        private HB.Room HBObject { get; set; }

        public Brep BrepGeomerty => this.HostObjRef.Brep();
        public string Name => this.HBObject.Name;
        public List<HB.Face> HBFaces => this.HBObject.Faces;

        public int ApertureCount => this.HostObjRef.Brep().Surfaces.Sum(_ => _.TryGetFaceEntity().ApertureObjRefs.Count);

        //TODO: override isValid to check if hostID exists
        public override bool IsValid
        {
            get
            {
                if (this.HostObjRef == null)
                    return false;
                if (this.BrepGeomerty == null)
                    return false;

                return this.BrepGeomerty.IsValid
                    && this.HBObject != null;
            }
        }

        public override string Description => this.IsValid ? $"HBRoomEntity: {HBObject.Name}" : base.Description;
        public RoomEntity()
        {
        }
        public RoomEntity(ObjRef brepObject, Func<Brep, bool> objectReplaceFunc,  double maxRoofFloorAngle = 30, double tolerance = 0.0001)
        {
            //check if Null, valid, solid
            if (!CheckIfBrepObjectValid(brepObject))
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");

            //Create honeybee room object here.
            var closedBrep = brepObject.Brep().DuplicateBrep();
            var dupBrep = closedBrep.ToAllPlaneBrep(tolerance);
            var subFaces = dupBrep.Faces;

            var hbFaces = subFaces.Select(_ => _.ToHBFace(maxRoofFloorAngle)).ToList();

            for (int i = 0; i < hbFaces.Count; i++)
            {
                var faceEnt = new FaceEntity(hbFaces[i]);
                var bFace = dupBrep.Surfaces[i];
                bFace.UserData.Add(faceEnt);
            }


            this.HBObject = new HB.Room($"Room_{brepObject.ObjectId}", hbFaces, new HB.RoomPropertiesAbridged());
            this.HostObjRef = brepObject;
            this.GroupEntityID = brepObject.ObjectId;

            //Add this RoomEntity to brep's userdata at the end.
            dupBrep.UserData.Add(this);
            //Make sure the underneath brep geometry is replaced.
            var success = objectReplaceFunc(dupBrep);
            if (!success)
                throw new ArgumentException("Failed to convert to honeybee room!");
#if DEBUG

            if (!dupBrep.TryGetRoomEntity().IsValid)
                throw new ArgumentException("Failed to convert to honeybee room!");

            var refreshedObj = new ObjRef(brepObject.ObjectId).Object();
            if (!refreshedObj.Geometry.TryGetRoomEntity().IsValid)
                throw new ArgumentException("Failed to convert to honeybee room!");
            if (refreshedObj.Geometry.TryGetRoomEntity().GroupEntityID == Guid.Empty)
                throw new ArgumentException("Failed to convert to honeybee room!");

#endif
        }

    
        public bool SelectAndHighlight()
        {
            var ObjRefs = new List<ObjRef>();

            //select and highlight room
            ObjRefs.Add(this.HostObjRef);

            //select apertures
            var apts = this.HostObjRef.Brep().Faces
                .Select(_ => _.TryGetFaceEntity())
                .Where(_ => _.IsValid)
                .SelectMany(_ => _.ApertureObjRefs);
            ObjRefs.AddRange(apts);

            //TODO: select shades

            return ObjRefs.SelectHighlight();

        }

        public HB.Room GetHBRoom(bool recomputeGeometry = false)
        {
            if (!recomputeGeometry)
                return this.HBObject;

            //recompute all rhino geometries to hbFace3D
            var roomBrep = Brep.TryConvertBrep(this.BrepGeomerty);
            var room = this.HBObject;

            //check all subfaces
            var brepFaces = roomBrep.Faces;
            var checkedHBFaces = new List<HB.Face>();
            foreach (var bFace in brepFaces)
            {
                var faceEnt = bFace.UnderlyingSurface().TryGetFaceEntity();
                var HBFace = faceEnt.HBObject;
                var face3d = bFace.ToHBFace3D();
                HBFace.Geometry = face3d;

                //check apertures
                var apertureBreps = faceEnt.ApertureObjRefs;
                var checkedHBApertures = new List<HB.Aperture>();
                foreach (var apertureBrep in apertureBreps)
                {
                    //update aperture geometry
                    var aptFace3D = apertureBrep.Brep().ToHBFace3Ds().First();
                    var HBAperture = apertureBrep.TryGetApertureEntity().HBObject;
                    HBAperture.Geometry = aptFace3D;
                    checkedHBApertures.Add(HBAperture);
                }
                HBFace.Apertures = checkedHBApertures;

                //TODO: check shades

                //TODO: make sure all other meta data still exists in face.
                checkedHBFaces.Add(HBFace);
            }

            room.Faces = checkedHBFaces;
            return room;
        }


   
        /// <summary>
        /// Use this for objects were duplicated alone with RhinoObject, but Ids were still referencing old Rhino object ID.
        /// </summary>
        /// <param name="roomObj"></param>
        public RoomEntity UpdateHost(ObjRef newObj)
        {
            //update hostRed
            this.HostObjRef = newObj;

            //update HBobject name (ID):
            var newID = newObj.ObjectId;
            this.HBObject.Name = $"Room_{newID}";
            this.HBObject.DisplayName = null;


            //update subsurfaces:
            var faces = this.HBObject.Faces;
            foreach (var face in faces)
            {
                face.Name = $"Face_{Guid.NewGuid()}";
                face.DisplayName = null;

            }

          
            return this;
        }
        public void Duplicate(RoomEntity otherRoomEntityToCopyFrom)
        {
            this.OnDuplicate(otherRoomEntityToCopyFrom);
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is RoomEntity src)
            {
                if (!src.IsValid)
                    return;
                
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Room.FromJson(json);
                this.HostObjRef = src.HostObjRef;
            }
            
        }

        private protected override void Deserialize(ArchivableDictionary dictionary)
        {
            base.Deserialize(dictionary);
            var dic = dictionary;
            var json = dic.GetString("HBData");
            this.HBObject = HB.Room.FromJson(json);
            
        }

        private protected override ArchivableDictionary Serialize()
        {
            var dic = base.Serialize();
            dic.Set("HBData", this.HBObject.ToJson());
            return dic;
        }

        //public static RoomEntity TryGetFrom(RhinoObject obj)
        //{
        //    return TryGetFrom(obj.Geometry);
        //}

        public static RoomEntity TryGetFrom(Rhino.Geometry.GeometryBase roomGeo)
        {
            var rc = new RoomEntity();
            if (!roomGeo.IsValid)
                return rc;

            var ent = roomGeo.UserData.Find(typeof(RoomEntity)) as RoomEntity;

            return ent == null ? rc : ent;
        }


        
        public void SetEnergyProp(HB.RoomEnergyPropertiesAbridged roomEnergyProp)
        {
            this.HBObject.Properties.Energy = roomEnergyProp;
        }
        public HB.RoomEnergyPropertiesAbridged GetEnergyProp()
        {
            return this.HBObject.Properties.Energy ?? new HB.RoomEnergyPropertiesAbridged();
        }

        private static bool CheckIfBrepObjectValid(ObjRef roomObj)
        {
            if (roomObj == null)
                throw new NullReferenceException();

            if (!roomObj.Brep().IsValid)
                throw new ArgumentException("Input geometry is not a valid object to convert to honeybee room!");

            if (!roomObj.Brep().IsSolid)
                throw new ArgumentException("This rhino object is not a water-tight solid!");

            return true;
        }
    }
}
