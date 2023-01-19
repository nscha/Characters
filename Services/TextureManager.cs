using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenedia.Modules.Characters.Services
{
    public class TextureManager : IDisposable
    {
        private readonly List<Texture2D> backgrounds = new();
        private readonly List<Texture2D> icons = new();
        private readonly List<Texture2D> controls = new();

        private bool disposed = false;

        public TextureManager()
        {
            Blish_HUD.Modules.Managers.ContentsManager contentsManager = Characters.ModuleInstance.ContentsManager;

            Array values = Enum.GetValues(typeof(Backgrounds));
            if (values.Length > 0)
            {
                backgrounds = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Backgrounds num in values)
                {
                    Texture2D texture = contentsManager.GetTexture(@"textures\backgrounds\" + (int)num + ".png");
                    if (texture != null)
                    {
                        backgrounds.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Icons));
            if (values.Length > 0)
            {
                icons = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Icons num in values)
                {
                    Texture2D texture = contentsManager.GetTexture(@"textures\icons\" + (int)num + ".png");
                    if (texture != null)
                    {
                        icons.Insert((int)num, texture);
                    }
                }
            }

            values = Enum.GetValues(typeof(Controls));
            if (values.Length > 0)
            {
                controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (Controls num in values)
                {
                    Texture2D texture = contentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                    if (texture != null)
                    {
                        controls.Insert((int)num, texture);
                    }
                }
            }
        }

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

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                backgrounds?.DisposeAll();
                icons?.DisposeAll();
                controls?.DisposeAll();
            }
        }

        public Texture2D GetBackground(Backgrounds background)
        {
            int index = (int)background;

            if (index < backgrounds.Count && backgrounds[index] != null)
            {
                return backgrounds[index];
            }

            return icons[0];
        }

        public Texture2D GetIcon(Icons icon)
        {
            int index = (int)icon;

            if (index < icons.Count && icons[index] != null)
            {
                return icons[index];
            }

            return icons[0];
        }

        public Texture2D GetControlTexture(Controls control)
        {
            int index = (int)control;
            if (index < controls.Count && controls[index] != null)
            {
                return controls[index];
            }

            return icons[0];
        }
    }
}
