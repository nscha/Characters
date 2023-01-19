using Blish_HUD.Content;
using Blish_HUD.Controls;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class PanelTab : Panel
    {
        private AsyncTexture2D icon;
        private Rectangle textureRectangle = Rectangle.Empty;
        private bool active;
        private string name;
        private TabButton tabButton;

        public PanelTab()
        {
            tabButton = new TabButton()
            {
                BasicTooltipText = Name,
            };
        }

        private event EventHandler Activated;

        private event EventHandler TextureRectangleChanged;

        private event EventHandler Deactivated;

        private event EventHandler IconChanged;

        public TabButton TabButton
        {
            get => tabButton;
            private set => tabButton = value;
        }

        public AsyncTexture2D Icon
        {
            get => icon;
            set
            {
                icon = value;
                tabButton.Icon = Icon;
                IconChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Rectangle TextureRectangle
        {
            get => textureRectangle;
            set
            {
                textureRectangle = value;
                tabButton.TextureRectangle = value;
                TextureRectangleChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                tabButton.BasicTooltipText = value;
            }
        }

        public bool Active
        {
            get => active;
            set
            {
                active = value;
                TabButton.Active = value;

                if (value)
                {
                    OnActivated();
                }
                else
                {
                    OnDeactivated();
                }
            }
        }

        protected void OnActivated()
        {
            Show();
            Activated?.Invoke(this, EventArgs.Empty);
        }

        protected void OnDeactivated()
        {
            Hide();
            Deactivated?.Invoke(this, EventArgs.Empty);
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            tabButton?.Dispose();
            icon = null;
        }
    }
}
