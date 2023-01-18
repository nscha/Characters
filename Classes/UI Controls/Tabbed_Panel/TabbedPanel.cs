using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class TabbedPanel : Panel
    {
        private PanelTab _activeTab;
        public PanelTab ActiveTab
        {
            get => _activeTab;
            set => SwitchTab(value);
        }

        protected FlowPanel _tabsButtonPanel = new FlowPanel()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            ControlPadding = new Vector2(1, 0),
            Height = 25,
        };

        public AsyncTexture2D Background;
        public Color BackgroundTint;
        public Color ColorBackground;
        public Rectangle TextureRectangle = Rectangle.Empty;
        public Point TextureOffset = Point.Zero;

        private List<PanelTab> _tabs = new List<PanelTab>();
        public List<PanelTab> Tabs
        {
            get => _tabs;
        }

        public TabbedPanel()
        {
            _tabsButtonPanel.Parent = this;
            _tabsButtonPanel.Resized += OnTabButtonPanelResized;

            HeightSizingMode = SizingMode.AutoSize;
            //Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);
            Parent = GameService.Graphics.SpriteScreen;
            ZIndex = 999;
            Visible = true;
            BackgroundTint = Color.Honeydew * 0.95f;
        }

        private void OnTabButtonPanelResized(object sender, ResizedEventArgs e)
        {
            RecalculateLayout();
        }

        event EventHandler TabAdded;
        event EventHandler TabRemoved;
        public void AddTab(PanelTab tab)
        {
            tab.Parent = this;
            tab.Disposed += OnTabDisposed;
            tab.TabButton.Parent = _tabsButtonPanel;
            tab.TabButton.Click += (sender, model) => TabButton_Click(sender, model, tab);
            _tabs.Add(tab);
            this.TabAdded?.Invoke(this, EventArgs.Empty);
            ActiveTab ??= tab;
            RecalculateLayout();
        }

        private void TabButton_Click(object sender, MouseEventArgs e, PanelTab t)
        {
            SwitchTab(t);
        }

        private void OnTabDisposed(object sender, EventArgs e)
        {
            RemoveTab((PanelTab)sender);
        }

        public void RemoveTab(PanelTab tab)
        {
            tab.Disposed -= OnTabDisposed;
            tab.TabButton.Click -= (sender, model) => TabButton_Click(sender, model, tab);
            tab.Parent = null;
            tab.TabButton.Parent = null;

            _tabs.Remove(tab);
            this.TabRemoved?.Invoke(this, EventArgs.Empty);
            RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            var button_amount = Math.Max(1, _tabsButtonPanel.Children.Count);
            var width = (_tabsButtonPanel.Width - ((button_amount -1) * (int)_tabsButtonPanel.ControlPadding.X)) / button_amount;
            foreach (Control c in _tabsButtonPanel.Children)
            {
                c.Width = width;
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected void SwitchTab(PanelTab tab = null)
        {
            foreach(PanelTab t in Tabs)
            {
                if(t != tab) t.Active = false;
            }
            if(tab != null) tab.Active = true;

            _activeTab = tab;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            //if(ColorBackground != null) spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, _tabsButtonPanel.Bottom, bounds.Width, bounds.Height - _tabsButtonPanel.Bottom), Rectangle.Empty, ColorBackground);
            if(ColorBackground != null) spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height), Rectangle.Empty, ColorBackground);

            if (Background != null)
            {
                var textRect = TextureRectangle != Rectangle.Empty ? TextureRectangle : new Rectangle(Point.Zero, _size);

                spriteBatch.DrawOnCtrl(this,
                        Background,
                        bounds,
                        new Rectangle(TextureOffset.X + TextureRectangle.X, TextureOffset.Y + TextureRectangle.Y, textRect.Width, textRect.Height),
                        BackgroundTint,
                        0f,
                        default);
            }


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

            _tabs.DisposeAll();
        }
    }
}
