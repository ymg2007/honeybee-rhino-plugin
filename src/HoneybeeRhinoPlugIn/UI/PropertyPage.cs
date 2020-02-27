using Rhino.DocObjects;
using Rhino.UI;
using HoneybeeRhino.Entities;
using System.Linq;

namespace HoneybeeRhino.UI
{
    public class PropertyPage : ObjectPropertiesPage
    {
        private PropertyPanel panelUI;
        public override object PageControl => panelUI ?? (panelUI = new PropertyPanel());

        public override string EnglishPageTitle => "Honeybee";
       
        public override ObjectType SupportedTypes => ObjectType.Brep;

        //TODO: will add support to subobj later.
        public override bool SupportsSubObjects => false;

        private HBObjEntity _HBObjEntity;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            var groupTable = HoneybeeRhinoPlugIn.Instance.GroupEntityTable;

            //Check groupEntity first.
            //TODO: orphaned object doesn't have groupEntity, deal with this later
            var gpEnts = e.Objects.Select(_ => _.Geometry.TryGetGroupEntity(groupTable)).Where(_ => _.IsValid).Distinct();
            //Do not show if there are two or more groups are selected.
            if (!gpEnts.Any()) return false;
            if (gpEnts.Count() > 1) return false;
            if (!gpEnts.First().IsValid) return false;
          
            this._HBObjEntity = gpEnts.First().Room.TryGetRoomEntity();
            return this._HBObjEntity.IsValid;
      
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {
            if (this._HBObjEntity.IsValid)
            {
                this.panelUI.updateRoomPanel(this._HBObjEntity);
            }
        }
    }

}