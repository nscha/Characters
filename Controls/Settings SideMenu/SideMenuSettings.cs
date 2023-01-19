using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class SettingsSideMenu : TabbedPanel
    {
        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        public SettingsSideMenu()
        {
            int width = 0;
            switch (GameService.Overlay.UserLocale.Value)
            {
                case Gw2Sharp.WebApi.Locale.French:
                    width = 300;
                    break;
                case Gw2Sharp.WebApi.Locale.Spanish:
                    width = 235;
                    break;
                default:
                    width = 200;
                    break;
            }

            Size = new Point(width, 100);
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);

            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = false;
            HeightSizingMode = SizingMode.AutoSize;

            AddTab(new OrderSettings(width - 10)
            {
                Name = string.Format(Strings.common.ItemSettings, Strings.common.Order),
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156909),
                Active = true,
            });

            AddTab(new FilterSettings(width - 10)
            {
                Name = string.Format(Strings.common.ItemSettings, Strings.common.Filter),
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(784371),
                Active = false,
            });

            AddTab(new DisplaySettings(width - 10)
            {
                Name = string.Format(Strings.common.ItemSettings, Strings.common.Display),
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(528726),
                Active = false,
            });

            AddTab(new SettingsAndShortcuts()
            {
                Name = Strings.common.GeneralAndWindows,
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(440021),
                Active = false,
            });
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - opacityTick > 50)
            {
                opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && DateTime.Now.Subtract(lastMouseOver).TotalMilliseconds >= 2500)
                {
                    Opacity = Opacity - 0.05F;
                    if (Opacity <= 0F)
                    {
                        Hide();
                    }
                }
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            Color color = Color.Black;

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

        protected override void DisposeControl() => base.DisposeControl();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            lastMouseOver = DateTime.Now;
            Opacity = 1f;
            Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            lastMouseOver = DateTime.Now;
            Opacity = 1f;
        }
    }
}
