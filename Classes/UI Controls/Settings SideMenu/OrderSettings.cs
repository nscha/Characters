using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;


namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class OrderSettings : FlowTab
    {
        Dropdown OrderDropdown;
        Dropdown FlowDropdown;

        public OrderSettings()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            OrderDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            OrderDropdown.Items.Add("Sort By Name");
            //OrderDropdown.Items.Add("Sort By Tag");
            OrderDropdown.Items.Add("Sort By Profession");
            OrderDropdown.Items.Add("Sort By Last Login");
            OrderDropdown.Items.Add("Sort By Map");
            OrderDropdown.Items.Add("Custom");
            OrderDropdown.SelectedItem = Characters.ModuleInstance.Settings.Sort_Type.Value.GetSortType();
            OrderDropdown.ValueChanged += OrderDropdown_ValueChanged;

            FlowDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            FlowDropdown.Items.Add("Sort Ascending");
            FlowDropdown.Items.Add("Sort Descending");
            FlowDropdown.SelectedItem = Characters.ModuleInstance.Settings.Sort_Order.Value.GetSortOrder();
            FlowDropdown.ValueChanged += FlowDropdown_ValueChanged;
        }

        private void FlowDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.Sort_Order.Value = e.CurrentValue.GetSortOrder();
            Characters.ModuleInstance.MainWindow?.SortCharacters();
        }

        private void OrderDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.Sort_Type.Value = e.CurrentValue.GetSortType();
            Characters.ModuleInstance.MainWindow?.SortCharacters();
        }
    }
}
