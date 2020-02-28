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

        public override bool SupportsSubObjects => true;

        private HBObjEntity _HBObjEntity;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            if (!e.Objects.Any()) return false;

            //check if a subsurface is selected.
            var selectedObj = e.Objects[0];
            var subObjes = selectedObj.GetSelectedSubObjects();
            var isSelectedBrepFace = null != subObjes;

            if (isSelectedBrepFace)
            {
                var comIndex = subObjes[0];
                if (comIndex.ComponentIndexType != Rhino.Geometry.ComponentIndexType.BrepFace)
                    return false;

                var faceIndex = comIndex.Index;
                var hostRoomObjRef =  new ObjRef(selectedObj.Id);
                this._HBObjEntity = hostRoomObjRef.TryGetFaceEntity(comIndex);
                return this._HBObjEntity.IsValid;
            }

            //Now checking room group entity.
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

            if (!this._HBObjEntity.IsValid) return;

            if (this._HBObjEntity is RoomEntity rm)
            {
                this.panelUI.updateRoomPanel(rm);
            }
            else if (this._HBObjEntity is FaceEntity face)
            {
                this.panelUI.updateFacePanel(face);
            }
        }
    }

}