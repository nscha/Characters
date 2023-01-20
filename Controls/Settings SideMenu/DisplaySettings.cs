using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class DisplaySettings : FlowTab
    {
        private readonly Dropdown _layoutDropdown;
        private readonly Dropdown _panelSizeDropdown;
        private readonly Checkbox _nameCheckbox;
        private readonly Checkbox _levelCheckbox;
        private readonly Checkbox _raceCheckbox;
        private readonly Checkbox _professionCheckbox;
        private readonly Checkbox _lastLoginCheckbox;
        private readonly Checkbox _mapCheckbox;
        private readonly Checkbox _craftingCheckbox;
        private readonly Checkbox _maxCraftingCheckbox;
        private readonly Checkbox _detailedTooltipCheckbox;
        private readonly Checkbox _tagsCheckbox;

        public DisplaySettings(int width = 190)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            _panelSizeDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            _panelSizeDropdown.Items.Add(Strings.common.Small);
            _panelSizeDropdown.Items.Add(Strings.common.Normal);
            _panelSizeDropdown.Items.Add(Strings.common.Large);
            _panelSizeDropdown.SelectedItem = Characters.ModuleInstance.Settings.PanelSize.Value.GetPanelSize();
            _panelSizeDropdown.ValueChanged += PanelSizeDropdown_ValueChanged;

            _layoutDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            _layoutDropdown.Items.Add(Strings.common.OnlyIcons);
            _layoutDropdown.Items.Add(Strings.common.OnlyText);
            _layoutDropdown.Items.Add(Strings.common.TextAndIcon);
            _layoutDropdown.SelectedItem = Characters.ModuleInstance.Settings.PanelLayout.Value.GetPanelLayout();
            _layoutDropdown.ValueChanged += LayoutDropdown_ValueChanged;

            _nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowName.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Name),
            };
            _nameCheckbox.CheckedChanged += CheckedChanged;

            _levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLevel.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Level),
            };
            _levelCheckbox.CheckedChanged += CheckedChanged;

            _raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowRace.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Race),
            };
            _raceCheckbox.CheckedChanged += CheckedChanged;

            _professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowProfession.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Profession),
            };
            _professionCheckbox.CheckedChanged += CheckedChanged;

            _lastLoginCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLastLogin.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.LastLogin),
            };
            _lastLoginCheckbox.CheckedChanged += CheckedChanged;

            _mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowMap.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Map),
            };
            _mapCheckbox.CheckedChanged += CheckedChanged;

            _craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowCrafting.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.CraftingProfession),
            };
            _craftingCheckbox.CheckedChanged += CheckedChanged;

            _maxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value,
                Text = Strings.common.ShowOnlyMaxCrafting,
            };
            _maxCraftingCheckbox.CheckedChanged += CheckedChanged;

            _tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowTags.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Tags),
            };
            _tagsCheckbox.CheckedChanged += CheckedChanged;

            _detailedTooltipCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.DetailedTooltip),
            };
            _detailedTooltipCheckbox.CheckedChanged += CheckedChanged;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _panelSizeDropdown?.Dispose();
            _layoutDropdown?.Dispose();
        }

        private void CheckedChanged(object sender, CheckChangedEvent e)
        {
            if (sender == _nameCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowName.Value = e.Checked;
            }
            else if (sender == _levelCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLevel.Value = e.Checked;
            }
            else if (sender == _raceCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowRace.Value = e.Checked;
            }
            else if (sender == _professionCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowProfession.Value = e.Checked;
            }
            else if (sender == _lastLoginCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLastLogin.Value = e.Checked;
            }
            else if (sender == _mapCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowMap.Value = e.Checked;
            }
            else if (sender == _craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowCrafting.Value = e.Checked;
            }
            else if (sender == _maxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == _detailedTooltipCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value = e.Checked;
            }
            else if (sender == _tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void LayoutDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.PanelLayout.Value = _layoutDropdown.SelectedItem.GetPanelLayout();
            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void PanelSizeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Characters.ModuleInstance.Settings.PanelSize.Value = _panelSizeDropdown.SelectedItem.GetPanelSize();
            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }
    }
}
