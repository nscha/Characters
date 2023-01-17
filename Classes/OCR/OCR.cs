using Patagames.Ocr;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
using System.Diagnostics;

namespace Kenedia.Modules.Characters.Classes
{
    public class OCR : IDisposable
    {

        Label Instructions;
        Label Result;
        Image OCR_ResultImage;
        SizeablePanel Container;
        OcrApi _ocrApi;
        string BasePath
        {
            get => Characters.ModuleInstance.BasePath;
        }

        public OCR()
        {
            OcrApi.PathToEngine = BasePath + @"\tesseract.dll";

            _ocrApi = OcrApi.Create();
            _ocrApi.Init(BasePath + @"\", "gw2");

            Result = new Label()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
            };

            Instructions = new Label()
            {
                Text = "Move and Size the container to contain only the character name row in the character selection!",
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
            };

            OCR_ResultImage = new Image()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
            };

            Container = new SizeablePanel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                Location = Characters.ModuleInstance.Settings.OCRRegion.Location,
                Size = Characters.ModuleInstance.Settings.OCRRegion.Size,
            };
            Container.Resized += Container_Changed;
            Container.Moved += Container_Changed;
            Container.LeftMouseButtonReleased += Container_LeftMouseButtonReleased;

            Instructions.Location = new Point(Container.Left, Container.Top - Instructions.Height - 2);
            Result.Location = new Point(Container.Left, Container.Top - Instructions.Height - 2 - Result.Height - 20);

        }

        private void Container_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            var res = GameService.Graphics.Resolution.ToString();
            var regions = Characters.ModuleInstance.Settings._OCRRegions.Value;
            Instructions.Location = new Point(Container.Left, Container.Top - Instructions.Height - 2);
            Result.Location = new Point(Container.Left, Container.Top - Instructions.Height - 2 - Result.Height - 20);
            Read(true);

            if (!regions.ContainsKey(res))
            {
                regions.Add(res, Container.LocalBounds);
            }
            else
            {
                regions[res] = Container.LocalBounds;
            }
        }

        public void ToggleContainer()
        {
            Debug.WriteLine("TOGGLE CONTAINER");
            Instructions?.ToggleVisibility();
            Container?.ToggleVisibility();
            OCR_ResultImage?.ToggleVisibility();
            //Result?.ToggleVisibility();
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Container?.Dispose();
                Instructions?.Dispose();
                OCR_ResultImage?.Dispose();
                Result?.Dispose();
            }
        }

        public string? Read(bool show = false)
        {
            string plainText = null;

            if (Container.Visible && !show)
            {
                ToggleContainer();
                return null;
            }

            var windowed = Characters.ModuleInstance.Settings._WindowedMode.Value;
            var wndBounds = Characters.ModuleInstance.WindowRectangle;
            var TitleBarHeight = !windowed ? 0 : Characters.ModuleInstance.TitleBarHeight;
            var SideBarWidth = !windowed ? 0 : Characters.ModuleInstance.SideBarWidth;

            double factor = GameService.Graphics.UIScaleMultiplier;
            var size = new Point(Math.Min((int)((Container.Width + 5) * factor), 499), Math.Min((int)((Container.Height + 5) * factor), 499));

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(size.X, size.Y))
            {

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var left = (int)((wndBounds.Left + SideBarWidth));
                    var top = (int)((wndBounds.Top + TitleBarHeight));

                    var x = (int)Math.Ceiling((Container.Left - 10) * factor);
                    var y = (int)Math.Ceiling((Container.Top - 10) * factor);

                    g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X, size.Y));

                    var blackAndWhite = !show;

                    if (blackAndWhite)
                    {
                        for (int i = 0; i < bitmap.Width; i++)
                        {
                            for (int j = 0; j < bitmap.Height; j++)
                            {
                                System.Drawing.Color oc = bitmap.GetPixel(i, j);
                                var threshold = 150;

                                if (oc.R >= threshold && oc.G >= threshold && oc.B >= threshold)
                                {
                                    System.Drawing.Color nc = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                                    bitmap.SetPixel(i, j, nc);
                                }
                                else
                                {
                                    System.Drawing.Color nc = System.Drawing.Color.FromArgb(255, 255, 255, 255);
                                    bitmap.SetPixel(i, j, nc);
                                }
                            }
                        }
                    }

                    using (MemoryStream s = new MemoryStream())
                    {
                        bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                        if (show)
                        {
                            OCR_ResultImage.Size = size;
                            OCR_ResultImage.Texture = s.CreateTexture2D();
                            OCR_ResultImage.Location = new Point(Container.Left, Instructions.Top - size.Y - 5);
                        }
                    }
                }

                plainText = _ocrApi.GetTextFromImage(bitmap);

                plainText = plainText?.Trim();
                Result.Text = plainText;
            }

            return plainText;
        }
    }
}
