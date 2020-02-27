using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;
using System;
namespace HoneybeeRhino.UI
{
    public class EnergyPropertyDialog: Dialog<HB.RoomEnergyPropertiesAbridged>
    {
     
        public EnergyPropertyDialog(HB.RoomEnergyPropertiesAbridged roomEnergyProperties)
        {
            var EnergyProp = roomEnergyProperties?? new HB.RoomEnergyPropertiesAbridged();

            EnergyProp.Hvac = "Default HVAC System";
            Padding = new Padding(5);
            Resizable = true;
            Title = "Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(450, 620);

            //Get constructions
            var constrSets = EnergyLibrary.DefaultConstructionSets.ToList();
            var constructionSetDP = MakeDropDown(constrSets, "By Global Model ConstructionSet");
            constructionSetDP.SelectedIndexBinding.Bind(
                   () => constrSets.FindIndex(_ => _.Name == EnergyProp.ConstructionSet) + 1,
                   (int i) => EnergyProp.ConstructionSet = i == 0 ? null : constrSets[i - 1].Name
                   );

            //tb.Rows.Add(new Label() { Text = "Room ConstructionSet:" });
            //tb.Rows.Add(constructionSetDP);

            //Get programs
            var prgSets = EnergyLibrary.DefaultProgramTypes.ToList();
            var programTypesDP = MakeDropDown(prgSets, "Unoccupied, NoLoads");
            programTypesDP.SelectedIndexBinding.Bind(
                  () => prgSets.FindIndex(_ => _.Name == EnergyProp.ProgramType) + 1,
                  (int i) => EnergyProp.ProgramType = i == 0 ? null : prgSets[i - 1].Name
                  );
            //tb.Rows.Add(new Label() { Text = "Room Program Type:" });
            //tb.Rows.Add(programTypesDP);

            //Get HVACs
            var hvacSets = EnergyLibrary.DefaultHVACs.ToList();
            var hvacDP = MakeDropDown( hvacSets, "Unconditioned");
            hvacDP.SelectedIndexBinding.Bind(
                  () => hvacSets.FindIndex(_ => _.Name == EnergyProp.Hvac) + 1,
                  (int i) => EnergyProp.Hvac = i == 0 ? null : hvacSets[i - 1].Name
                  );
            //tb.Rows.Add(new Label() { Text = "Room HVAC:" });
            //tb.Rows.Add(hvacDP);

            var defaultByProgramType = "By Room Program Type";
            //Get people
            var ppls= EnergyLibrary.DefaultPeopleLoads.ToList();
            var peopleDP = MakeDropDown( ppls, defaultByProgramType);
            peopleDP.SelectedIndexBinding.Bind(
                () => ppls.FindIndex(_ => _.Name == EnergyProp.People?.Name) + 1,
                (int i) => EnergyProp.People = i == 0 ? null : ppls[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "People [ppl/m2]:" });
            //tb.Rows.Add(peopleDP);

            //Get lighting
            var lpds = EnergyLibrary.DefaultLightingLoads.ToList();
            var lightingDP = MakeDropDown(lpds, defaultByProgramType);
            lightingDP.SelectedIndexBinding.Bind(
                () => lpds.FindIndex(_ => _.Name == EnergyProp.Lighting?.Name) + 1,
                (int i) => EnergyProp.Lighting = i == 0 ? null : lpds[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Lighting [W/m2]:" });
            //tb.Rows.Add(lightingDP);

            //Get ElecEqp
            var eqps = EnergyLibrary.DefaultElectricEquipmentLoads.ToList();
            var elecEqpDP = MakeDropDown(eqps, defaultByProgramType);
            elecEqpDP.SelectedIndexBinding.Bind(
                () => eqps.FindIndex(_ => _.Name == EnergyProp.ElectricEquipment?.Name) + 1,
                (int i) => EnergyProp.ElectricEquipment = i == 0 ? null : eqps[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Electric Equipment [W/m2]:" });
            //tb.Rows.Add(elecEqpDP);

            //Get gasEqp
            var gases = EnergyLibrary.GasEquipmentLoads.ToList();
            var gasEqpDP = MakeDropDown(gases, defaultByProgramType);
            gasEqpDP.SelectedIndexBinding.Bind(
                () => gases.FindIndex(_ => _.Name == EnergyProp.GasEquipment?.Name) + 1,
                (int i) => EnergyProp.GasEquipment = i == 0 ? null : gases[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Gas Equipment [W/m2]:" });
            //tb.Rows.Add(gasEqpDP);

            //Get infiltration
            var infls = EnergyLibrary.DefaultInfiltrationLoads.ToList();
            var infilDP = MakeDropDown(infls, defaultByProgramType);
            infilDP.SelectedIndexBinding.Bind(
                () => infls.FindIndex(_ => _.Name == EnergyProp.Infiltration?.Name) + 1,
                (int i) => EnergyProp.Infiltration = i == 0 ? null : infls[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Infiltration [m3/s per m2 facade @4Pa]:" });
            //tb.Rows.Add(infilDP);


            //Get ventilation
            var vents = EnergyLibrary.DefaultVentilationLoads.ToList();
            var ventDP = MakeDropDown(vents, defaultByProgramType);
            ventDP.SelectedIndexBinding.Bind(
                () => vents.FindIndex(_ => _.Name == EnergyProp.Ventilation?.Name) + 1,
                (int i) => EnergyProp.Ventilation = i == 0 ? null : vents[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Ventilation [m3/s]:" });
            //tb.Rows.Add(ventDP);

            //Get setpoint
            var stps = EnergyLibrary.DefaultSetpoints.ToList();
            var setPtDP = MakeDropDown(stps, defaultByProgramType);
            setPtDP.SelectedIndexBinding.Bind(
                () => stps.FindIndex(_ => _.Name == EnergyProp.Setpoint?.Name) + 1,
                (int i) => EnergyProp.Setpoint = i == 0 ? null : stps[i - 1]
                );
            //tb.Rows.Add(new Label() { Text = "Setpoint [C]:" });
            //tb.Rows.Add(ventDP);



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

        private DropDown MakeDropDown<T>(List<T> Library, string defaultItemName = default) where T : HB.INamed
        {
            var items = Library;
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

            ////Set to saved item
            //if (bindObj != null)
            //{
            //    var selIndex = dp.SelectedIndex;
            //}
            

            dp.Items.AddRange(dropdownItems);

            return dp;
            //Action<int> func = (int i) =>
            //{
            //    if (bindObj is string)
            //    {
            //        bindObj = i == 0 ? default : items[i - 1].Name;
            //    }
            //    else
            //    {
            //        bindObj = i == 0 ? default : items[i - 1];
            //    }

            //};
            //dp.SelectedIndexBinding.Bind(
            //    () => items.FindIndex(_ => _.Name == dp.SelectedKey) + 1,
            //    (int i)=> bindObj = i == 0 ? default : items[i - 1]
            //    );
            //return dp;

            //void setValueByIndex(int i)
            //{
            //    SetValue(ref bindObj, items[i - 1]);

            //};


        }

        //private void SetValue<T>(ref object bindObj, T item)
        //{
        //    if (bindObj is string)
        //    {
        //        bindObj = i == 0 ? default : items[i - 1].Name;
        //    }
        //    else
        //    {
        //        bindObj = i == 0 ? default : items[i - 1];
        //    }
        //}
    }
}
