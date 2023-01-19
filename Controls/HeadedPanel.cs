﻿namespace Kenedia.Modules.Characters.Controls
{
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using static Kenedia.Modules.Characters.Services.TextureManager;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    internal class HeadedPanel : Panel
    {
        private readonly AsyncTexture2D headerUnderline;
        private readonly bool initialized = false;
        private readonly Label headerLabel = new ()
        {
            Font = GameService.Content.DefaultFont16,
            Location = new Point(5, 3),
            AutoSizeWidth = true,
            AutoSizeHeight = true,
            Padding = new Thickness(4f),
        };

        private readonly FlowPanel contentPanel = new ()
        {
            WidthSizingMode = SizingMode.Fill,
            HeightSizingMode = SizingMode.AutoSize,
            OuterControlPadding = new Vector2(0, 5),
            ControlPadding = new Vector2(4, 4),
            AutoSizePadding = new Point(0, 5),
            Location = new Point(0, 25),
        };

        private string header = string.Empty;

        public HeadedPanel()
        {
            this.WidthSizingMode = SizingMode.Fill;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.headerLabel.Parent = this;
            this.contentPanel.Parent = this;

            this.headerUnderline = Characters.ModuleInstance.TextureManager.GetControlTexture(Controls.Separator);

            this.initialized = true;
        }

        public string Header
        {
            get => this.header;
            set
            {
                this.header = value;
                this.headerLabel.Text = value;
            }
        }

        public bool TintContent { get; set; } = false;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            var color = Color.Black;
            var b = this.contentPanel.LocalBounds;
            var pad = 5;

            if (this.TintContent)
            {
                // Bottom
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 2, b.Width - pad, 2), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 1, b.Width - pad, 1), Rectangle.Empty, color * 0.6f);

                // Left
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top + 4, 2, b.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top + 4, 1, b.Height), Rectangle.Empty, color * 0.6f);

                // Right
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 2 - pad, b.Top + 2, 2, b.Height), Rectangle.Empty, color * 0.5f);
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 1 - pad, b.Top + 2, 1, b.Height), Rectangle.Empty, color * 0.6f);

                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width - pad, b.Height), Rectangle.Empty, color * 0.5f);
            }

            b = this.headerLabel.LocalBounds;

            spriteBatch.DrawOnCtrl(
                this,
                this.headerUnderline,
                new Rectangle(bounds.Left, b.Bottom, bounds.Width, 5),
                this.headerUnderline.Bounds,
                Color.White,
                0f,
                default);
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
            base.OnChildAdded(e);

            if (this.initialized)
            {
                e.ChangedChild.Parent = this.contentPanel;
            }
        }
    }
}