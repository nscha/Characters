namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class IconLabel : Control
    {
        private AsyncTexture2D icon;
        private BitmapFont font = GameService.Content.DefaultFont14;
        private string text;
        private Rectangle iconRectangle = Rectangle.Empty;
        private Rectangle textRectangle = Rectangle.Empty;

        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.UpdateLayout();
            }
        }

        public Color TextColor { get; set; } = Color.White;

        public AsyncTexture2D Icon
        {
            get => this.icon;
            set
            {
                this.icon = value;
                if (value != null)
                {
                    this.UpdateLayout();
                }
            }
        }

        public BitmapFont Font
        {
            get => this.font;
            set
            {
                this.font = value;
                if (value != null)
                {
                    this.UpdateLayout();
                }
            }
        }

        public bool AutoSizeWidth { get; set; }

        public bool AutoSizeHeight { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var texture = this.Icon;
            if (texture != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    this.iconRectangle,
                    this.TextureRectangle == Rectangle.Empty ? texture.Bounds : this.TextureRectangle,
                    Color.White,
                    0f,
                    default);
            }

            spriteBatch.DrawStringOnCtrl(
                    this,
                    this.Text,
                    this.Font,
                    this.textRectangle,
                    this.TextColor,
                    false,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Middle);
        }

        private void UpdateLayout()
        {
            this.Width = Math.Max((int)this.Font.MeasureString(this.Text).Width + 4 + (this.Icon == null ? 0 : this.Height + 5), this.Height);

            this.iconRectangle = this.Icon == null ? Rectangle.Empty : new Rectangle(2, 2, this.LocalBounds.Height - 4, this.LocalBounds.Height - 4);
            this.textRectangle = new Rectangle(this.iconRectangle.Right + (this.Icon == null ? 0 : 5), 2, this.LocalBounds.Width - (this.iconRectangle.Right + (this.Icon == null ? 0 : 5) + 2), this.LocalBounds.Height - 4);
        }
    }
}
