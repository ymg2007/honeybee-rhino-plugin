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
            try
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
                hbModel.DisplayName = hbModel.DisplayName ?? string.Empty;
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
                var gloConstrSetDP = DialogHelper.MakeDropDown(hbModel.Properties.Energy.GlobalConstructionSet, (v) => hbModel.Properties.Energy.GlobalConstructionSet = v?.Name, 
                    hbModel.Properties.Energy.ConstructionSets, hbModel.Properties.Energy.GlobalConstructionSet);

                ////Construction Set list
                //var ConstructionSetsListBox = new ListBox();
                //ConstructionSetsListBox.Height = 60;
                //ConstructionSetsListBox.bin
                //foreach (var item in hbModel.Properties.Energy.ConstructionSets)
                //{
                //    .Add(new ListItem() { Text = $"Room_{ item.Value.Guid }" });
                //}

                //Room list
                var rooms = dup.RoomEntitiesWithoutHistory;
                var roomListBox = new ListBox();
                roomListBox.Height = 100;
                foreach (var item in rooms)
                {
                    var room = item.TryGetRoomEntity().HBObject;
                    var displayName = room.DisplayName ?? string.Empty;
                    roomListBox.Items.Add(new ListItem() { Text = displayName });
                }

                //Shade list
                var shades = dup.OrphanedShadesWithoutHistory;
                var shadeListBox = new ListBox();
                shadeListBox.Height = 100;
                foreach (var item in shades)
                {
                    var displayName = item.TryGetShadeEntity().HBObject.DisplayName ?? string.Empty;
                    shadeListBox.Items.Add(new ListItem() { Text = displayName });
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
                        new Label(){ Text = "Global Construction Set:"},
                        gloConstrSetDP,
                        new Label(){ Text = "North Angle:"},
                        northNum,
                        new Label(){ Text = $"Rooms: [total: {rooms.Count()}]"},
                        roomListBox,
                        new Label(){ Text = $"Shades:"},
                        shadeListBox,
                        new TableRow(buttons),
                        null
                    }
                };
            }
            catch (Exception e)
            {
                Rhino.RhinoApp.WriteLine(e.Message);
            }
            


        }

        private double CheckIfNum(string numString)
        {
            var isNum = double.TryParse(numString, out double numValue);
            return isNum ? numValue : 0;
        }

    
    }
}
