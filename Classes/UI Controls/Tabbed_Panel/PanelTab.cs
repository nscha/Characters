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
    public class PanelTab : Panel
    {
        private TabButton _tabButton;
        public TabButton TabButton
        {
            get => _tabButton;
            private set => _tabButton = value;
        }

        event EventHandler IconChanged;
        private AsyncTexture2D _icon;
        public AsyncTexture2D Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                _tabButton.Icon = Icon;
                this.IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        event EventHandler TextureRectangleChanged;
        private Rectangle _textureRectangle = Rectangle.Empty;
        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set
            {
                _textureRectangle = value;
                _tabButton.TextureRectangle = value;
                this.TextureRectangleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _tabButton.BasicTooltipText = value;
            }
        }

        event EventHandler Activated;
        event EventHandler Deactivated;
        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                TabButton.Active = value;

                if (value) OnActivated();
                else OnDeactivated();
            }
        }

        public PanelTab()
        {
            _tabButton = new TabButton()
            {
                BasicTooltipText = Name,
            };
        }

        protected void OnActivated()
        {
            this.Show();
            this.Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeactivated()
        {
            this.Hide();
            this.Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _tabButton?.Dispose();
            _icon = null;
        }
    }
}
