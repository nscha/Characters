namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class PanelTab : Panel
    {
        private AsyncTexture2D icon;
        private Rectangle textureRectangle = Rectangle.Empty;
        private bool active;
        private string name;
        private TabButton tabButton;

        public PanelTab()
        {
            this.tabButton = new TabButton()
            {
                BasicTooltipText = this.Name,
            };
        }

        private event EventHandler Activated;

        private event EventHandler TextureRectangleChanged;

        private event EventHandler Deactivated;

        private event EventHandler IconChanged;

        public TabButton TabButton
        {
            get => this.tabButton;
            private set => this.tabButton = value;
        }

        public AsyncTexture2D Icon
        {
            get => this.icon;
            set
            {
                this.icon = value;
                this.tabButton.Icon = this.Icon;
                this.IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Rectangle TextureRectangle
        {
            get => this.textureRectangle;
            set
            {
                this.textureRectangle = value;
                this.tabButton.TextureRectangle = value;
                this.TextureRectangleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.tabButton.BasicTooltipText = value;
            }
        }

        public bool Active
        {
            get => this.active;
            set
            {
                this.active = value;
                this.TabButton.Active = value;

                if (value)
                {
                    this.OnActivated();
                }
                else
                {
                    this.OnDeactivated();
                }
            }
        }

        protected void OnActivated()
        {
            this.Show();
            this.Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeactivated()
        {
            this.Hide();
            this.Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            this.tabButton?.Dispose();
            this.icon = null;
        }
    }
}
