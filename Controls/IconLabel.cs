using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class IconLabel : Control
    {
        private AsyncTexture2D icon;
        private BitmapFont font = GameService.Content.DefaultFont14;
        private string text;
        private Rectangle iconRectangle = Rectangle.Empty;
        private Rectangle textRectangle = Rectangle.Empty;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                UpdateLayout();
            }
        }

        public Color TextColor { get; set; } = Color.White;

        public AsyncTexture2D Icon
        {
            get => icon;
            set
            {
                icon = value;
                if (value != null)
                {
                    UpdateLayout();
                }
            }
        }

        public BitmapFont Font
        {
            get => font;
            set
            {
                font = value;
                if (value != null)
                {
                    UpdateLayout();
                }
            }
        }

        public bool AutoSizeWidth { get; set; }

        public bool AutoSizeHeight { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            AsyncTexture2D texture = Icon;
            if (texture != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    iconRectangle,
                    TextureRectangle == Rectangle.Empty ? texture.Bounds : TextureRectangle,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawStringOnCtrl(
                    this,
                    Text,
                    Font,
                    textRectangle,
                    TextColor,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
        }

        private void UpdateLayout()
        {
            Width = Math.Max((int)Font.MeasureString(Text).Width + 4 + (Icon == null ? 0 : Height + 5), Height);

            iconRectangle = Icon == null ? Rectangle.Empty : new Rectangle(2, 2, LocalBounds.Height - 4, LocalBounds.Height - 4);
            textRectangle = new Rectangle(iconRectangle.Right + (Icon == null ? 0 : 5), 2, LocalBounds.Width - (iconRectangle.Right + (Icon == null ? 0 : 5) + 2), LocalBounds.Height - 4);
        }
    }
}
