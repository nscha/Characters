namespace Kenedia.Modules.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework.Graphics;

    public enum Controls
    {
        Separator,
        Plus_Button,
        Plus_Button_Hovered,
        Minus_Button,
        Minus_Button_Hovered,
        ZoomIn_Button,
        ZoomIn_Button_Hovered,
        ZoomOut_Button,
        ZoomOut_Button_Hovered,
        Drag_Button,
        Drag_Button_Hovered,
        Potrait_Button,
        Potrait_Button_Hovered,
        Delete_Button,
        Delete_Button_Hovered,
    }

    public enum Icons
    {
        Bug,
        ModuleIcon,
        ModuleIcon_Hovered,
        ModuleIcon_HoveredWhite,
        Folder,
        FolderGray,
        Camera,
    }

    public enum Backgrounds
    {
        MainWindow,
    }

    public class TextureManager : IDisposable
    {
        public List<string> TextureFiles = new List<string>()
            {
                "155985.png", // Background
                "156015.png", // Emblem
                "156678.png", // Icon Normal
                "156679.png", // Icon Hovered
                "155052.png", // Cog
                "157110.png", // Cog Hovered
            };

        public List<Texture2D> _Backgrounds = new List<Texture2D>();
        public List<Texture2D> _Icons = new List<Texture2D>();
        public List<Texture2D> _Controls = new List<Texture2D>();

        private bool disposed = false;

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                this._Backgrounds?.DisposeAll();
                this._Icons?.DisposeAll();
                this._Controls?.DisposeAll();
            }
        }

        public TextureManager()
        {
            var contentsManager = Characters.ModuleInstance.ContentsManager;

            var values = Enum.GetValues(typeof(Backgrounds));
            if (values.Length > 0)
            {
                this._Backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Backgrounds num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this._Backgrounds.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Icons));
            if (values.Length > 0)
            {
                this._Icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Icons num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this._Icons.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Controls));
            if (values.Length > 0)
            {
                this._Controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Controls num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this._Controls.Insert((int)num, texture);
                    }
                }
            }
        }

        public Texture2D GetBackground(Backgrounds background)
        {
            var index = (int)background;

            if (index < this._Backgrounds.Count && this._Backgrounds[index] != null)
            {
                return this._Backgrounds[index];
            }

            return this._Icons[0];
        }

        public Texture2D GetIcon(Icons icon)
        {
            var index = (int)icon;

            if (index < this._Icons.Count && this._Icons[index] != null)
            {
                return this._Icons[index];
            }

            return this._Icons[0];
        }

        public Texture2D GetControlTexture(Controls control)
        {
            var index = (int)control;
            if (index < this._Controls.Count && this._Controls[index] != null)
            {
                return this._Controls[index];
            }

            return this._Icons[0];
        }
    }
}
