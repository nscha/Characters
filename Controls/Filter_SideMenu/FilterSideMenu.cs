using Blish_HUD;
using Blish_HUD.Content;
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
        private readonly FiltersPanel _filters;
        private readonly TagsPanel _tagsPanel;

        private double _opacityTick = 0;
        private DateTime _lastMouseOver = DateTime.Now;

        public FilterSideMenu()
        {
            Size = new Point(200, 100);
            Background = AsyncTexture2D.FromAssetId(156003); // 155985
            ColorBackground = Color.Black * 0.75f;
            BackgroundTint = new Color(200, 200, 200, 255);

            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = false;
            HeightSizingMode = SizingMode.AutoSize;

            _filters = new FiltersPanel();
            AddTab(_filters);

            _tagsPanel = new TagsPanel() { Visible = false };
            AddTab(_tagsPanel);
        }

        public List<Tag> Tags => _tagsPanel.Tags;

        public void ResetToggles()
        {
            _filters.ResetToggles();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - _opacityTick > 50)
            {
                _opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && Characters.ModuleInstance.Settings.FadeOut.Value && DateTime.Now.Subtract(_lastMouseOver).TotalMilliseconds >= 2500 && !Characters.ModuleInstance.MainWindow.FilterBox.Focused)
                {
                    Opacity -= 0.05F;
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
            _lastMouseOver = DateTime.Now;
            Opacity = 1f;
            Location = new Point(Characters.ModuleInstance.MainWindow.AbsoluteBounds.Right, Characters.ModuleInstance.MainWindow.AbsoluteBounds.Top + 45);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            _lastMouseOver = DateTime.Now;
            Opacity = 1f;
        }
    }
}
