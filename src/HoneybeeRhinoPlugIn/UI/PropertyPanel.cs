using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using Rhino.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoneybeeSchema;
using System.Collections;

namespace HoneybeeRhino.UI
{
    internal class PropertyPanel: Eto.Forms.Panel
    {
        public PropertyPanel()
        {
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
        }

        public void updateRoomPanel(Entities.HBObjEntity hbObjEntity)
        {

            if (!(hbObjEntity is Entities.RoomEntity rm))
                return;

            
            var room = rm.GetHBRoom();
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);


            layout.AddSeparateRow(new Label { Text = $"ID: {room.Name}" });
            
            layout.AddSeparateRow(new Label { Text = "Name:" });
            var modelNameTextBox = new TextBox() { };
            modelNameTextBox.TextBinding.Bind(room, m => m.DisplayName?? $"My Room {room.Name.Substring(5,5)}");
            layout.AddSeparateRow(modelNameTextBox);


            layout.AddSeparateRow(new Label { Text = "Properties:" });
            var rmPropBtn = new Button { Text = "Room Energy Properties" };
            rmPropBtn.Click += (s, e) => RmPropBtn_Click(room.Properties.Energy, (v)=> room.Properties.Energy = v);
            layout.AddSeparateRow(rmPropBtn);

            layout.AddSeparateRow(new Label { Text = $"Faces: (total: {room.Faces.Count})" });
            var facesListBox = new ListBox();
            facesListBox.Height = 120;
            var faces = room.Faces;
            if (faces != null)
            {
                var faceItems = faces.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                facesListBox.Items.AddRange(faceItems);
            }
            layout.AddSeparateRow(facesListBox);


            layout.AddSeparateRow(new Label { Text = "IndoorShades:" });
            var inShadesListBox = new ListBox();
            inShadesListBox.Height = 50;
            var inShds = room.IndoorShades;
            if (inShds != null)
            {
                var idShds = inShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                inShadesListBox.Items.AddRange(idShds);
            }
            layout.AddSeparateRow(inShadesListBox);

            layout.AddSeparateRow(new Label { Text = "OutdoorShades:" });
            var outShadesListBox = new ListBox();
            outShadesListBox.Height = 50;
            var outShds = room.OutdoorShades;
            if (outShds != null)
            {
                var outShdItems = outShds.Select(_ => new ListItem() { Text = _.DisplayName ?? _.Name, Tag = _ });
                outShadesListBox.Items.AddRange(outShdItems);
            }
            layout.AddSeparateRow(outShadesListBox);


            layout.AddSeparateRow(new Label { Text = "Multiplier:" });
            var multiplierNum = new NumericMaskedTextBox<int>() { };
            multiplierNum.TextBinding.Bind(Binding.Delegate(() => room.Multiplier.ToString(), v => room.Multiplier = CheckIfNum(v)));
            int CheckIfNum(string numString)
            {
                var isNum = int.TryParse(numString, out int numValue);
                return isNum ? numValue : 0;
            }
            layout.AddSeparateRow(multiplierNum);



            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", room.ToJson(), true, out string outJson);
            layout.AddSeparateRow(data_button, null);


            this.Content = layout;
            //layout.up

        }

        private void RmPropBtn_Click(RoomEnergyPropertiesAbridged roomEnergyProperties, Action<RoomEnergyPropertiesAbridged> setAction)
        {
            var dialog = new UI.EnergyPropertyDialog(roomEnergyProperties);
            dialog.RestorePosition();
            var dialog_rc = dialog.ShowModal(RhinoEtoApp.MainWindow);
            dialog.SavePosition();
            if (dialog_rc != null)
            {
                setAction(dialog_rc);
            }

        }

    }
}
