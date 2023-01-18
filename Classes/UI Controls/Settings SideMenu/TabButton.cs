namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class TabButton : Control
    {
        public bool Active;
        public bool UseGrayScale;
        public AsyncTexture2D Icon;
        public AsyncTexture2D IconGrayScale;
        public Rectangle TextureRectangle = Rectangle.Empty;
        public BitmapFont Font;

        public TabButton()
        {
            this.Height = 25;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var bgColor = this.Active ? Color.Transparent : Color.Black;
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Rectangle.Empty, bgColor * (this.MouseOver ? 0.35f : 0.55f));

            if (this.Icon != null)
            {
                var tRect = this.TextureRectangle != Rectangle.Empty ? this.TextureRectangle : this.Icon.Bounds;
                var size = bounds.Height - 4;

                if (this.UseGrayScale && this.IconGrayScale == null)
                {
                    this.IconGrayScale = this.Icon.Texture.ToGrayScaledPalettable();
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    this.Active ? this.Icon : this.UseGrayScale ? this.IconGrayScale : this.Icon,
                    new Rectangle(2 + ((bounds.Width - size) / 2), 3, size, size),
                    tRect,
                    this.Active ? Color.White : new Color(75, 75, 75),
                    0f,
                    default);
            }

            var color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);


            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            // Active = true;
        }
    }
}
