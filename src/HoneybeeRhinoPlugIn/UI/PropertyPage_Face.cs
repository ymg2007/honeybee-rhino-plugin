using Rhino.DocObjects;
using Rhino.UI;
using HoneybeeRhino.Entities;
using System.Linq;

namespace HoneybeeRhino.UI
{
    public class PropertyPage_Face : ObjectPropertiesPage
    {
        private PropertyPanel panelUI;
        public override object PageControl => panelUI ?? (panelUI = new PropertyPanel());

        public override string EnglishPageTitle => "HBFace";
       
        public override ObjectType SupportedTypes => ObjectType.Brep;

        public override bool SupportsSubObjects => true;

        private HBObjEntity _HBObjEntity;

        public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e)
        {
            if (!e.Objects.Any()) return false;

            //check if a subsurface is selected.
            var selectedObj = e.Objects.Last();
            var subObjes = selectedObj.GetSelectedSubObjects();
            var isSelectedBrepFace = null != subObjes;

            if (isSelectedBrepFace)
            {
                var comIndex = subObjes.Last();
                if (comIndex.ComponentIndexType != Rhino.Geometry.ComponentIndexType.BrepFace)
                    return false;

                var faceIndex = comIndex.Index;
                var hostRoomObjRef =  new ObjRef(selectedObj.Id);
                this._HBObjEntity = hostRoomObjRef.TryGetFaceEntity(comIndex);
                return this._HBObjEntity.IsValid;
            }
            return false;

      
        }

        public override void UpdatePage(ObjectPropertiesPageEventArgs e)
        {

            if (!this._HBObjEntity.IsValid) return;

            if (this._HBObjEntity is FaceEntity face)
            {
                this.panelUI.updateFacePanel(face);
            }
        }
    }

}