using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class ImageButton : Control
    {

        private AsyncTexture2D _texture;
        public AsyncTexture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
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
                    new Color(255, 191, 255)
            };

        private Rectangle _textureRectangle = Rectangle.Empty;
        public Rectangle SizeRectangle;
        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set => _textureRectangle = value;
        }

        public Color ColorHovered = new Color(255, 255, 255, 255);
        public Color ColorClicked = new Color(0, 0, 255, 255);
        public Color ColorDefault = new Color(255, 255, 255, 255);

        static public Color DefaultColorHovered = new Color(255, 255, 255, 255);
        static public Color DefaultColorClicked = new Color(255, 255, 255, 255);
        static public Color DefaultColor = new Color(255, 255, 255, 255);

        public void ResetColors()
        {
            ColorHovered = DefaultColorHovered;
            ColorClicked = DefaultColorClicked;
            ColorDefault = DefaultColor;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_texture != null)
            {
                var texture = _clicked && ClickedTexture != null ? ClickedTexture : MouseOver && HoveredTexture != null ? HoveredTexture : Texture;
                _clicked = _clicked && MouseOver;

                spriteBatch.DrawOnCtrl(this,
                                        texture,
                                        SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                                        _textureRectangle == Rectangle.Empty ? texture.Bounds : _textureRectangle,
                                        MouseOver ? ColorHovered : MouseOver && _clicked ? ColorClicked : ColorDefault,
                                        0f,
                                        default);

            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            _clicked = true;
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            _clicked = false;
        }
    }
}

