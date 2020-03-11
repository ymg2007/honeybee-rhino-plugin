﻿using Rhino.DocObjects;
using Rhino.UI;
using HoneybeeRhino.Entities;
using System.Linq;

namespace HoneybeeRhino.UI
{
    public class PropertyPage_Door : ObjectPropertiesPage
    {
        private PropertyPanel panelUI;
        public override object PageControl => panelUI ?? (panelUI = new PropertyPanel());

        public override string EnglishPageTitle => "HBDoor";
        
        public override ObjectType SupportedTypes => ObjectType.Brep;

        public override bool SupportsSubObjects => false;

        private HBObjEntity _HBObjEntity;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            if (!e.Objects.Any()) return false;
            //if (e.ObjectCount !=1) return false;
            if (e.Objects.Count() != 1) return false;

            var obj = e.Objects.Last();
            var ent = obj.Geometry.TryGetDoorEntity();
            this._HBObjEntity = ent;
            return this._HBObjEntity.IsValid;
      
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {

            if (!this._HBObjEntity.IsValid) return;

            if (this._HBObjEntity is DoorEntity apt)
            {
                this.panelUI.updateDoorPanel(apt);
            }
        }
    }

}