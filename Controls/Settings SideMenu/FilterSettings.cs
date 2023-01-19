namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Text.RegularExpressions;
    using Blish_HUD.Controls;
    using Kenedia.Modules.Characters.Extensions;
    using Microsoft.Xna.Framework;
    using static Kenedia.Modules.Characters.Services.SettingsModel;
    using Point = Microsoft.Xna.Framework.Point;

    public class FilterSettings : FlowTab
    {
        private readonly Dropdown filterBehaviorDropdown;
        private readonly Dropdown matchingDropdown;
        private readonly Checkbox nameCheckbox;
        private readonly Checkbox levelCheckbox;
        private readonly Checkbox raceCheckbox;
        private readonly Checkbox professionCheckbox;
        private readonly Checkbox mapCheckbox;
        private readonly Checkbox craftingCheckbox;
        private readonly Checkbox onlyMaxCraftingCheckbox;
        private readonly Checkbox tagsCheckbox;

        public FilterSettings(int width = 190)
        {
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 5);
            this.Location = new Point(0, 25);

            this.matchingDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(MatchingBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = string.Join(" ", split);
                this.matchingDropdown.Items.Add(entry);
            }

            // this.matchingDropdown.Items.Add(Strings.common.MatchAnyFilter);
            // this.matchingDropdown.Items.Add(Strings.common.MatchAllFilter);
            this.matchingDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterMatching.Value.GetMatchingBehavior();
            this.matchingDropdown.ValueChanged += this.Matching_Dropdown_ValueChanged;

            this.filterBehaviorDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(FilterBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = string.Join(" ", split);
                this.filterBehaviorDropdown.Items.Add(entry);
            }

            // this.filterBehaviorDropdown.Items.Add(Strings.common.IncludeMatches);
            // this.filterBehaviorDropdown.Items.Add(Strings.common.ExcludeMatches);
            this.filterBehaviorDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterDirection.Value.GetFilterBehavior();
            this.filterBehaviorDropdown.ValueChanged += this.FilterBehavior_Dropdown_ValueChanged;

            this.nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckName.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Name),
            };
            this.nameCheckbox.CheckedChanged += this.CheckedChanged;

            this.levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckLevel.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Level),
            };
            this.levelCheckbox.CheckedChanged += this.CheckedChanged;

            this.raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckRace.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Race),
            };
            this.raceCheckbox.CheckedChanged += this.CheckedChanged;

            this.professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckProfession.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Profession),
            };
            this.professionCheckbox.CheckedChanged += this.CheckedChanged;

            this.mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckMap.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Map),
            };
            this.mapCheckbox.CheckedChanged += this.CheckedChanged;

            this.craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckCrafting.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.CraftingProfession),
            };
            this.craftingCheckbox.CheckedChanged += this.CheckedChanged;
            this.onlyMaxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value,
                Text = Strings.common.CheckOnlyMaxCrafting,
            };
            this.onlyMaxCraftingCheckbox.CheckedChanged += this.CheckedChanged;

            this.tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckTags.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Tags),
            };
            this.tagsCheckbox.CheckedChanged += this.CheckedChanged;
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
            if (sender == this.nameCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckName.Value = e.Checked;
            }
            else if (sender == this.levelCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckLevel.Value = e.Checked;
            }
            else if (sender == this.raceCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckRace.Value = e.Checked;
            }
            else if (sender == this.professionCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckProfession.Value = e.Checked;
            }
            else if (sender == this.mapCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckMap.Value = e.Checked;
            }
            else if (sender == this.craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckCrafting.Value = e.Checked;
            }
            else if (sender == this.onlyMaxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == this.tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }
    }
}
