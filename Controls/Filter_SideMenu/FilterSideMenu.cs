using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public class FilterSideMenu : TabbedPanel
    {
        private readonly FiltersPanel filters;
        private readonly TagsPanel tagsPanel;

        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        public FilterSideMenu()
        {
            Size = new Point(200, 100);
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); // 155985
            ColorBackground = Color.Black * 0.75f;
            BackgroundTint = new Color(200, 200, 200, 255);

            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = false;
            HeightSizingMode = SizingMode.AutoSize;

            filters = new FiltersPanel();
            AddTab(filters);

            tagsPanel = new TagsPanel() { Visible = false };
            AddTab(tagsPanel);
        }

        public List<Tag> Tags
        {
            get => tagsPanel.Tags;
        }

        public void ResetToggles() => filters.ResetToggles();

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - opacityTick > 50)
            {
                opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && DateTime.Now.Subtract(lastMouseOver).TotalMilliseconds >= 2500 && !Characters.ModuleInstance.MainWindow.FilterBox.Focused)
                {
                    Opacity = Opacity - 0.05F;
                    if (Opacity <= 0F)
                    {
                        Hide();
                    }
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            lastMouseOver = DateTime.Now;
            Opacity = 1f;
            Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            Point newSize = new(Width / 2, 25);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            lastMouseOver = DateTime.Now;
            Opacity = 1f;
        }
    }
}
