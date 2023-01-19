using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
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
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            matchingDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(MatchingBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                matchingDropdown.Items.Add(entry);
            }

            // this.matchingDropdown.Items.Add(Strings.common.MatchAnyFilter);
            // this.matchingDropdown.Items.Add(Strings.common.MatchAllFilter);
            matchingDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterMatching.Value.GetMatchingBehavior();
            matchingDropdown.ValueChanged += Matching_Dropdown_ValueChanged;

            filterBehaviorDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(FilterBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                filterBehaviorDropdown.Items.Add(entry);
            }

            // this.filterBehaviorDropdown.Items.Add(Strings.common.IncludeMatches);
            // this.filterBehaviorDropdown.Items.Add(Strings.common.ExcludeMatches);
            filterBehaviorDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterDirection.Value.GetFilterBehavior();
            filterBehaviorDropdown.ValueChanged += FilterBehavior_Dropdown_ValueChanged;

            nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckName.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Name),
            };
            nameCheckbox.CheckedChanged += CheckedChanged;

            levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckLevel.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Level),
            };
            levelCheckbox.CheckedChanged += CheckedChanged;

            raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckRace.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Race),
            };
            raceCheckbox.CheckedChanged += CheckedChanged;

            professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckProfession.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Profession),
            };
            professionCheckbox.CheckedChanged += CheckedChanged;

            mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckMap.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Map),
            };
            mapCheckbox.CheckedChanged += CheckedChanged;

            craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckCrafting.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.CraftingProfession),
            };
            craftingCheckbox.CheckedChanged += CheckedChanged;
            onlyMaxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value,
                Text = Strings.common.CheckOnlyMaxCrafting,
            };
            onlyMaxCraftingCheckbox.CheckedChanged += CheckedChanged;

            tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckTags.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Tags),
            };
            tagsCheckbox.CheckedChanged += CheckedChanged;
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
            if (sender == nameCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckName.Value = e.Checked;
            }
            else if (sender == levelCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckLevel.Value = e.Checked;
            }
            else if (sender == raceCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckRace.Value = e.Checked;
            }
            else if (sender == professionCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckProfession.Value = e.Checked;
            }
            else if (sender == mapCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckMap.Value = e.Checked;
            }
            else if (sender == craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckCrafting.Value = e.Checked;
            }
            else if (sender == onlyMaxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }
    }
}
