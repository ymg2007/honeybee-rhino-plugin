using Eto.Drawing;
using Eto.Forms;
using Rhino.UI;
using Rhino.UI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoneybeeDotNet;
using System.Collections;

namespace HoneybeeRhino.UI
{
    internal class PropertyPanel: Eto.Forms.Panel
    {
        public PropertyPanel()
        {
        }

        public void updateRoomPanel(Room selectedRoom)
        {
            var room = selectedRoom;
            var layout = new DynamicLayout { };
            layout.Spacing = new Size(5, 5);
            layout.Padding = new Padding(10);
            layout.DefaultSpacing = new Size(2, 2);
            //layout.DefaultPadding = new Padding(10);


            var props = typeof(Room).GetProperties();
            foreach (var p in props)
            {
                var pType = p.PropertyType;
                if (pType.IsGenericType && (pType.GetGenericTypeDefinition() ==(typeof(List<>))))
                {
                    layout.AddSeparateRow(new Label { Text = p.Name });
                    //var t = new TextBox { Text = p.GetValue(room) as string };
                    var t2 = new Eto.Forms.ListBox();
                    var items = (IList)p.GetValue(room);
                    t2.Height = 50;
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var value = nameof(item);
                            t2.Items.Add(new ListItem() { Text = value });
                        }

                        if (items.Count > 0)
                        {
                            t2.Height = 80;
                        }
                       
                    }
                    
                    layout.AddSeparateRow(t2);
                }
                else
                {
                    layout.AddSeparateRow(new Label { Text = p.Name });
                    var t = new TextBox { Text = p.GetValue(room)?.ToString() };
                    layout.AddSeparateRow(t);
                }
            }
            

            layout.Add(null);
            var data_button = new Button { Text = "Honeybee Data" };
            data_button.Click += (sender, e) => Dialogs.ShowEditBox("Honeybee Data", "Honeybee Data can be shared across all platforms.", room.ToJson(), true, out string outJson);
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
