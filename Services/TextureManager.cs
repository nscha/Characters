﻿using Kenedia.Modules.Characters.Extensions;
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

            values = Enum.GetValues(typeof(ControlTextures));
            if (values.Length > 0)
            {
                controls = new List<Texture2D>(new Texture2D[values.Cast<int>().Max() + 1]);
                foreach (ControlTextures num in values)
                {
                    Texture2D texture = contentsManager.GetTexture(@"textures\controls\" + (int)num + ".png");
                    if (texture != null)
                    {
                        controls.Insert((int)num, texture);
                    }
                }
            }
        }

        public enum ControlTextures
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
            Random_Button,
            Random_Button_Hovered,
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
            Dice,
            Dice_Hovered,
            Pin,
            Pin_Hovered,
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

            return index < backgrounds.Count && backgrounds[index] != null ? backgrounds[index] : icons[0];
        }

        public Texture2D GetIcon(Icons icon)
        {
            int index = (int)icon;

            return index < icons.Count && icons[index] != null ? icons[index] : icons[0];
        }

        public Texture2D GetControlTexture(ControlTextures control)
        {
            int index = (int)control;
            return index < controls.Count && controls[index] != null ? controls[index] : icons[0];
        }
    }
}
