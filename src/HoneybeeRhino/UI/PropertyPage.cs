﻿using System;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using HB = HoneybeeDotNet;
using System.Linq;
using System.Collections.Generic;

namespace HoneybeeRhino.UI
{
    public class PropertyPage : ObjectPropertiesPage
    {
        private PropertyPanel panelUI;
        public override object PageControl => panelUI ?? (panelUI = new PropertyPanel());

        public override string EnglishPageTitle => "Honeybee";

        public override ObjectType SupportedTypes => ObjectType.Brep | ObjectType.Extrusion;

        //TODO: will add support to subobj later.
        public override bool SupportsSubObjects => false;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            if (e.ObjectCount != 1) return false;
            if (e.Objects.Length != 1) return false; //there is a bug in Rhino, which ObjectCount ==1, but Object is empty.

            var rObj = e.Objects[0];
      
            return rObj.Geometry.HasHBJson();
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            var selectedObj = e.Objects[0].Geometry;
            if (selectedObj.HasHBJson())
            {
                var room = HoneybeeDotNet.Room.FromJson(selectedObj.GetHBJson());
                this.panelUI.updateRoomPanel(room);
            }
        }
    }

}