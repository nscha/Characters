namespace Kenedia.Modules.Characters.Classes.UI_Controls
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
        private AsyncTexture2D texture;

        public AsyncTexture2D Texture
        {
            get => this.texture;
            set
            {
                this.texture = value;
            }
        }

        public AsyncTexture2D HoveredTexture;
        public AsyncTexture2D ClickedTexture;

        private bool clicked;

        public static Color[] palColorArray = new Color[]
            {
                    new Color(191, 191, 191),
                    new Color(000, 000, 000),
                    new Color(255, 255, 255),
                    new Color(255, 000, 000),
                    new Color(191, 000, 000),
                    new Color(255, 191, 191),
                    new Color(255, 255, 000),
                    new Color(191, 191, 000),
                    new Color(255, 255, 191),
                    new Color(000, 255, 000),
                    new Color(000, 191, 000),
                    new Color(191, 255, 191),
                    new Color(000, 255, 255),
                    new Color(000, 191, 191),
                    new Color(191, 255, 255),
                    new Color(000, 000, 255),
                    new Color(000, 000, 191),
                    new Color(191, 191, 255),
                    new Color(255, 000, 255),
                    new Color(191, 000, 191),
                    new Color(255, 191, 255),
            };

        private Rectangle textureRectangle = Rectangle.Empty;
        public Rectangle SizeRectangle;

        public Rectangle TextureRectangle
        {
            get => this.textureRectangle;
            set => this.textureRectangle = value;
        }

        public Color ColorHovered = new Color(255, 255, 255, 255);
        public Color ColorClicked = new Color(0, 0, 255, 255);
        public Color ColorDefault = new Color(255, 255, 255, 255);

        static public Color DefaultColorHovered = new Color(255, 255, 255, 255);
        static public Color DefaultColorClicked = new Color(255, 255, 255, 255);
        static public Color DefaultColor = new Color(255, 255, 255, 255);

        public void ResetColors()
        {
            this.ColorHovered = DefaultColorHovered;
            this.ColorClicked = DefaultColorClicked;
            this.ColorDefault = DefaultColor;
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

