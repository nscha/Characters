namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class SettingsSideMenu : TabbedPanel
    {
        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        public SettingsSideMenu()
        {
            this.Size = new Point(200, 100);
            this.Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);

            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = 999;
            this.Visible = false;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.AddTab(new OrderSettings()
            {
                Name = "Order Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156909),
                Active = true,
            });

            this.AddTab(new FilterSettings()
            {
                Name = "Filter Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(784371),
                Active = false,
            });

            this.AddTab(new DisplaySettings()
            {
                Name = "Display Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(528726),
                Active = false,
            });

            this.AddTab(new SettingsAndShortcuts()
            {
                Name = "General Settings & Windows",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(440021),
                Active = false,
            });
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.lastMouseOver = DateTime.Now;
            this.Opacity = 1f;
            this.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - this.opacityTick > 50)
            {
                this.opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!this.MouseOver && DateTime.Now.Subtract(this.lastMouseOver).TotalMilliseconds >= 2500)
                {
                    this.Opacity = this.Opacity - (float)0.05;
                    if (this.Opacity <= (float)0)
                    {
                        this.Hide();
                    }
                }
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            this.lastMouseOver = DateTime.Now;
            this.Opacity = 1f;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

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

        protected override void DisposeControl()
        {
            base.DisposeControl();
        }
    }
}
