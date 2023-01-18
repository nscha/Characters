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

        private AsyncTexture2D _texture;

        public AsyncTexture2D Texture
        {
            get => this._texture;
            set
            {
                this._texture = value;
            }
        }

        public AsyncTexture2D HoveredTexture;
        public AsyncTexture2D ClickedTexture;

        private bool _clicked;

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

        private Rectangle _textureRectangle = Rectangle.Empty;
        public Rectangle SizeRectangle;

        public Rectangle TextureRectangle
        {
            get => this._textureRectangle;
            set => this._textureRectangle = value;
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
            if (this._texture != null)
            {
                var texture = this._clicked && this.ClickedTexture != null ? this.ClickedTexture : this.MouseOver && this.HoveredTexture != null ? this.HoveredTexture : this.Texture;
                this._clicked = this._clicked && this.MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    this.SizeRectangle != Rectangle.Empty ? this.SizeRectangle : bounds,
                    this._textureRectangle == Rectangle.Empty ? texture.Bounds : this._textureRectangle,
                    this.MouseOver ? this.ColorHovered : this.MouseOver && this._clicked ? this.ColorClicked : this.ColorDefault,
                    0f,
                    default);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            this._clicked = true;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            this._clicked = false;
        }
    }
}

