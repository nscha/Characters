namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class ImageButton : Control
    {
        private static Color defaultColorHovered = new (255, 255, 255, 255);
        private static Color defaultColorClicked = new (255, 255, 255, 255);
        private static Color defaultColor = new (255, 255, 255, 255);

        private AsyncTexture2D texture;
        private Rectangle textureRectangle = Rectangle.Empty;
        private bool clicked;

        public AsyncTexture2D Texture
        {
            get => this.texture;
            set
            {
                this.texture = value;
            }
        }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle
        {
            get => this.textureRectangle;
            set => this.textureRectangle = value;
        }

        public Color ColorHovered { get; set; } = new (255, 255, 255, 255);

        public Color ColorClicked { get; set; } = new (0, 0, 255, 255);

        public Color ColorDefault { get; set; } = new (255, 255, 255, 255);

        public void ResetColors()
        {
            this.ColorHovered = defaultColorHovered;
            this.ColorClicked = defaultColorClicked;
            this.ColorDefault = defaultColor;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.texture != null)
            {
                var texture = this.clicked && this.ClickedTexture != null ? this.ClickedTexture : this.MouseOver && this.HoveredTexture != null ? this.HoveredTexture : this.Texture;
                this.clicked = this.clicked && this.MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    this.SizeRectangle != Rectangle.Empty ? this.SizeRectangle : bounds,
                    this.textureRectangle == Rectangle.Empty ? texture.Bounds : this.textureRectangle,
                    this.MouseOver ? this.ColorHovered : this.MouseOver && this.clicked ? this.ColorClicked : this.ColorDefault,
                    0f,
                    default);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            this.clicked = true;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            this.clicked = false;
        }
    }
}