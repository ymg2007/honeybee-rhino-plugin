using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.UI
{
    public class HBModelDialog: Dialog<Entities.ModelEntity>
    {
       
        public HBModelDialog(Entities.ModelEntity modelEntity)
        {
            var dup = modelEntity.Duplicate();
            var hbModel = dup.HBObject;

            Padding = new Padding(5);
            Resizable = true;
            Title = "Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(450, 620);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e)
                => Close(dup);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close();

       
            var buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(10, 10),
                Rows = { new TableRow(null, this.DefaultButton, this.AbortButton, null) }
            };

            hbModel.DisplayName = hbModel.DisplayName ?? "My Honeybee Model"; 
            var modelNameTextBox = new TextBox() { };
            modelNameTextBox.TextBinding.Bind(hbModel, m => m.DisplayName);

            var northNum = new NumericMaskedTextBox<double>() { };
            northNum.TextBinding.Bind(Binding.Delegate(() => hbModel.NorthAngle.ToString(), v => hbModel.NorthAngle = CheckIfNum(v)));

            var rooms = dup.RoomGroupEntities.Where(_ => _.Value.IsValid);
            var roomListBox = new ListBox();
            roomListBox.Height = 100;
            foreach (var item in rooms)
            {
                roomListBox.Items.Add(new ListItem() { Text = $"Room_{ item.Value.Guid }" });
            }
           
            //Create layout
            Content = new TableLayout
            {
                Padding = new Padding(10),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new Label(){ Text = $"ID: {hbModel.Name}"},
                    new Label(){ Text = "Model Name:"},
                    modelNameTextBox,
                    new Label(){ Text = "North Angle:"},
                    northNum,
                    new Label(){ Text = $"Rooms: [total: {rooms.Count()}]"},
                    roomListBox,
                    new TableRow(buttons),
                    null
                }
            };


        }

        private double CheckIfNum(string numString)
        {
            var isNum = double.TryParse(numString, out double numValue);
            return isNum ? numValue : 0;
        }

        //private DropDown MakeDropDown<T>(IEnumerable<T> Library, string defaultItemName = default) where T : HB.INamed
        //{

        //    var dropdownItems = Library.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text).ToList();
        //    var dp = new DropDown();

        //    if (!string.IsNullOrEmpty(defaultItemName))
        //    {
        //        var foundIndex = dropdownItems.FindIndex(_ => _.Text == defaultItemName);
                
        //        if (foundIndex > -1)
        //        {
        //            //Add exist item from list
        //            dp.Items.Add(dropdownItems[foundIndex]);
        //            dropdownItems.RemoveAt(foundIndex);
        //        }
        //        else
        //        {
        //            //Add a default None item with a name
        //            dp.Items.Add(defaultItemName);
        //        }

        //    }
            
        //    dp.Items.AddRange(dropdownItems);
        //    dp.SelectedIndex = 0;

        //    return dp;
        //}
    }
}
