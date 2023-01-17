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
    class HeadedPanel : Panel
    {
        AsyncTexture2D _headerUnderline;
        public bool TintContent = false;

        bool _initialized = false;
        Label _headerLabel = new Label()
        {
            Font = GameService.Content.DefaultFont16,
            Location = new Point(5, 3),
            AutoSizeWidth = true,
            AutoSizeHeight = true,
            Padding = new Thickness(4f)
        };
        FlowPanel _contentPanel = new FlowPanel()
        {            
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            OuterControlPadding = new Vector2(0,5),
            ControlPadding = new Vector2(4, 4),
            AutoSizePadding = new Point(0,5),
            Location = new Point(0, 25),
        };

        private string _header = "";
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                _headerLabel.Text = value;
            }
        }

        public HeadedPanel()
        {
            WidthSizingMode = SizingMode.Fill;
            HeightSizingMode = SizingMode.AutoSize;

            _headerLabel.Parent = this;
            _contentPanel.Parent = this;

            _headerUnderline = Characters.ModuleInstance.TextureManager.getControlTexture(_Controls.Separator);

            _initialized = true;
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            if (_initialized)
            {
                e.ChangedChild.Parent = _contentPanel;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);


            var color = Color.Black;
            var b = _contentPanel.LocalBounds;
            var pad = 5;

            if (TintContent)
            {
                //Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 2, b.Width - pad, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 1, b.Width - pad, 1), Rectangle.Empty, color * 0.6f);

                //Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top + 4, 2, b.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top + 4, 1, b.Height), Rectangle.Empty, color * 0.6f);

                //Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 2 - pad, b.Top + 2, 2, b.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 1 - pad, b.Top + 2, 1, b.Height), Rectangle.Empty, color * 0.6f);

                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width - pad, b.Height), Rectangle.Empty, color * 0.5f);
            }

            b = _headerLabel.LocalBounds;

            spriteBatch.DrawOnCtrl(this,
                    _headerUnderline,
                    new Rectangle(bounds.Left, b.Bottom, bounds.Width, 5),
                    _headerUnderline.Bounds,
                    Color.White,
                    0f,
                    default);
        }
    }
}
