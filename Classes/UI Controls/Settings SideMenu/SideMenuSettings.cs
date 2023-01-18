using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class SettingsSideMenu : TabbedPanel
    {
        double OpacityTick = 0;
        DateTime LastMouseOver = DateTime.Now;

        public SettingsSideMenu()
        {
            Size = new Point(200, 100);
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);

            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = false;
            HeightSizingMode = SizingMode.AutoSize;

            AddTab(new OrderSettings()
            {
                Name = "Order Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156909),
                Active = true,
            });

            AddTab(new FilterSettings()
            {
                Name = "Filter Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(784371),
                Active = false,
            });

            AddTab(new DisplaySettings()
            {
                Name = "Display Settings",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(528726),
                Active = false,
            });

            AddTab(new SettingsAndShortcuts()
            {
                Name = "General Settings & Windows",
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(440021),
                Active = false,
            });
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LastMouseOver = DateTime.Now;
            Opacity = 1f;
            Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - OpacityTick > 50)
            {
                OpacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && DateTime.Now.Subtract(LastMouseOver).TotalMilliseconds >= 2500)
                {
                    Opacity = Opacity - (float)0.05;
                    if (Opacity <= (float)0) Hide();
                }
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            LastMouseOver = DateTime.Now;
            Opacity = 1f;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);
            
            var color = Color.Black;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

        }

        protected override void DisposeControl()
        {
            base.DisposeControl();


        }
    }
}
