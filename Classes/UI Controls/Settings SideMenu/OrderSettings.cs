namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;

    public class OrderSettings : FlowTab
    {
        private readonly Dropdown orderDropdown;
        private readonly Dropdown flowDropdown;

        public OrderSettings()
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
                Width = 190,
            };
            this.orderDropdown.Items.Add("Sort By Name");
            // OrderDropdown.Items.Add("Sort By Tag");
            this.orderDropdown.Items.Add("Sort By Profession");
            this.orderDropdown.Items.Add("Sort By Last Login");
            this.orderDropdown.Items.Add("Sort By Map");
            this.orderDropdown.Items.Add("Custom");
            this.orderDropdown.SelectedItem = Characters.ModuleInstance.Settings.SortType.Value.GetSortType();
            this.orderDropdown.ValueChanged += this.OrderDropdown_ValueChanged;

            this.flowDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            this.flowDropdown.Items.Add("Sort Ascending");
            this.flowDropdown.Items.Add("Sort Descending");
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
