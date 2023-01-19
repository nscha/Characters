namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;

    public class FilterSideMenu : TabbedPanel
    {
        private readonly FiltersPanel filters;
        private readonly TagsPanel tagsPanel;

        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        public FilterSideMenu()
        {
            this.Size = new Point(200, 100);
            this.Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); // 155985
            this.ColorBackground = Color.Black * 0.75f;
            this.BackgroundTint = new Color(200, 200, 200, 255);

            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = 999;
            this.Visible = false;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.filters = new FiltersPanel();
            this.AddTab(this.filters);

            this.tagsPanel = new TagsPanel() { Visible = false };
            this.AddTab(this.tagsPanel);
        }

        public List<Tag> Tags
        {
            get => this.tagsPanel.Tags;
        }

        public void ResetToggles()
        {
            this.filters.ResetToggles();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - this.opacityTick > 50)
            {
                this.opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!this.MouseOver && DateTime.Now.Subtract(this.lastMouseOver).TotalMilliseconds >= 2500 && !Characters.ModuleInstance.MainWindow.FilterBox.Focused)
                {
                    this.Opacity = this.Opacity - 0.05F;
                    if (this.Opacity <= 0F)
                    {
                        this.Hide();
                    }
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.lastMouseOver = DateTime.Now;
            this.Opacity = 1f;
            this.Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            var newSize = new Point(this.Width / 2, 25);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            this.lastMouseOver = DateTime.Now;
            this.Opacity = 1f;
        }
    }
}
