using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class OrderSettings : FlowTab
    {
        private readonly Dropdown orderDropdown;
        private readonly Dropdown flowDropdown;

        public OrderSettings(int width = 190)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            orderDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(ESortType)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                orderDropdown.Items.Add(entry);
            }

            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Name));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Tags));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Profession));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.LastLogin));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Map));
            // this.orderDropdown.Items.Add(Strings.common.Custom);
            orderDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortType.Value.GetSortType();
            orderDropdown.ValueChanged += OrderDropdown_ValueChanged;

            flowDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(ESortOrder)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                flowDropdown.Items.Add(entry);
            }

            // this.flowDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Ascending));
            // this.flowDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Descending));
            flowDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortOrder.Value.GetSortOrder();
            flowDropdown.ValueChanged += FlowDropdown_ValueChanged;
        }

        private void FlowDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.SortOrder.Value = e.CurrentValue.GetSortOrder();
            Characters.ModuleInstance.MainWindow?.SortCharacters();
        }

        private void OrderDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.SortType.Value = e.CurrentValue.GetSortType();
            Characters.ModuleInstance.MainWindow?.SortCharacters();
        }
    }
}
