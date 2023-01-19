namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Text.RegularExpressions;
    using Blish_HUD.Controls;
    using Kenedia.Modules.Characters.Extensions;
    using Microsoft.Xna.Framework;
    using static Kenedia.Modules.Characters.Services.SettingsModel;
    using Point = Microsoft.Xna.Framework.Point;

    public class OrderSettings : FlowTab
    {
        private readonly Dropdown orderDropdown;
        private readonly Dropdown flowDropdown;

        public OrderSettings(int width = 190)
        {
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 5);
            this.Location = new Point(0, 25);

            this.orderDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(ESortType)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = string.Join(" ", split);
                this.orderDropdown.Items.Add(entry);
            }

            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Name));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Tags));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Profession));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.LastLogin));
            // this.orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Map));
            // this.orderDropdown.Items.Add(Strings.common.Custom);
            this.orderDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortType.Value.GetSortType();
            this.orderDropdown.ValueChanged += this.OrderDropdown_ValueChanged;

            this.flowDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(ESortOrder)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = string.Join(" ", split);
                this.flowDropdown.Items.Add(entry);
            }

            // this.flowDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Ascending));
            // this.flowDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Descending));
            this.flowDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortOrder.Value.GetSortOrder();
            this.flowDropdown.ValueChanged += this.FlowDropdown_ValueChanged;
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
