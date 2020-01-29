using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using Rhino.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HoneybeeRhino.UI
{
    internal class PropertyPanel: Eto.Forms.Panel
    {
        public PropertyPanel()
        {
        }

        public void updateRoomPanel(Rhino.Geometry.GeometryBase selectedRoom)
        {
            var json = selectedRoom.GetHBJson();
            var room = HoneybeeDotNet.Model.Room.FromJson(json);
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);
            //layout.DefaultPadding = new Padding(10);

            layout.AddSeparateRow(new Label { Text = "Name" });
            layout.AddSeparateRow(new TextBox { Text = room.Name });

            layout.AddSeparateRow(new Label { Text = "Properties" });
            layout.AddSeparateRow(new TextBox { Text = room.Properties?.ToJson() });

            layout.AddSeparateRow(new Label { Text = "Display Name" });
            layout.AddSeparateRow(new TextBox { Text = room.DisplayName });

            layout.AddSeparateRow(new Label { Text = "Indoor Shades" });
            layout.AddSeparateRow(new TextBox { Text = room.IndoorShades?.Any().ToString() });

            layout.AddSeparateRow(new Label { Text = "Outdoor Shades" });
            layout.AddSeparateRow(new TextBox { Text = room.OutdoorShades?.Any().ToString() });

            layout.AddSeparateRow(new Label { Text = "Multiplier" });
            layout.AddSeparateRow(new TextBox { Text = room.Multiplier.ToString() });

            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", json, true, out string outJson);
            layout.AddSeparateRow(data_button, null);


            this.Content = layout;
            //layout.up

        }

        protected void OnHelloButton()
        {
            // Use the Rhino common message box and NOT the Eto MessageBox,
            // the Eto version expects a top level Eto Window as the owner for
            // the MessageBox and will cause problems when running on the Mac.
            // Since this panel is a child of some Rhino container it does not
            // have a top level Eto Window.
            Dialogs.ShowMessage("Hello Rhino!", "Sample");
        }

        /// <summary>
        /// Sample of how to display a child Eto dialog
        /// </summary>
        protected void OnChildButton()
        {
            var dialog = new SampleCsEtoHelloWorld();
            dialog.ShowModal(this);
        }

        class SampleCsEtoHelloWorld : CommandDialog
        {
            public SampleCsEtoHelloWorld()
            {
                Padding = new Padding(10);
                Title = "Hello World";
                Resizable = false;
                Content = new StackLayout()
                {
                    Padding = new Padding(0),
                    Spacing = 6,
                    Items =
                    {
                      new Label { Text="This is a child dialog..." }
                    }
                };
            }
        }
    }
}
