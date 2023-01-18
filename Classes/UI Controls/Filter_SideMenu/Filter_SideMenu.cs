using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class Filter_SideMenu : TabbedPanel
    {
        double OpacityTick = 0;
        DateTime LastMouseOver = DateTime.Now;

        Filters_Panel Filters;
        public List<Tag> Tags {
            get => _tagsPanel.Tags;
        }
        Tags_Panel _tagsPanel;

        public Filter_SideMenu()
        {
            Size = new Point(200, 100);
            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); //155985
            ColorBackground = Color.Black * 0.75f;
            BackgroundTint = new Color(200,200,200,255);

            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = false;
            HeightSizingMode = SizingMode.AutoSize;

            Filters = new Filters_Panel();
            AddTab(Filters);

            _tagsPanel = new Tags_Panel() { Visible = false };
            AddTab(_tagsPanel);
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

                if (!MouseOver && DateTime.Now.Subtract(LastMouseOver).TotalMilliseconds >= 2500 && !Characters.ModuleInstance.MainWindow.FilterBox.Focused)
                {
                    Opacity = Opacity - (float)0.05;
                    if (Opacity <= (float)0) Hide();
                }
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            var newSize = new Point(Width / 2, 25);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            LastMouseOver = DateTime.Now;
            Opacity = 1f;
        }

        public void ResetToggles()
        {
            Filters.ResetToggles();
        }
    }
}
