namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class PanelTab : Panel
    {
        private TabButton tabButton;

        public TabButton TabButton
        {
            get => this.tabButton;
            private set => this.tabButton = value;
        }

        private event EventHandler IconChanged;

        private AsyncTexture2D icon;

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

        private event EventHandler TextureRectangleChanged;

        private Rectangle textureRectangle = Rectangle.Empty;

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

        private string name;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.tabButton.BasicTooltipText = value;
            }
        }

        private event EventHandler Activated;

        private event EventHandler Deactivated;

        private bool active;

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

        public PanelTab()
        {
            this.tabButton = new TabButton()
            {
                BasicTooltipText = this.Name,
            };
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
