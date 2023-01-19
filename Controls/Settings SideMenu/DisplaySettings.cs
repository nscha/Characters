using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using System;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class DisplaySettings : FlowTab
    {
        private readonly Dropdown layoutDropdown;
        private readonly Dropdown panelSizeDropdown;
        private readonly Checkbox nameCheckbox;
        private readonly Checkbox levelCheckbox;
        private readonly Checkbox raceCheckbox;
        private readonly Checkbox professionCheckbox;
        private readonly Checkbox lastLoginCheckbox;
        private readonly Checkbox mapCheckbox;
        private readonly Checkbox craftingCheckbox;
        private readonly Checkbox maxCraftingCheckbox;
        private readonly Checkbox detailedTooltipCheckbox;
        private readonly Checkbox tagsCheckbox;

        public DisplaySettings(int width = 190)
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            panelSizeDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };

            foreach (string s in Enum.GetNames(typeof(PanelSizes)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                panelSizeDropdown.Items.Add(entry);
                if (s == Characters.ModuleInstance.Settings.PanelSize.Value.ToString())
                {
                    panelSizeDropdown.SelectedItem = entry;
                }
            }

            panelSizeDropdown.ValueChanged += PanelSizeDropdown_ValueChanged;

            layoutDropdown = new Dropdown()
            {
                Parent = this,
                Width = width,
            };
            foreach (string s in Enum.GetNames(typeof(CharacterPanelLayout)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                string entry = string.Join(" ", split);
                layoutDropdown.Items.Add(entry);

                if (s == Characters.ModuleInstance.Settings.PanelLayout.Value.ToString())
                {
                    layoutDropdown.SelectedItem = entry;
                }
            }

            layoutDropdown.ValueChanged += LayoutDropdown_ValueChanged;

            nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowName.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Name),
            };
            nameCheckbox.CheckedChanged += CheckedChanged;

            levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLevel.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Level),
            };
            levelCheckbox.CheckedChanged += CheckedChanged;

            raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowRace.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Race),
            };
            raceCheckbox.CheckedChanged += CheckedChanged;

            professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowProfession.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Profession),
            };
            professionCheckbox.CheckedChanged += CheckedChanged;

            lastLoginCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLastLogin.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.LastLogin),
            };
            lastLoginCheckbox.CheckedChanged += CheckedChanged;

            mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowMap.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Map),
            };
            mapCheckbox.CheckedChanged += CheckedChanged;

            craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowCrafting.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.CraftingProfession),
            };
            craftingCheckbox.CheckedChanged += CheckedChanged;

            maxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value,
                Text = Strings.common.ShowOnlyMaxCrafting,
            };
            maxCraftingCheckbox.CheckedChanged += CheckedChanged;

            tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowTags.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.Tags),
            };
            tagsCheckbox.CheckedChanged += CheckedChanged;

            detailedTooltipCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value,
                Text = string.Format(Strings.common.ShowItem, Strings.common.DetailedTooltip),
            };
            detailedTooltipCheckbox.CheckedChanged += CheckedChanged;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            panelSizeDropdown?.Dispose();
            layoutDropdown?.Dispose();
        }

        private void CheckedChanged(object sender, CheckChangedEvent e)
        {
            if (sender == nameCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowName.Value = e.Checked;
            }
            else if (sender == levelCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLevel.Value = e.Checked;
            }
            else if (sender == raceCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowRace.Value = e.Checked;
            }
            else if (sender == professionCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowProfession.Value = e.Checked;
            }
            else if (sender == lastLoginCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLastLogin.Value = e.Checked;
            }
            else if (sender == mapCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowMap.Value = e.Checked;
            }
            else if (sender == craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowCrafting.Value = e.Checked;
            }
            else if (sender == maxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == detailedTooltipCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value = e.Checked;
            }
            else if (sender == tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void LayoutDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            string layout = Regex.Replace(layoutDropdown.SelectedItem, @"\s+", string.Empty);
            CharacterPanelLayout result;

            if (Enum.TryParse(layout, out result))
            {
                Characters.ModuleInstance.Settings.PanelLayout.Value = result;
                Characters.ModuleInstance.MainWindow?.UpdateLayout();
            }
        }

        private void PanelSizeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            string layout = Regex.Replace(panelSizeDropdown.SelectedItem, @"\s+", string.Empty);
            PanelSizes result;

            if (Enum.TryParse(layout, out result))
            {
                Characters.ModuleInstance.Settings.PanelSize.Value = result;
                Characters.ModuleInstance.MainWindow?.UpdateLayout();
            }
        }
    }
}
