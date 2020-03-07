using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;
using System;
namespace HoneybeeRhino.UI
{
    public class Dialog_RoomEnergyProperty: Dialog<HB.RoomEnergyPropertiesAbridged>
    {
     
        public Dialog_RoomEnergyProperty(HB.RoomEnergyPropertiesAbridged roomEnergyProperties)
        {
            try
            {
                var EnergyProp = roomEnergyProperties ?? new HB.RoomEnergyPropertiesAbridged();

                Padding = new Padding(5);
                Resizable = true;
                Title = "Room Energy Properties - Honeybee Rhino PlugIn";
                WindowStyle = WindowStyle.Default;
                MinimumSize = new Size(450, 620);

                //Get constructions
                var constructionSetDP = MakeDropDown(EnergyProp.ConstructionSet, (v) => EnergyProp.ConstructionSet = v?.Name,
                    EnergyLibrary.DefaultConstructionSets, "By Global Model ConstructionSet");


                //Get programs
                var programTypesDP = MakeDropDown(EnergyProp.ProgramType, (v) => EnergyProp.ProgramType = v?.Name,
                   EnergyLibrary.DefaultProgramTypes, "Unoccupied, NoLoads");

                //Get HVACs
                var hvacDP = MakeDropDown(EnergyProp.Hvac, (v) => EnergyProp.Hvac = v?.Name,
                   EnergyLibrary.DefaultHVACs, "Unconditioned");


                var defaultByProgramType = "By Room Program Type";
                //Get people
                var peopleDP = MakeDropDown(EnergyProp.People, (v) => EnergyProp.People = v,
                    EnergyLibrary.DefaultPeopleLoads, defaultByProgramType);

                //Get lighting
                var lightingDP = MakeDropDown(EnergyProp.Lighting, (v) => EnergyProp.Lighting = v,
                    EnergyLibrary.DefaultLightingLoads, defaultByProgramType);

                //Get ElecEqp
                var elecEqpDP = MakeDropDown(EnergyProp.ElectricEquipment, (v) => EnergyProp.ElectricEquipment = v,
                    EnergyLibrary.DefaultElectricEquipmentLoads, defaultByProgramType);

                //Get gasEqp
                var gasEqpDP = MakeDropDown(EnergyProp.GasEquipment, (v) => EnergyProp.GasEquipment = v,
                    EnergyLibrary.GasEquipmentLoads, defaultByProgramType);

                //Get infiltration
                var infilDP = MakeDropDown(EnergyProp.Infiltration, (v) => EnergyProp.Infiltration = v,
                    EnergyLibrary.DefaultInfiltrationLoads, defaultByProgramType);


                //Get ventilation
                var ventDP = MakeDropDown(EnergyProp.Ventilation, (v) => EnergyProp.Ventilation = v,
                    EnergyLibrary.DefaultVentilationLoads, defaultByProgramType);

                //Get setpoint
                var setPtDP = MakeDropDown(EnergyProp.Setpoint, (v) => EnergyProp.Setpoint = v,
                    EnergyLibrary.DefaultSetpoints, defaultByProgramType);


                DefaultButton = new Button { Text = "OK" };
                DefaultButton.Click += (sender, e) => Close(EnergyProp);

                AbortButton = new Button { Text = "Cancel" };
                AbortButton.Click += (sender, e) => Close();

                var buttons = new TableLayout
                {
                    Padding = new Padding(5, 10, 5, 5),
                    Spacing = new Size(10, 10),
                    Rows = { new TableRow(null, this.DefaultButton, this.AbortButton, null) }
                };


                //Create layout
                Content = new TableLayout()
                {
                    Padding = new Padding(10),
                    Spacing = new Size(5, 5),
                    Rows =
                    {
                    new Label() { Text = "Room ConstructionSet:" }, constructionSetDP,
                        new Label() { Text = "Room Program Type:" }, programTypesDP,
                        new Label() { Text = "Room HVAC:" }, hvacDP,
                        new Label() { Text = " " },
                        new Label() { Text = "People [ppl/m2]:", }, peopleDP,
                        new Label() { Text = "Lighting [W/m2]:" }, lightingDP,
                        new Label() { Text = "Electric Equipment [W/m2]:" }, elecEqpDP,
                        new Label() { Text = "Gas Equipment [W/m2]:" }, gasEqpDP,
                        new Label() { Text = "Infiltration [m3/s per m2 facade @4Pa]:" }, infilDP,
                        new Label() { Text = "Ventilation [m3/s.m2]:" }, ventDP,
                        new Label() { Text = "Setpoint [C]:" }, setPtDP,
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
        private DropDown MakeDropDown<T>(string currentObjName, Action<T> setAction, IEnumerable<T> valueLibrary, string defaultItemName = default) where T : HB.INamed
        {
            var items = valueLibrary.ToList();
            var dropdownItems = items.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).ToList();
            var dp = new DropDown();

            if (!string.IsNullOrEmpty(defaultItemName))
            {
                var foundIndex = dropdownItems.FindIndex(_ => _.Text == defaultItemName);

                if (foundIndex > -1)
                {
                    //Add exist item from list
                    dp.SelectedIndex = foundIndex;
                }
                else
                {
                    //Add a default None item with a name
                    dp.Items.Add(defaultItemName);
                    dp.SelectedIndex = 0;
                }

            }

            dp.Items.AddRange(dropdownItems);

            dp.SelectedIndexBinding.Bind(
                () => items.FindIndex(_ => _.Name == currentObjName) + 1,
                (int i) => setAction(i == 0 ? default : items[i - 1])
                );

            return dp;

        }
        private DropDown MakeDropDown<T>(T currentValue, Action<T> setAction, IEnumerable<T> valueLibrary, string defaultItemName = default) where T : HB.INamed
        {
            return MakeDropDown(currentValue?.Name, setAction, valueLibrary, defaultItemName);
        }

        //private DropDown MakeDropDown<T>(List<T> Library, string defaultItemName = default) where T : HB.INamed
        //{
        //    var items = Library;
        //    var dropdownItems = items.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).ToList();
        //    var dp = new DropDown();

        //    if (!string.IsNullOrEmpty(defaultItemName))
        //    {
        //        var foundIndex = dropdownItems.FindIndex(_ => _.Text == defaultItemName);

        //        if (foundIndex > -1)
        //        {
        //            //Add exist item from list
        //            dp.SelectedIndex = foundIndex;
        //        }
        //        else
        //        {
        //            //Add a default None item with a name
        //            dp.Items.Add(defaultItemName);
        //            dp.SelectedIndex = 0;
        //        }

        //    }

        //    dp.Items.AddRange(dropdownItems);

        //    return dp;


        //}
    }
}
