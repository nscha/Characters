namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class ImageGrayScaled : Control
    {
        private static Color defaultColorHovered = new Color(255, 255, 255, 255);
        private static Color defaultColorActive = new Color(200, 200, 200, 200);
        private static Color defaultColorInActive = new Color(175, 175, 175, 255);
        private Rectangle textureRectangle = Rectangle.Empty;
        private Texture2D grayScaleTexture;
        private AsyncTexture2D texture;
        private bool active = false;

        public bool UseGrayScale { get; set; } = true;

        public AsyncTexture2D Texture
        {
            get => this.texture;
            set
            {
                this.texture = value;
                this.texture.TextureSwapped += this.Texture_TextureSwapped;
                if (value != null)
                {
                    this.grayScaleTexture = this.ToGrayScaledPalettable(value.Texture);
                }
            }
        }

        public Rectangle SizeRectangle { get; set; }

        public Rectangle TextureRectangle
        {
            get => this.textureRectangle;
            set => this.textureRectangle = value;
        }

        public bool Active
        {
            get => this.active;
            set => this.active = value;
        }

        public Color ColorHovered { get; set; } = new Color(255, 255, 255, 255);

        public Color ColorActive { get; set; } = new Color(200, 200, 200, 200);

        public Color ColorInActive { get; set; } = new Color(175, 175, 175, 255);

        public float Alpha { get; set; } = 0.25f;

        public void ResetColors()
        {
            this.ColorHovered = defaultColorHovered;
            this.ColorActive = defaultColorActive;
            this.ColorInActive = defaultColorInActive;
        }

        public Texture2D ToGrayScaledPalettable(Texture2D original)
        {
            // make an empty bitmap the same size as original
            Color[] colors = new Color[original.Width * original.Height];
            original.GetData<Color>(colors);
            Color[] destColors = new Color[original.Width * original.Height];
            Texture2D newTexture;

            using (var device = GameService.Graphics.LendGraphicsDeviceContext())
            {
                newTexture = new Texture2D(device.GraphicsDevice, original.Width, original.Height);
            }

            for (int i = 0; i < original.Width; i++)
            {
                for (int j = 0; j < original.Height; j++)
                {
                    // get the pixel from the original image
                    int index = i + (j * original.Width);
                    Color originalColor = colors[index];

                    // create the grayscale version of the pixel
                    float maxval = .3f + .59f + .11f + .79f;
                    float grayScale = (originalColor.R / 255f * .3f) + (originalColor.G / 255f * .59f) + (originalColor.B / 255f * .11f) + (originalColor.A / 255f * .79f);
                    grayScale = grayScale / maxval;

                    destColors[index] = new Color(grayScale, grayScale, grayScale, originalColor.A);
                }
            }

            newTexture.SetData<Color>(destColors);
            return newTexture;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.texture != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this.UseGrayScale && !this.active && !this.MouseOver ? this.grayScaleTexture : this.texture,
                    this.SizeRectangle != Rectangle.Empty ? this.SizeRectangle : bounds,
                    this.textureRectangle == Rectangle.Empty ? this.texture.Bounds : this.textureRectangle,
                    this.MouseOver ? this.ColorHovered : this.active ? this.ColorActive : this.ColorInActive * (this.UseGrayScale ? 0.5f : this.Alpha),
                    0f,
                    default);
            }
        }

        private void Texture_TextureSwapped(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            this.grayScaleTexture = this.ToGrayScaledPalettable(this.texture);
            this.texture.TextureSwapped -= this.Texture_TextureSwapped;
        }
    }
}