using Eto.Drawing;
using Eto.Forms;
using System.Linq;

namespace HoneybeeRhino.UI
{
    public class ProgramTypesDialog: Dialog<HoneybeeSchema.ProgramTypeAbridged>
    {
        private HoneybeeSchema.ProgramTypeAbridged _programType;
        public ProgramTypesDialog()
        {
            Padding = new Padding(5);
            Resizable = true;
            Title = "Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(400, 300);

            DefaultButton = new Button { Text = "OK" };
            DefaultButton.Click += (sender, e) => Close(this._programType);

            AbortButton = new Button { Text = "Cancel" };
            AbortButton.Click += (sender, e) => Close();
        

            var programs = Utility.DefaultProgramTypes.Select(_=> new ListItem() { Text = _.Name, Tag = _ }).OrderBy(_=>_.Text);
            var programTypesDropdown = new DropDown();
            programTypesDropdown.Items.AddRange(programs);
            programTypesDropdown.SelectedIndexChanged += 
                (s, e) => this._programType = (programTypesDropdown.Items[programTypesDropdown.SelectedIndex] as ListItem).Tag as HoneybeeSchema.ProgramTypeAbridged;


            var buttons = new TableLayout
            {
                Padding = new Padding(5, 10, 5, 5),
                Spacing = new Size(10, 10),
                Rows = { new TableRow(null,this.DefaultButton, this.AbortButton,null) }
            };

            Content = new TableLayout
            {
                Padding = new Padding(5),
                Spacing = new Size(5, 5),
                Rows =
                {
                    new Label(){ Text = "Select one room program type:"},
                    programTypesDropdown,
                    new TableRow(buttons),
                    null
                }
            };


        }

    }
}
