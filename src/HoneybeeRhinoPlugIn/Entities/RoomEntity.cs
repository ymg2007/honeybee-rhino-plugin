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
        /// <summary>
        /// This object doesn't always has the most updated geometry data, this is mainly used for keeping honeybee data. 
        /// Use GetHBRoom to get real recalculated HBModel with all geometry data.
        /// </summary>
        public HB.Room HBObject { get; private set; }

        public Brep BrepGeomerty => this.HostObjRef.Brep();
        public string Name => this.HBObject.Name;
        public List<HB.Face> HBFaces => this.BrepGeomerty.Faces.Select(_ => _.TryGetFaceEntity().HBObject).ToList();

        public int ApertureCount
        {
            get
            {
                if (!this.IsValid)
                    return 0;
                var apts = this.HostObjRef.Brep().Surfaces.SelectMany(_ => _.TryGetFaceEntity().ApertureObjRefs);
                var valid = apts.Where(_ => _.TryGetApertureEntity().IsValid);
                return valid.Count();
            }
        } 

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
        public RoomEntity(HB.Room room, ObjRef hostObjRef)
        {
            this.HBObject = room;
            this.HostObjRef = hostObjRef;
            this.HostRoomObjRef = hostObjRef;
        }

    
        public bool SelectAndHighlight()
        {
            var ObjRefs = new List<ObjRef>();

            //select and highlight room
            ObjRefs.Add(this.HostObjRef);

            //select apertures, doors
            var apts = this.HostObjRef.Brep().Faces
                .Select(_ => _.TryGetFaceEntity())
                .Where(_ => _.IsValid)
                .SelectMany(_ => { 
                    var objRefs = _.ApertureObjRefs; 
                    objRefs.AddRange(_.DoorObjRefs); 
                    return objRefs; });

            ObjRefs.AddRange(apts);

            //TODO: select shades

            return ObjRefs.SelectHighlight();

        }

        public HB.Room GetHBRoom()
        {

            //recompute all rhino geometries to hbFace3D
            var roomBrep = this.BrepGeomerty;
            var room = this.HBObject;

            //check all subfaces
            var hbfaces = roomBrep.Faces.Select(_=>_.UnderlyingSurface().TryGetFaceEntity().GetHBFace(_));
            room.Faces = hbfaces.ToList();

            //TODO: check shades.
            return room;
        }



        /// <summary>
        /// Use this for objects were duplicated with its children objects alone with RhinoObject
        /// </summary>
        /// <param name="newObj">new host ObjRef</param>
        public RoomEntity UpdateHost(ObjRef newObj)
        {
            //update hostRed
            this.HostObjRef = newObj;

            //update HBobject name (ID):
            var newID = newObj.ObjectId;
            this.HBObject.Name = $"Room_{newID}";
            this.HBObject.DisplayName = $"My Room {newID.ToString().Substring(0, 5)}";


            //update subsurfaces:
            var faces = this.HBObject.Faces;
            foreach (var face in faces)
            {
                var fId = Guid.NewGuid();
                face.Name = $"Face_{fId}";
                face.DisplayName = $"Face {fId.ToString().Substring(0, 5)}";


            }

            //TODO: update windows? not sure if it is needed to be done at here, since it requires a host ObjRef.
            //TODO: update shades? same reason.


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

        public new static RoomEntity TryGetFrom(Rhino.Geometry.GeometryBase roomGeo)
        {
            var rc = new RoomEntity();
            if (roomGeo == null)
                return rc;

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

       
     
        #region Helper

       
        #endregion

    }
}
