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
