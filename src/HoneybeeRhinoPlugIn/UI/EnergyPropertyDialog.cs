using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.UI
{
    public class EnergyPropertyDialog: Dialog<HB.RoomEnergyPropertiesAbridged>
    {
        private HB.ConstructionSetAbridged _constructionSet;
        private HB.ProgramTypeAbridged _programType;
        private HB.IdealAirSystemAbridged _idealAirSystem;
        private HB.PeopleAbridged _people;
        private HB.LightingAbridged _lighting;
        private HB.ElectricEquipmentAbridged _elecEqp;
        private HB.GasEquipmentAbridged _gasEqp;
        private HB.InfiltrationAbridged _infilt;
        private HB.VentilationAbridged _vent;
        private HB.SetpointAbridged _setpoint;
        public EnergyPropertyDialog()
        {
            Padding = new Padding(5);
            Resizable = true;
            Title = "Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(450, 600);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e)
                => Close(
                new HB.RoomEnergyPropertiesAbridged(
                    constructionSet: this._constructionSet?.Name, 
                    programType: this._programType?.Name, 
                    hvac: this._idealAirSystem?.Name,
                    people: this._people,
                    lighting: this._lighting,
                    electricEquipment:this._elecEqp,
                    gasEquipment: this._gasEqp,
                    infiltration: this._infilt,
                    ventilation: this._vent,
                    setpoint: this._setpoint
                    )
                );

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close();

            //Get constructions
            var constructions = EnergyLibrary.DefaultConstructionSets.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var constructionSetDropdown = new DropDown();
            constructionSetDropdown.Items.AddRange(constructions);
            constructionSetDropdown.SelectedIndexChanged +=
                (s, e) => this._constructionSet = (constructionSetDropdown.Items[constructionSetDropdown.SelectedIndex] as ListItem).Tag as HB.ConstructionSetAbridged;


            //Get programs
            var programs = EnergyLibrary.DefaultProgramTypes.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var programTypesDropdown = new DropDown();
            programTypesDropdown.Items.AddRange(programs);
            programTypesDropdown.SelectedIndexChanged +=
                (s, e) => this._programType = (programTypesDropdown.Items[programTypesDropdown.SelectedIndex] as ListItem).Tag as HB.ProgramTypeAbridged;


            //Get HVACs
            var hvacs = EnergyLibrary.DefaultHVACs.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var hvacsDropdown = new DropDown();
            hvacsDropdown.Items.AddRange(hvacs);
            hvacsDropdown.SelectedIndexChanged +=
                (s, e) => this._idealAirSystem = (hvacsDropdown.Items[hvacsDropdown.SelectedIndex] as ListItem).Tag as HB.IdealAirSystemAbridged;

            var defaultByProgramType = "By Default Program Type";
            //Get people
            var peopleDP = MakeDropDown(EnergyLibrary.DefaultPeopleLoads, defaultByProgramType);
            peopleDP.SelectedIndexChanged +=
                (s, e) => this._people = (peopleDP.Items[peopleDP.SelectedIndex] as ListItem).Tag as HB.PeopleAbridged;

            //Get lighting
            var lightingDP = MakeDropDown(EnergyLibrary.DefaultLightingLoads, defaultByProgramType);
            lightingDP.SelectedIndexChanged +=
                (s, e) => this._lighting = (lightingDP.Items[lightingDP.SelectedIndex] as ListItem).Tag as HB.LightingAbridged;

            //Get ElecEqp
            var elecEqpDP = MakeDropDown(EnergyLibrary.DefaultElectricEquipmentLoads, defaultByProgramType);
            elecEqpDP.SelectedIndexChanged +=
                (s, e) => this._elecEqp = (elecEqpDP.Items[elecEqpDP.SelectedIndex] as ListItem).Tag as HB.ElectricEquipmentAbridged;

            //Get gasEqp
            var gasEqpDP = MakeDropDown(EnergyLibrary.GasEquipmentLoads, defaultByProgramType);
            gasEqpDP.SelectedIndexChanged +=
                (s, e) => this._gasEqp = (gasEqpDP.Items[gasEqpDP.SelectedIndex] as ListItem).Tag as HB.GasEquipmentAbridged;

            //Get infiltration
            var infilDP = MakeDropDown(EnergyLibrary.DefaultInfiltrationLoads, defaultByProgramType);
            infilDP.SelectedIndexChanged +=
                (s, e) => this._infilt = (infilDP.Items[infilDP.SelectedIndex] as ListItem).Tag as HB.InfiltrationAbridged;

            //Get ventilation
            var ventDP = MakeDropDown(EnergyLibrary.DefaultVentilationLoads, defaultByProgramType);
            ventDP.SelectedIndexChanged +=
                (s, e) => this._vent = (ventDP.Items[ventDP.SelectedIndex] as ListItem).Tag as HB.VentilationAbridged;

            //Get setpoint
            var setPtDP = MakeDropDown(EnergyLibrary.DefaultSetpoints, defaultByProgramType);
            setPtDP.SelectedIndexChanged +=
                (s, e) => this._setpoint = (setPtDP.Items[setPtDP.SelectedIndex] as ListItem).Tag as HB.SetpointAbridged;


            var buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(10, 10),
                Rows = { new TableRow(null, this.DefaultButton, this.AbortButton, null) }
            };

            //Create layout
            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new Label(){ Text = "Default Room ConstructionSet:"},
                    constructionSetDropdown,
                    new Label(){ Text = "Default Program Type:"},
                    programTypesDropdown,
                    new Label(){ Text = "Default HVAC:"},
                    hvacsDropdown,
                    new Label(){ Text = " "},
                    new Label(){ Text = "People [ppl/m2]:"}, peopleDP,
                    new Label(){ Text = "Lighting [W/m2]:"}, lightingDP,
                    new Label(){ Text = "Electric Equipment [W/m2]:"}, elecEqpDP,
                    new Label(){ Text = "Gas Equipment [W/m2]:"}, gasEqpDP,
                    new Label(){ Text = "Infiltration [m3/s per m2 facade @4Pa]:"}, infilDP,
                    new Label(){ Text = "Ventilation [m3/s.m2]:"}, ventDP,
                    new Label(){ Text = "Setpoint [C]:"}, setPtDP,
                    new TableRow(buttons),
                    null
                }
            };


        }

        private DropDown MakeDropDown<T>(IEnumerable<T> Library, string defaultItem) where T : HB.IAbridged
        {

            var dropdownItems = Library.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var dp = new DropDown();

            if (!string.IsNullOrEmpty(defaultItem))
            {
                dp.Items.Add(defaultItem);
                dp.SelectedIndex = 0;
            }
            
            dp.Items.AddRange(dropdownItems);
           
            return dp;
        }
    }
}
