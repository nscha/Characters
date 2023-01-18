namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;

    public class Filter_SideMenu : TabbedPanel
    {
        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;
        private readonly Filters_Panel filters;

        public List<Tag> Tags
        {
            get => this._tagsPanel.Tags;
        }

        private readonly Tags_Panel _tagsPanel;

        public Filter_SideMenu()
        {
            this.Size = new Point(200, 100);
            this.Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); // 155985
            this.ColorBackground = Color.Black * 0.75f;
            this.BackgroundTint = new Color(200, 200, 200, 255);

            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = 999;
            this.Visible = false;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.filters = new Filters_Panel();
            this.AddTab(this.filters);

            this._tagsPanel = new Tags_Panel() { Visible = false };
            this.AddTab(this._tagsPanel);
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

                if (!this.MouseOver && DateTime.Now.Subtract(this.lastMouseOver).TotalMilliseconds >= 2500 && !Characters.ModuleInstance.MainWindow.FilterBox.Focused)
                {
                    this.Opacity = this.Opacity - (float)0.05;
                    if (this.Opacity <= (float)0)
                    {
                        this.Hide();
                    }
                }
            }
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

        public void ResetToggles()
        {
            this.filters.ResetToggles();
        }
    }
}
