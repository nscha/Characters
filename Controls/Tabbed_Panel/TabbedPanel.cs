namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.Characters.Extensions;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class TabbedPanel : Panel
    {
        private readonly List<PanelTab> tabs = new ();
        private readonly FlowPanel tabsButtonPanel = new ()
        {
            FlowDirection = ControlFlowDirection.SingleLeftToRight,
            WidthSizingMode = SizingMode.Fill,
            ControlPadding = new Vector2(1, 0),
            Height = 25,
        };

        private PanelTab activeTab;

        public TabbedPanel()
        {
            this.tabsButtonPanel.Parent = this;
            this.tabsButtonPanel.Resized += this.OnTabButtonPanelResized;

            this.HeightSizingMode = SizingMode.AutoSize;

            // Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);
            this.Parent = GameService.Graphics.SpriteScreen;
            this.ZIndex = 999;
            this.Visible = true;
            this.BackgroundTint = Color.Honeydew * 0.95f;
        }

        private event EventHandler TabAdded;

        private event EventHandler TabRemoved;

        public List<PanelTab> Tabs
        {
            get => this.tabs;
        }

        public PanelTab ActiveTab
        {
            get => this.activeTab;
            set => this.SwitchTab(value);
        }

        public AsyncTexture2D Background { get; set; }

        public Color BackgroundTint { get; set; }

        public Color ColorBackground { get; set; }

        public Rectangle TextureRectangle { get; set; } = Rectangle.Empty;

        public Point TextureOffset { get; set; } = Point.Zero;

        public void AddTab(PanelTab tab)
        {
            tab.Parent = this;
            tab.Disposed += this.OnTabDisposed;
            tab.TabButton.Parent = this.tabsButtonPanel;
            tab.TabButton.Click += (sender, model) => this.TabButton_Click(sender, model, tab);
            this.tabs.Add(tab);
            this.TabAdded?.Invoke(this, EventArgs.Empty);
            this.ActiveTab ??= tab;
            this.RecalculateLayout();
        }

        public void RemoveTab(PanelTab tab)
        {
            tab.Disposed -= this.OnTabDisposed;
            tab.TabButton.Click -= (sender, model) => this.TabButton_Click(sender, model, tab);
            tab.Parent = null;
            tab.TabButton.Parent = null;

            this.tabs.Remove(tab);
            this.TabRemoved?.Invoke(this, EventArgs.Empty);
            this.RecalculateLayout();
        }

        public override void RecalculateLayout()
        {
            var button_amount = Math.Max(1, this.tabsButtonPanel.Children.Count);
            var width = (this.tabsButtonPanel.Width - ((button_amount - 1) * (int)this.tabsButtonPanel.ControlPadding.X)) / button_amount;
            foreach (Control c in this.tabsButtonPanel.Children)
            {
                c.Width = width;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            // if(ColorBackground != null) spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, _tabsButtonPanel.Bottom, bounds.Width, bounds.Height - _tabsButtonPanel.Bottom), Rectangle.Empty, ColorBackground);
            if (this.ColorBackground != null)
            {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height), Rectangle.Empty, this.ColorBackground);
            }

            if (this.Background != null)
            {
                var textRect = this.TextureRectangle != Rectangle.Empty ? this.TextureRectangle : new Rectangle(Point.Zero, this._size);

                spriteBatch.DrawOnCtrl(
                    this,
                    this.Background,
                    bounds,
                    new Rectangle(this.TextureOffset.X + this.TextureRectangle.X, this.TextureOffset.Y + this.TextureRectangle.Y, textRect.Width, textRect.Height),
                    this.BackgroundTint,
                    0f,
                    default);
            }

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

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected void SwitchTab(PanelTab tab = null)
        {
            foreach (PanelTab t in this.Tabs)
            {
                if (t != tab)
                {
                    t.Active = false;
                }
            }

            if (tab != null)
            {
                tab.Active = true;
            }

            this.activeTab = tab;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.tabs.DisposeAll();
        }

        private void OnTabButtonPanelResized(object sender, ResizedEventArgs e)
        {
            this.RecalculateLayout();
        }

        private void TabButton_Click(object sender, MouseEventArgs e, PanelTab t)
        {
            this.SwitchTab(t);
        }

        private void OnTabDisposed(object sender, EventArgs e)
        {
            this.RemoveTab((PanelTab)sender);
        }
    }
}
