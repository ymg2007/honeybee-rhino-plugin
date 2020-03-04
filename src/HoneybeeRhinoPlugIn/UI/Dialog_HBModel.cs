using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;
using HoneybeeRhino.Entities;

namespace HoneybeeRhino.UI
{
    public class Dialog_HBModel: Dialog<Entities.ModelEntity>
    {
       
        public Dialog_HBModel(Entities.ModelEntity modelEntity)
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

            //Name
            hbModel.DisplayName = hbModel.DisplayName ?? "My Honeybee Model"; 
            var modelNameTextBox = new TextBox() { };
            modelNameTextBox.TextBinding.Bind(hbModel, m => m.DisplayName);

            //NorthAngle
            var northNum = new NumericMaskedTextBox<double>() { };
            northNum.TextBinding.Bind(Binding.Delegate(() => hbModel.NorthAngle.ToString(), v => hbModel.NorthAngle = CheckIfNum(v)));



            //Properties
            //Energy 
            var energyProp = hbModel.Properties.Energy;
            //TerrainType
            var terrainTypeDP = new EnumDropDown<HB.ModelEnergyProperties.TerrainTypeEnum>();
            terrainTypeDP.SelectedValueBinding.Bind(Binding.Delegate(() => energyProp.TerrainType.Value, v => energyProp.TerrainType = v));
        
            //Get constructions
            var gloConstrSetDP = MakeDropDown(hbModel.Properties.Energy.ConstructionSets, hbModel.Properties.Energy.GlobalConstructionSet);
            gloConstrSetDP.SelectedKeyBinding.Bind(hbModel, v => v.Properties.Energy.GlobalConstructionSet);

            ////Construction Set list
            //var ConstructionSetsListBox = new ListBox();
            //ConstructionSetsListBox.Height = 60;
            //ConstructionSetsListBox.bin
            //foreach (var item in hbModel.Properties.Energy.ConstructionSets)
            //{
            //    .Add(new ListItem() { Text = $"Room_{ item.Value.Guid }" });
            //}

            //Room list
            var rooms = dup.RoomEntities.Where(_ => _.Geometry().TryGetRoomEntity().IsValid);
            var roomListBox = new ListBox();
            roomListBox.Height = 100;
            foreach (var item in rooms)
            {
                roomListBox.Items.Add(new ListItem() { Text = $"Room_{ item.ObjectId }" });
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
                    new Label(){ Text = "Terrain Type:"},
                    terrainTypeDP,
                    new Label(){ Text = "Global ConstructionSet:"},
                    gloConstrSetDP,
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

        private DropDown MakeDropDown<T>(IEnumerable<T> Library, string defaultItemName = default) where T : HB.INamed
        {

            var dropdownItems = Library.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text).ToList();
            var dp = new DropDown();

            if (!string.IsNullOrEmpty(defaultItemName))
            {
                var foundIndex = dropdownItems.FindIndex(_ => _.Text == defaultItemName);

                if (foundIndex > -1)
                {
                    //Add exist item from list
                    dp.Items.Add(dropdownItems[foundIndex]);
                    dropdownItems.RemoveAt(foundIndex);
                }
                else
                {
                    //Add a default None item with a name
                    dp.Items.Add(defaultItemName);
                }

            }

            dp.Items.AddRange(dropdownItems);
            dp.SelectedIndex = 0;

            return dp;
        }
    }
}
