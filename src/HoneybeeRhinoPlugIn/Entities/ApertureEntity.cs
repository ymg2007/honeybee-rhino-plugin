﻿using Rhino.Collections;
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
    [Guid("FC5517EF-9AFF-4D02-A26D-80E1FEC4B6F1")]
    public class ApertureEntity : HBObjEntity
    {
        /// <summary>
        /// This object doesn't always has the most updated geometry data, this is mainly used for keeping honeybee data. 
        /// Use GetHBAperture to get real recalculated HBModel with all geometry data.
        /// </summary>
        public HB.Aperture HBObject { get; private set; } 
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
            var id = newApertureObj.ObjectId;
            this.HBObject.Name = $"Aperture_{id}";
            this.HBObject.DisplayName = $"My Aperture {id.ToString().Substring(0, 5)}";
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

        public new static ApertureEntity TryGetFrom(Rhino.Geometry.GeometryBase rhinoGeo)
        {
            var rc = new ApertureEntity();
            if (rhinoGeo == null)
                return rc;
            if (!rhinoGeo.IsValid)
                return rc;

            var ent = rhinoGeo.UserData.Find(typeof(ApertureEntity)) as ApertureEntity;

            return ent == null ? rc : ent;
        }



        #region Helper
        //========================= Helpers ===================================
        /// <summary>
        /// Get real recalculated HBModel with all geometry data.
        /// </summary>
        public HB.Aperture GetHBAperture()
        {
            //check Resource object
            CheckResourceForWindowConstruction(this.HBObject.Properties.Energy?.Construction);

            var aptFace3D = this.HostObjRef.Brep().ToHBFace3Ds().First();
            var obj = this.HBObject;
            obj.Geometry = aptFace3D;

            //TODO: check shades
            return obj;
        }

        #endregion
    }
}
