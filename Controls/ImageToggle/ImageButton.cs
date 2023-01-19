using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class ImageButton : Control
    {
        private static Color defaultColorHovered = new(255, 255, 255, 255);
        private static Color defaultColorClicked = new(255, 255, 255, 255);
        private static Color defaultColor = new(255, 255, 255, 255);

        private AsyncTexture2D texture;
        private Rectangle textureRectangle = Rectangle.Empty;
        private bool clicked;

        public AsyncTexture2D Texture
        {
            get => texture;
            set
            {
                texture = value;
            }
        }

        public AsyncTexture2D HoveredTexture { get; set; }

        public AsyncTexture2D ClickedTexture { get; set; }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle
        {
            get => textureRectangle;
            set => textureRectangle = value;
        }

        public Color ColorHovered { get; set; } = new(255, 255, 255, 255);

        public Color ColorClicked { get; set; } = new(0, 0, 255, 255);

        public Color ColorDefault { get; set; } = new(255, 255, 255, 255);

        public void ResetColors()
        {
            ColorHovered = defaultColorHovered;
            ColorClicked = defaultColorClicked;
            ColorDefault = defaultColor;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (texture != null)
            {
                AsyncTexture2D texture = clicked && ClickedTexture != null ? ClickedTexture : MouseOver && HoveredTexture != null ? HoveredTexture : Texture;
                clicked = clicked && MouseOver;

                spriteBatch.DrawOnCtrl(
                    this,
                    texture,
                    SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                    textureRectangle == Rectangle.Empty ? texture.Bounds : textureRectangle,
                    MouseOver ? ColorHovered : MouseOver && clicked ? ColorClicked : ColorDefault,
                    0f,
                    default);
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            clicked = true;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            clicked = false;
        }
    }
}