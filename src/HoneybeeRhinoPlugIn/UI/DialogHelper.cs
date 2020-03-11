using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;

namespace HoneybeeRhino.UI
{
    public static class DialogHelper
    {
        public static DropDown MakeDropDown<T>(T currentValue, Action<T> setAction, IEnumerable<T> valueLibrary, string defaultItemName = default) where T : HoneybeeSchema.INamed
        {
            return MakeDropDown(currentValue?.Name, setAction, valueLibrary, defaultItemName);
        }
        public static DropDown MakeDropDown<T>(string currentObjName, Action<T> setAction, IEnumerable<T> valueLibrary, string defaultItemName = default) where T : HoneybeeSchema.INamed
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
