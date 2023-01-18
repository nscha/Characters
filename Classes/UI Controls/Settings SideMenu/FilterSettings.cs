using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class FilterSettings : FlowTab
    {
        Dropdown FilterBehavior_Dropdown;
        Dropdown Matching_Dropdown;

        Checkbox Name_Checkbox;
        Checkbox Level_Checkbox;
        Checkbox Race_Checkbox;
        Checkbox Profession_Checkbox;
        Checkbox Map_Checkbox;
        Checkbox Crafting_Checkbox;
        Checkbox OnlyMaxCrafting_Checkbox;
        Checkbox Tags_Checkbox;

        public FilterSettings()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);


            Matching_Dropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            Matching_Dropdown.Items.Add("Match Any Filter");
            Matching_Dropdown.Items.Add("Match All Filter");
            Matching_Dropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterMatching.Value.GetMatchingBehavior();
            Matching_Dropdown.ValueChanged += Matching_Dropdown_ValueChanged;

            FilterBehavior_Dropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            FilterBehavior_Dropdown.Items.Add("Include Filters");
            FilterBehavior_Dropdown.Items.Add("Exclude Filters");
            FilterBehavior_Dropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterDirection.Value.GetFilterBehavior();
            FilterBehavior_Dropdown.ValueChanged += FilterBehavior_Dropdown_ValueChanged;

            Name_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Name.Value,
                Text = "Check Name",
            };
            Name_Checkbox.CheckedChanged += CheckedChanged;

            Level_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Level.Value,
                Text = "Check Level",
            };
            Level_Checkbox.CheckedChanged += CheckedChanged;

            Race_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Race.Value,
                Text = "Check Race",
            };
            Race_Checkbox.CheckedChanged += CheckedChanged;

            Profession_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Profession.Value,
                Text = "Check Profession",
            };
            Profession_Checkbox.CheckedChanged += CheckedChanged;


            Map_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Map.Value,
                Text = "Check Map",
            };
            Map_Checkbox.CheckedChanged += CheckedChanged;

            Crafting_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Crafting.Value,
                Text = "Check Crafting",
            };
            Crafting_Checkbox.CheckedChanged += CheckedChanged;
            OnlyMaxCrafting_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_OnlyMaxCrafting.Value,
                Text = "Check Only Max Crafting",
            };
            OnlyMaxCrafting_Checkbox.CheckedChanged += CheckedChanged;

            Tags_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Check_Tags.Value,
                Text = "Check Tags",
            };
            Tags_Checkbox.CheckedChanged += CheckedChanged;
        }

        private void Matching_Dropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.FilterMatching.Value = e.CurrentValue.GetMatchingBehavior();
            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void FilterBehavior_Dropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.FilterDirection.Value = e.CurrentValue.GetFilterBehavior();
            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void CheckedChanged(object sender, CheckChangedEvent e)
        {
            if (sender == Name_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Name.Value = e.Checked;
            }
            else if (sender == Level_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Level.Value = e.Checked;
            }
            else if (sender == Race_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Race.Value = e.Checked;
            }
            else if (sender == Profession_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Profession.Value = e.Checked;
            }
            else if (sender == Map_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Map.Value = e.Checked;
            }
            else if (sender == Crafting_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Crafting.Value = e.Checked;
            }
            else if (sender == OnlyMaxCrafting_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_OnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == Tags_Checkbox)
            {
                Characters.ModuleInstance.Settings.Check_Tags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }
    }
}
