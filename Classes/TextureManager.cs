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
        private readonly List<Texture2D> backgrounds = new List<Texture2D>();
        private readonly List<Texture2D> icons = new List<Texture2D>();
        private readonly List<Texture2D> controls = new List<Texture2D>();

        private bool disposed = false;

        public TextureManager()
        {
            var contentsManager = Characters.ModuleInstance.ContentsManager;

            var values = Enum.GetValues(typeof(Backgrounds));
            if (values.Length > 0)
            {
                this.backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Backgrounds num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this.backgrounds.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Icons));
            if (values.Length > 0)
            {
                this.icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Icons num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this.icons.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Controls));
            if (values.Length > 0)
            {
                this.controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Controls num in values)
                {
                    var texture = contentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                    if (texture != null)
                    {
                        this.controls.Insert((int)num, texture);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;

                this.backgrounds?.DisposeAll();
                this.icons?.DisposeAll();
                this.controls?.DisposeAll();
            }
        }

        public Texture2D GetBackground(Backgrounds background)
        {
            var index = (int)background;

            if (index < this.backgrounds.Count && this.backgrounds[index] != null)
            {
                return this.backgrounds[index];
            }

            return this.icons[0];
        }

        public Texture2D GetIcon(Icons icon)
        {
            var index = (int)icon;

            if (index < this.icons.Count && this.icons[index] != null)
            {
                return this.icons[index];
            }

            return this.icons[0];
        }

        public Texture2D GetControlTexture(Controls control)
        {
            var index = (int)control;
            if (index < this.controls.Count && this.controls[index] != null)
            {
                return this.controls[index];
            }

            return this.icons[0];
        }
    }
}
