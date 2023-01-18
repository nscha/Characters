namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using System.Text.RegularExpressions;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Point = Microsoft.Xna.Framework.Point;

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

        public DisplaySettings()
        {
            this.FlowDirection = ControlFlowDirection.SingleTopToBottom;
            this.WidthSizingMode = SizingMode.Fill;
            this.AutoSizePadding = new Point(5, 5);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.OuterControlPadding = new Vector2(5, 5);
            this.ControlPadding = new Vector2(5, 5);
            this.Location = new Point(0, 25);

            this.panelSizeDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };

            foreach (string s in Enum.GetNames(typeof(PanelSizes)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = String.Join(" ", split);
                this.panelSizeDropdown.Items.Add(entry);
                if (s == Characters.ModuleInstance.Settings.PanelSize.Value.ToString())
                {
                    this.panelSizeDropdown.SelectedItem = entry;
                }
            }
            this.panelSizeDropdown.ValueChanged += this.PanelSizeDropdown_ValueChanged;

            this.layoutDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            foreach (string s in Enum.GetNames(typeof(CharacterPanelLayout)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = String.Join(" ", split);
                this.layoutDropdown.Items.Add(entry);

                if (s == Characters.ModuleInstance.Settings.PanelLayout.Value.ToString())
                {
                    this.layoutDropdown.SelectedItem = entry;
                }
            }
            this.layoutDropdown.ValueChanged += this.LayoutDropdown_ValueChanged;

            this.nameCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowName.Value,
                Text = "Show Name",
            };
            this.nameCheckbox.CheckedChanged += this.CheckedChanged;

            this.levelCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLevel.Value,
                Text = "Show Level",
            };
            this.levelCheckbox.CheckedChanged += this.CheckedChanged;

            this.raceCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowRace.Value,
                Text = "Show Race",
            };
            this.raceCheckbox.CheckedChanged += this.CheckedChanged;

            this.professionCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowProfession.Value,
                Text = "Show Profession",
            };
            this.professionCheckbox.CheckedChanged += this.CheckedChanged;

            this.lastLoginCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowLastLogin.Value,
                Text = "Show Last Login",
            };
            this.lastLoginCheckbox.CheckedChanged += this.CheckedChanged;

            this.mapCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowMap.Value,
                Text = "Show Map",
            };
            this.mapCheckbox.CheckedChanged += this.CheckedChanged;

            this.craftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowCrafting.Value,
                Text = "Show Crafting",
            };
            this.craftingCheckbox.CheckedChanged += this.CheckedChanged;

            this.maxCraftingCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value,
                Text = "Show Only Max Crafting",
            };
            this.maxCraftingCheckbox.CheckedChanged += this.CheckedChanged;

            this.tagsCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowTags.Value,
                Text = "Show Tags",
            };
            this.tagsCheckbox.CheckedChanged += this.CheckedChanged;

            this.detailedTooltipCheckbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value,
                Text = "Show Detailed Tooltip",
            };
            this.detailedTooltipCheckbox.CheckedChanged += this.CheckedChanged;
        }

        private void CheckedChanged(object sender, CheckChangedEvent e)
        {
            if (sender == this.nameCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowName.Value = e.Checked;
            }
            else if (sender == this.levelCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLevel.Value = e.Checked;
            }
            else if (sender == this.raceCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowRace.Value = e.Checked;
            }
            else if (sender == this.professionCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowProfession.Value = e.Checked;
            }
            else if (sender == this.lastLoginCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowLastLogin.Value = e.Checked;
            }
            else if (sender == this.mapCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowMap.Value = e.Checked;
            }
            else if (sender == this.craftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowCrafting.Value = e.Checked;
            }
            else if (sender == this.maxCraftingCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowOnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == this.detailedTooltipCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value = e.Checked;
            }
            else if (sender == this.tagsCheckbox)
            {
                Characters.ModuleInstance.Settings.ShowTags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void LayoutDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var layout = Regex.Replace(this.layoutDropdown.SelectedItem, @"\s+", "");
            CharacterPanelLayout result;

            if (Enum.TryParse(layout, out result))
            {
                Characters.ModuleInstance.Settings.PanelLayout.Value = result;
                Characters.ModuleInstance.MainWindow?.UpdateLayout();
            }
        }

        private void PanelSizeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var layout = Regex.Replace(this.panelSizeDropdown.SelectedItem, @"\s+", "");
            PanelSizes result;

            if (Enum.TryParse(layout, out result))
            {
                Characters.ModuleInstance.Settings.PanelSize.Value = result;
                Characters.ModuleInstance.MainWindow?.UpdateLayout();
            }
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.panelSizeDropdown?.Dispose();
            this.layoutDropdown?.Dispose();
        }
    }
}
