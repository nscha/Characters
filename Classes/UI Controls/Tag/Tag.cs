namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class Tag : FlowPanel
    {
        public BitmapFont Font
        {
            get { return this.text.Font; }
            set { this.text.Font = value; }
        }

        private Color disabledColor = new Color(156, 156, 156);
        public bool Active = true;
        public bool CanInteract = true;
        private Texture2D disabledBackground;
        private AsyncTexture2D background;

        public AsyncTexture2D Background
        {
            get => this.background;
            set
            {
                this.background = value;
                if (value != null)
                {
                    this.CreateDisabledBackground(null, null);
                    this.background.TextureSwapped += this.CreateDisabledBackground;
                }
            }
        }

        private void CreateDisabledBackground(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            this.disabledBackground = this.background.Texture.ToGrayScaledPalettable();
            this.background.TextureSwapped -= this.CreateDisabledBackground;
        }

        private readonly Label text;
        private readonly ImageButton delete;
        private readonly ImageButton dummy;

        public bool ShowDelete
        {
            get => this.delete.Visible;
            set
            {
                if (this.delete != null)
                {
                    this.delete.Visible = value;
                    this.dummy.Visible = !value;
                }
            }
        }

        public string Text
        {
            get => this.text != null ? this.text.Text : null;
            set
            {
                if (this.text != null)
                {
                    this.text.Text = value;
                }
            }
        }

        public event EventHandler Deleted;

        public Tag()
        {
            this.Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(1620622);
            this.WidthSizingMode = SizingMode.AutoSize;
            this.FlowDirection = ControlFlowDirection.SingleLeftToRight;
            this.OuterControlPadding = new Vector2(3, 3);
            this.ControlPadding = new Vector2(2, 0);
            this.AutoSizePadding = new Point(5, 3);
            this.Height = 26;

            this.delete = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156012),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156011),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                Size = new Point(20, 20),
                BasicTooltipText = "Remove Tag",
            };
            this.delete.Click += this.Delete_Click;

            this.dummy = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025),
                TextureRectangle = new Rectangle(44, 48, 43, 46),
                Size = new Point(20, 20),
                Visible = false,
            };

            this.text = new Label()
            {
                Parent = this,
                AutoSizeWidth = true,
                Height = this.Height - (int)this.OuterControlPadding.Y,
            };
        }

        private void Delete_Click(object sender, MouseEventArgs e)
        {
            this.Deleted?.Invoke(this, EventArgs.Empty);
            this.Dispose();
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.background != null)
            {
                var texture = this.Active ? this.background : this.disabledBackground != null ? this.disabledBackground : this.background;

                spriteBatch.DrawOnCtrl(this, texture, bounds, bounds, this.Active ? Color.White * 0.98f : this.disabledColor * 0.8f);
            }

            var color = Color.Black;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (this.CanInteract)
            {
                base.OnClick(e);
                this.Active = !this.Active;
            }
        }
    }
}
