using Rhino.DocObjects;
using Rhino.UI;
using HoneybeeRhino.Entities;
using System.Linq;

namespace HoneybeeRhino.UI
{
    public class PropertyPage_Room : ObjectPropertiesPage
    {
        private PropertyPanel panelUI;
        public override object PageControl => panelUI ?? (panelUI = new PropertyPanel());

        public override string EnglishPageTitle => "HBRoom";
       
        public override ObjectType SupportedTypes => ObjectType.Brep;

        public override bool SupportsSubObjects => false;

        private HBObjEntity _HBObjEntity;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            //reset
            _HBObjEntity = new RoomEntity();
            if (!e.Objects.Any()) return false;

            var roomEnts = e.Objects.Where(_ => _.Geometry.TryGetRoomEntity().IsValid);
            if (!roomEnts.Any()) return false; 

            //var isApertureOnly = e.Objects.Count() == 1 && e.Objects[0].Geometry.TryGetApertureEntity().IsValid;
            //if (isApertureOnly)
            //    return false;

            this._HBObjEntity = roomEnts.First().Geometry.TryGetRoomEntity();
            return this._HBObjEntity.IsValid;

        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {

            if (!this._HBObjEntity.IsValid) return;

            if (this._HBObjEntity is RoomEntity rm)
            {
                this.panelUI.updateRoomPanel(rm);
            }
        }
    }

}