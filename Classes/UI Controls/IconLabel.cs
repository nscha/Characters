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
        private string _Text;

        public string Text
        {
            get => this._Text;
            set
            {
                this._Text = value;
                this.UpdateLayout();
            }
        }

        public Color TextColor = Color.White;
        private AsyncTexture2D _Icon;

        public AsyncTexture2D Icon
        {
            get => this._Icon;
            set
            {
                this._Icon = value;
                if (value != null)
                {
                    this.UpdateLayout();
                }
            }
        }

        private BitmapFont _Font = GameService.Content.DefaultFont14;

        public BitmapFont Font
        {
            get => this._Font;
            set
            {
                this._Font = value;
                if (value != null)
                {
                    this.UpdateLayout();
                }
            }
        }

        public bool AutoSizeWidth;
        public bool AutoSizeHeight;

        public Rectangle TextureRectangle = Rectangle.Empty;
        private Rectangle iconRectangle = Rectangle.Empty;
        private Rectangle textRectangle = Rectangle.Empty;

        private void UpdateLayout()
        {
            this.Width = Math.Max((int)this.Font.MeasureString(this.Text).Width + 4 + (this.Icon == null ? 0 : this.Height + 5), this.Height);

            this.iconRectangle = this.Icon == null ? Rectangle.Empty : new Rectangle(2, 2, this.LocalBounds.Height - 4, this.LocalBounds.Height - 4);
            this.textRectangle = new Rectangle(this.iconRectangle.Right + (this.Icon == null ? 0 : 5), 2, this.LocalBounds.Width - (this.iconRectangle.Right + (this.Icon == null ? 0 : 5) + 2), this.LocalBounds.Height - 4);
        }

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
    }
}
