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
            get { return this._text.Font; }
            set { this._text.Font = value; }
        }

        private Color _disabledColor = new Color(156, 156, 156);
        public bool Active = true;
        public bool CanInteract = true;
        private Texture2D _disabledBackground;
        private AsyncTexture2D _background;

        public AsyncTexture2D Background
        {
            get => this._background;
            set
            {
                this._background = value;
                if (value != null)
                {
                    this.CreateDisabledBackground(null, null);
                    this._background.TextureSwapped += this.CreateDisabledBackground;
                }
            }
        }

        private void CreateDisabledBackground(object sender, ValueChangedEventArgs<Texture2D> e)
        {
            this._disabledBackground = this._background.Texture.ToGrayScaledPalettable();
            this._background.TextureSwapped -= this.CreateDisabledBackground;
        }

        private readonly Label _text;
        private readonly ImageButton _delete;
        private readonly ImageButton _dummy;

        public bool ShowDelete
        {
            get => this._delete.Visible;
            set
            {
                if (this._delete != null)
                {
                    this._delete.Visible = value;
                    this._dummy.Visible = !value;
                };
            }
        }

        public string Text
        {
            get => this._text != null ? this._text.Text : null;
            set
            {
                if (this._text != null)
                {
                    this._text.Text = value;
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

            this._delete = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156012),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156011),
                TextureRectangle = new Rectangle(4, 4, 24, 24),
                Size = new Point(20, 20),
                BasicTooltipText = "Remove Tag",
            };
            this._delete.Click += this.Delete_Click;

            this._dummy = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(156025),
                TextureRectangle = new Rectangle(44, 48, 43, 46),
                Size = new Point(20, 20),
                Visible = false,
            };

            this._text = new Label()
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
            if (this._background != null)
            {
                var texture = this.Active ? this._background : this._disabledBackground != null ? this._disabledBackground : this._background;

                spriteBatch.DrawOnCtrl(this, texture, bounds, bounds, this.Active ? Color.White * 0.98f : this._disabledColor * 0.8f);
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
