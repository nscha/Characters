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
    public class ImageGrayScaled : Control
    {
        public bool UseGrayScale = true;

        private Texture2D _grayScaleTexture;
        private AsyncTexture2D _texture;
        public AsyncTexture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                _texture.TextureSwapped += _texture_TextureSwapped;
                if (value != null) _grayScaleTexture = ToGrayScaledPalettable(value.Texture);
            }
        }

        private void _texture_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            _grayScaleTexture = ToGrayScaledPalettable(_texture);
            _texture.TextureSwapped -= _texture_TextureSwapped;
        }

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
        public Texture2D ToGrayScaledPalettable(Texture2D original)
        {
            //make an empty bitmap the same size as original
            Color[] colors = new Color[original.Width * original.Height];
            original.GetData<Color>(colors);
            Color[] destColors = new Color[original.Width * original.Height];
            Texture2D newTexture;

            using (var device = GraphicsService.Graphics.LendGraphicsDeviceContext())
            {
                newTexture = new Texture2D(device.GraphicsDevice, original.Width, original.Height);
            }

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    //get the pixel from the original image
                    int index = i + j * original.Width;
                    Color originalColor = colors[index];

                    //create the grayscale version of the pixel
                    float maxval = .3f + .59f + .11f + .79f;
                    float grayScale = (((originalColor.R / 255f) * .3f) + ((originalColor.G / 255f) * .59f) + ((originalColor.B / 255f) * .11f) + ((originalColor.A / 255f) * .79f));
                    grayScale = grayScale / maxval;

                    destColors[index] = new Color(grayScale, grayScale, grayScale, originalColor.A);
                }
            }
            newTexture.SetData<Color>(destColors);
            return newTexture;
        }

        private Rectangle _textureRectangle = Rectangle.Empty;
        public Rectangle SizeRectangle;
        public Rectangle TextureRectangle
        {
            get => _textureRectangle;
            set => _textureRectangle = value;
        }

        private bool _active = false;
        public bool Active
        {
            get => _active;
            set => _active = value;
        }
        public Color ColorHovered = new Color(255, 255, 255, 255);
        public Color ColorActive = new Color(200, 200, 200, 200);
        public Color ColorInActive = new Color(175, 175, 175, 255);
        public float Alpha = 0.25f;
        static public Color DefaultColorHovered = new Color(255, 255, 255, 255);
        static public Color DefaultColorActive = new Color(200, 200, 200, 200);
        static public Color DefaultColorInActive = new Color(175, 175, 175, 255);
        
        public void ResetColors()
        {
            ColorHovered = DefaultColorHovered;
            ColorActive = DefaultColorActive;
            ColorInActive = DefaultColorInActive;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (_texture != null)
            {
                spriteBatch.DrawOnCtrl(this,
                                        UseGrayScale && (!_active && !MouseOver) ? _grayScaleTexture : _texture,
                                        SizeRectangle != Rectangle.Empty ? SizeRectangle : bounds,
                                        _textureRectangle == Rectangle.Empty ? _texture.Bounds : _textureRectangle,
                                        MouseOver ? ColorHovered : _active ? ColorActive : ColorInActive * (UseGrayScale ? 0.5f : Alpha),
                                        0f,
                                        default);

            }
        }
    }
}

