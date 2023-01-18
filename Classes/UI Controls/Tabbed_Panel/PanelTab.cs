namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class PanelTab : Panel
    {
        private TabButton _tabButton;

        public TabButton TabButton
        {
            get => this._tabButton;
            private set => this._tabButton = value;
        }

        private event EventHandler IconChanged;

        private AsyncTexture2D _icon;

        public AsyncTexture2D Icon
        {
            get => this._icon;
            set
            {
                this._icon = value;
                this._tabButton.Icon = this.Icon;
                this.IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private event EventHandler TextureRectangleChanged;

        private Rectangle _textureRectangle = Rectangle.Empty;

        public Rectangle TextureRectangle
        {
            get => this._textureRectangle;
            set
            {
                this._textureRectangle = value;
                this._tabButton.TextureRectangle = value;
                this.TextureRectangleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private string _name;

        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this._tabButton.BasicTooltipText = value;
            }
        }

        private event EventHandler Activated;

        private event EventHandler Deactivated;

        private bool _active;

        public bool Active
        {
            get => this._active;
            set
            {
                this._active = value;
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
            this._tabButton = new TabButton()
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

            this._tabButton?.Dispose();
            this._icon = null;
        }
    }
}
