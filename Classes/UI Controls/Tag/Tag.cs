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
using MonoGame.Extended.BitmapFonts;
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
    public class Tag : FlowPanel
    {
        public BitmapFont Font
        {
            get { return _text.Font; }
            set { _text.Font = value; }
        }

        private Color _disabledColor = new Color(156, 156, 156);
        public bool Active = true;
        public bool CanInteract = true;
        private Texture2D _disabledBackground;
        private AsyncTexture2D _background;
        public AsyncTexture2D Background
        {
            get => _background;
            set
            {
                _background = value;
                if(value != null)
                {
                    CreateDisabledBackground(null, null);
                    _background.TextureSwapped += CreateDisabledBackground;
                }
            }
        }

        private void CreateDisabledBackground(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _disabledBackground = _background.Texture.ToGrayScaledPalettable();
            _background.TextureSwapped -= CreateDisabledBackground;
        }

        private Label _text;
        private ImageButton _delete;
        private ImageButton _dummy;
        public bool ShowDelete
        {
            get => _delete.Visible;
            set
            {
                if (_delete != null) 
                {
                    _delete.Visible = value;
                    _dummy.Visible = !value;
                };
            }
        }

        public string Text
        {
            get => _text != null ? _text.Text : null;
            set
            {
                if (_text != null) _text.Text = value;
            }
        }
        public event EventHandler Deleted;

        public Tag()
        {
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(1620622);
            WidthSizingMode = SizingMode.AutoSize;
            FlowDirection = ControlFlowDirection.SingleLeftToRight;
            OuterControlPadding = new Vector2(3, 3);
            ControlPadding = new Vector2(2, 0);
            AutoSizePadding = new Point(5, 3);
            Height = 26;

            _delete = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156012),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156011),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                Size = new Point(20, 20),
                BasicTooltipText = "Remove Tag",
            };
            _delete.Click += _delete_Click;

            _dummy = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025),
                TextureRectangle = new Rectangle(44, 48, 43, 46),
                Size = new Point(20, 20),
                Visible = false,
            };

            _text = new Label()
            {
                Parent = this,
                AutoSizeWidth = true,
                Height = Height - (int) OuterControlPadding.Y,
            };
        }

        private void _delete_Click(object sender, MouseEventArgs e)
        {
            this.Deleted?.Invoke(this, EventArgs.Empty);
            Dispose();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_background != null)
            {
                var texture = Active ? _background : _disabledBackground != null ? _disabledBackground : _background;

                spriteBatch.DrawOnCtrl(this, texture, bounds, bounds, Active ? Color.White * 0.98f : _disabledColor * 0.8f);
            }

            var color = Color.Black;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (CanInteract)
            {
                base.OnClick(e); 
                Active = !Active;
            }                
        }
    }
}
