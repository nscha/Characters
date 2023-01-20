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
        private readonly Dropdown _orderDropdown;
        private readonly Dropdown _flowDropdown;

        public OrderSettings(int width = 190)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            _orderDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            _orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Name));
            _orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Tags));
            _orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Profession));
            _orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.LastLogin));
            _orderDropdown.Items.Add(string.Format(Strings.common.SortBy, Strings.common.Map));
            _orderDropdown.Items.Add(Strings.common.Custom);
            _orderDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortType.Value.GetSortType();
            _orderDropdown.ValueChanged += OrderDropdown_ValueChanged;

            _flowDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            _flowDropdown.Items.Add(Strings.common.Ascending);
            _flowDropdown.Items.Add(Strings.common.Descending);
            _flowDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortOrder.Value.GetSortOrder();
            _flowDropdown.ValueChanged += FlowDropdown_ValueChanged;
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
