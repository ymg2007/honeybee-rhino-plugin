using Eto.Drawing;
using Eto.Forms;
using System.Linq;
using HB = HoneybeeSchema;

namespace HoneybeeRhino.UI
{
    public class EnergyPropertyDialog: Dialog<HB.RoomEnergyPropertiesAbridged>
    {
        private HB.ConstructionSetAbridged _constructionSet;
        private HB.ProgramTypeAbridged _programType;
        private HB.IdealAirSystemAbridged _idealAirSystem;

        public EnergyPropertyDialog()
        {
            Padding = new Padding(5);
            Resizable = true;
            Title = "Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(400, 300);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) 
                => Close(
                new HB.RoomEnergyPropertiesAbridged(constructionSet: this._constructionSet?.Name, programType: this._programType?.Name, hvac: this._idealAirSystem?.Name)
                );

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close();

            //Get constructions
            var constructions = EnergyLibrary.DefaultConstructionSets.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var constructionSetDropdown = new DropDown();
            constructionSetDropdown.Items.AddRange(constructions);
            constructionSetDropdown.SelectedIndexChanged +=
                (s, e) => this._constructionSet = (constructionSetDropdown.Items[constructionSetDropdown.SelectedIndex] as ListItem).Tag as HoneybeeSchema.ConstructionSetAbridged;


            //Get programs
            var programs = EnergyLibrary.DefaultProgramTypes.Select(_=> new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_=>_.Text);
            var programTypesDropdown = new DropDown();
            programTypesDropdown.Items.AddRange(programs);
            programTypesDropdown.SelectedIndexChanged += 
                (s, e) => this._programType = (programTypesDropdown.Items[programTypesDropdown.SelectedIndex] as ListItem).Tag as HoneybeeSchema.ProgramTypeAbridged;

            
            //Get HVACs
            var hvacs = EnergyLibrary.DefaultHVACs.Select(_ => new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_ => _.Text);
            var hvacsDropdown = new DropDown();
            hvacsDropdown.Items.AddRange(hvacs);
            hvacsDropdown.SelectedIndexChanged +=
                (s, e) => this._idealAirSystem = (hvacsDropdown.Items[hvacsDropdown.SelectedIndex] as ListItem).Tag as HoneybeeSchema.IdealAirSystemAbridged;


            var buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(10, 10),
                Rows = { new TableRow(null,this.DefaultButton, this.AbortButton,null) }
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
                    new TableRow(buttons),
                    null
                }
            };


        }

    }
}
