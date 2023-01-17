using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class DisplaySettings : FlowTab
    {
        Dropdown LayoutDropdown;
        Dropdown PanelSizeDropdown;
        Checkbox Name_Checkbox;
        Checkbox Level_Checkbox;
        Checkbox Race_Checkbox;
        Checkbox Profession_Checkbox;
        Checkbox LastLogin_Checkbox;
        Checkbox Map_Checkbox;
        Checkbox Crafting_Checkbox;
        Checkbox MaxCrafting_Checkbox;
        Checkbox DetailedTooltip_Checkbox;
        Checkbox Tags_Checkbox;

        public DisplaySettings()
        {
            FlowDirection = ControlFlowDirection.SingleTopToBottom;
            WidthSizingMode = SizingMode.Fill;
            AutoSizePadding = new Point(5, 5);
            HeightSizingMode = SizingMode.AutoSize;
            OuterControlPadding = new Vector2(5, 5);
            ControlPadding = new Vector2(5, 5);
            Location = new Point(0, 25);

            PanelSizeDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };

            foreach (string s in Enum.GetNames(typeof(PanelSizes)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = String.Join(" ", split);
                PanelSizeDropdown.Items.Add(entry);
                if (s == Characters.ModuleInstance.Settings.PanelSize.Value.ToString()) PanelSizeDropdown.SelectedItem = entry;
            }
            PanelSizeDropdown.ValueChanged += PanelSizeDropdown_ValueChanged;

            LayoutDropdown = new Dropdown()
            {
                Parent = this,
                Width = 190,
            };
            foreach (string s in Enum.GetNames(typeof(CharacterPanelLayout)))
            {
                string[] split = Regex.Split(s, @"(?<!^)(?=[A-Z])");
                var entry = String.Join(" ", split);
                LayoutDropdown.Items.Add(entry);

                if (s == Characters.ModuleInstance.Settings.PanelLayout.Value.ToString()) LayoutDropdown.SelectedItem = entry;
            }
            LayoutDropdown.ValueChanged += LayoutDropdown_ValueChanged;

            Name_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Name.Value,
                Text = "Show Name",
            };
            Name_Checkbox.CheckedChanged += CheckedChanged;

            Level_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Level.Value,
                Text = "Show Level",
            };
            Level_Checkbox.CheckedChanged += CheckedChanged;

            Race_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Race.Value,
                Text = "Show Race",
            };
            Race_Checkbox.CheckedChanged += CheckedChanged;

            Profession_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Profession.Value,
                Text = "Show Profession",
            };
            Profession_Checkbox.CheckedChanged += CheckedChanged;

            LastLogin_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_LastLogin.Value,
                Text = "Show Last Login",
            };
            LastLogin_Checkbox.CheckedChanged += CheckedChanged;

            Map_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Map.Value,
                Text = "Show Map",
            };
            Map_Checkbox.CheckedChanged += CheckedChanged;

            Crafting_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Crafting.Value,
                Text = "Show Crafting",
            };
            Crafting_Checkbox.CheckedChanged += CheckedChanged;

            MaxCrafting_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_OnlyMaxCrafting.Value,
                Text = "Show Only Max Crafting",
            };
            MaxCrafting_Checkbox.CheckedChanged += CheckedChanged;

            Tags_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_Tags.Value,
                Text = "Show Tags",
            };
            Tags_Checkbox.CheckedChanged += CheckedChanged;

            DetailedTooltip_Checkbox = new Checkbox()
            {
                Parent = this,
                Checked = Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value,
                Text = "Show Detailed Tooltip",
            };
            DetailedTooltip_Checkbox.CheckedChanged += CheckedChanged;
        }

        private void CheckedChanged(object sender, CheckChangedEvent e)
        {
            if (sender == Name_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Name.Value = e.Checked;
            }
            else if (sender == Level_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Level.Value = e.Checked;
            }
            else if (sender == Race_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Race.Value = e.Checked;
            }
            else if (sender == Profession_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Profession.Value = e.Checked;
            }
            else if (sender == LastLogin_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_LastLogin.Value = e.Checked;
            }
            else if (sender == Map_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Map.Value = e.Checked;
            }
            else if (sender == Crafting_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Crafting.Value = e.Checked;
            }
            else if (sender == MaxCrafting_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_OnlyMaxCrafting.Value = e.Checked;
            }
            else if (sender == DetailedTooltip_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value = e.Checked;
            }
            else if (sender == Tags_Checkbox)
            {
                Characters.ModuleInstance.Settings.Show_Tags.Value = e.Checked;
            }

            Characters.ModuleInstance.MainWindow?.UpdateLayout();
        }

        private void LayoutDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var layout = Regex.Replace(LayoutDropdown.SelectedItem, @"\s+", "");
            CharacterPanelLayout result;

            if (Enum.TryParse(layout, out result))
            {
                Characters.ModuleInstance.Settings.PanelLayout.Value = result;
                Characters.ModuleInstance.MainWindow?.UpdateLayout();
            }
        }

        private void PanelSizeDropdown_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var layout = Regex.Replace(PanelSizeDropdown.SelectedItem, @"\s+", "");
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

            PanelSizeDropdown?.Dispose();
            LayoutDropdown?.Dispose();
        }
    }
}
