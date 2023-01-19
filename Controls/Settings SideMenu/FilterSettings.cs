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
        private readonly Dropdown _filterBehaviorDropdown;
        private readonly Dropdown _matchingDropdown;
        private readonly Checkbox _nameCheckbox;
        private readonly Checkbox _levelCheckbox;
        private readonly Checkbox _raceCheckbox;
        private readonly Checkbox _professionCheckbox;
        private readonly Checkbox _mapCheckbox;
        private readonly Checkbox _craftingCheckbox;
        private readonly Checkbox _onlyMaxCraftingCheckbox;
        private readonly Checkbox _tagsCheckbox;

        public FilterSettings(int width = 190)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            _matchingDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(MatchingBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                _matchingDropdown.Items.Add(entry);
            }

            // this.matchingDropdown.Items.Add(Strings.common.MatchAnyFilter);
            // this.matchingDropdown.Items.Add(Strings.common.MatchAllFilter);
            _matchingDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterMatching.Value.GetMatchingBehavior();
            _matchingDropdown.ValueChanged += Matching_Dropdown_ValueChanged;

            _filterBehaviorDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(FilterBehavior)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                _filterBehaviorDropdown.Items.Add(entry);
            }

            // this.filterBehaviorDropdown.Items.Add(Strings.common.IncludeMatches);
            // this.filterBehaviorDropdown.Items.Add(Strings.common.ExcludeMatches);
            _filterBehaviorDropdown.SelectedItem = Characters.ModuleInstance.Settings.FilterDirection.Value.GetFilterBehavior();
            _filterBehaviorDropdown.ValueChanged += FilterBehavior_Dropdown_ValueChanged;

            _nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckName.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Name),
            };
            _nameCheckbox.CheckedChanged += CheckedChanged;

            _levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckLevel.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Level),
            };
            _levelCheckbox.CheckedChanged += CheckedChanged;

            _raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckRace.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Race),
            };
            _raceCheckbox.CheckedChanged += CheckedChanged;

            _professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckProfession.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Profession),
            };
            _professionCheckbox.CheckedChanged += CheckedChanged;

            _mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckMap.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Map),
            };
            _mapCheckbox.CheckedChanged += CheckedChanged;

            _craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckCrafting.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.CraftingProfession),
            };
            _craftingCheckbox.CheckedChanged += CheckedChanged;
            _onlyMaxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value,
                Text = Strings.common.CheckOnlyMaxCrafting,
            };
            _onlyMaxCraftingCheckbox.CheckedChanged += CheckedChanged;

            _tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.CheckTags.Value,
                Text = string.Format(Strings.common.CheckItem, Strings.common.Tags),
            };
            _tagsCheckbox.CheckedChanged += CheckedChanged;
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
            if (sender == _nameCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckName.Value = e.Checked;
            }
            else if (sender == _levelCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckLevel.Value = e.Checked;
            }
            else if (sender == _raceCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckRace.Value = e.Checked;
            }
            else if (sender == _professionCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckProfession.Value = e.Checked;
            }
            else if (sender == _mapCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckMap.Value = e.Checked;
            }
            else if (sender == _craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckCrafting.Value = e.Checked;
            }
            else if (sender == _onlyMaxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == _tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.CheckTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }
    }
}
