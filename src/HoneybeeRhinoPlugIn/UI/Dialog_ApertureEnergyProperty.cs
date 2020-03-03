using Eto.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;
using HB = HoneybeeSchema;
using System;
namespace HoneybeeRhino.UI
{
    public class Dialog_ApertureEnergyProperty: Dialog<HB.ApertureEnergyPropertiesAbridged>
    {
     
        public Dialog_ApertureEnergyProperty(HB.ApertureEnergyPropertiesAbridged energyProp)
        {
            var EnergyProp = energyProp?? new HB.ApertureEnergyPropertiesAbridged();

            Padding = new Padding(5);
            Resizable = true;
            Title = "Aperture Energy Properties - Honeybee Rhino PlugIn";
            WindowStyle = WindowStyle.Default;
            MinimumSize = new Size(450, 200);

            //Get constructions
            var constructionSetDP = MakeDropDown(EnergyProp.Construction, (v) => EnergyProp.Construction = v?.Name,
                EnergyLibrary.StandardsOpaqueConstructions, "By Room ConstructionSet---------------------");


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
                    new Label() { Text = "Face Construction:" }, constructionSetDP,
                    new TableRow(buttons),
                    null
                }
            };
            
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
     

    }
}
