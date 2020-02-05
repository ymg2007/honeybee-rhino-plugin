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
using HB = HoneybeeDotNet;

namespace HoneybeeRhino.Entities
{
    [Guid("D0F6A6F9-0CE0-41B7-8029-AB67F6B922AD")]
    public class RoomEntity : HBObjEntity
    {
        public HB.Room HBObject { get; private set; }

        public RoomEntity()
        {
        }

        public RoomEntity(HB.Room room, Guid hostID)
        {
            this.HBObject = room;
            this.HostGeoID = hostID;
            this.GroupEntityID = hostID;

            var ent = new Entities.GroupEntity(hostID);
        }
        /// <summary>
        /// Use this for objects were duplicated alone with RhinoObject, but Ids were still referencing old Rhino object ID.
        /// </summary>
        /// <param name="roomObj"></param>
        public void UpdateHostID(RhinoObject newRoomObj)
        {
            var hostID = newRoomObj.Id;
            this.HostGeoID = hostID;
            this.GroupEntityID = hostID;

            var ent = new Entities.GroupEntity(hostID);
        }

        protected override void OnDuplicate(UserData source)
        {
            if (source is RoomEntity src)
            {
                base.OnDuplicate(source);
                var json = src.HBObject.ToJson();
                this.HBObject = HB.Room.FromJson(json);
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
    }
}
