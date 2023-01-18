namespace Kenedia.Modules.Characters.Classes
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.Characters.Classes.Classes.UI_Controls;
    using Kenedia.Modules.Characters.Classes.UI_Controls;
    using Microsoft.Xna.Framework;
    using Patagames.Ocr;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class OCR : IDisposable
    {
        private readonly BasicFrameContainer contentContainer;
        private readonly System.Drawing.Color spacingColor = System.Drawing.Color.FromArgb(255, 200, 200, 200);
        private readonly System.Drawing.Color ignoredColor = System.Drawing.Color.FromArgb(255, 100, 100, 100);
        private readonly TextBox leftBox;
        private readonly TextBox topBox;
        private readonly TextBox rightBox;
        private readonly TextBox bottomBox;
        private readonly TextBox columnBox;
        private readonly Label instructions;
        private readonly Label result;
        private readonly Image oCRResultImage;
        private readonly Image oCRResultImageBlackWhite;
        private readonly SizeablePanel container;
        private readonly OcrApi ocrApi;
        private bool disposed = false;

        public OCR()
        {
            OcrApi.PathToEngine = this.BasePath + @"\tesseract.dll";

            this.ocrApi = OcrApi.Create();
            this.ocrApi.Init(this.BasePath + @"\", "gw2");

            var tM = Characters.ModuleInstance.TextureManager;

            this.contentContainer = new BasicFrameContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                FrameColor = Color.Black, // new Color(32, 32 , 32),
                Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ZIndex = 999,
                Visible = false,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = this.contentContainer,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            this.instructions = new Label()
            {
                Text = "Move and Size the yellow frame to contain only the character name row in the character selection!",
                Parent = contentFlowPanel,
                AutoSizeHeight = true,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
            };

            var flowPanel = new FlowPanel()
            {
                Parent = contentFlowPanel,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                ControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
            };

            var offsetLabel = new Label()
            {
                Parent = flowPanel,
                Text = "Offset",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Custom Offset to make sure the frame is not included in the capture.",
            };

            this.leftBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = this.CustomOffset.Left.ToString(),
                BasicTooltipText = "Left Offset",
            };
            this.leftBox.TextChanged += this.OffsetChanged;

            this.topBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = this.CustomOffset.Top.ToString(),
                BasicTooltipText = "Top Offset",
            };
            this.topBox.TextChanged += this.OffsetChanged;

            this.rightBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = this.CustomOffset.Right.ToString(),
                BasicTooltipText = "Right Offset",
            };
            this.rightBox.TextChanged += this.OffsetChanged;

            this.bottomBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = this.CustomOffset.Bottom.ToString(),
                BasicTooltipText = "Bottom Offset",
            };
            this.bottomBox.TextChanged += this.OffsetChanged;

            var thresholdLabel = new Label()
            {
                Parent = flowPanel,
                Text = "Empty Columns",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Threshold of how many pixel columns are allowed to contain no content",
            };
            this.columnBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = this.CustomThreshold.ToString(),
                BasicTooltipText = "Number of rows",
            };
            this.columnBox.TextChanged += this.ColumnThresholdChanged;

            new Panel()
            {
                Parent = flowPanel,
                BackgroundColor = new Color(this.spacingColor.R, this.spacingColor.G, this.spacingColor.B, this.spacingColor.A),
                Size = new Point(25, 25),
            };
            new Label()
            {
                Parent = flowPanel,
                Text = "Empty Column",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Pixel Column which does not contain any 'white'ish' pixels",
            };

            new Panel()
            {
                Parent = flowPanel,
                BackgroundColor = new Color(this.ignoredColor.R, this.ignoredColor.G, this.ignoredColor.B, this.ignoredColor.A),
                Size = new Point(25, 25),
            };
            new Label()
            {
                Parent = flowPanel,
                Text = "Ignored Part",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Ignored part of the image due to the empty column threshold",
            };

            this.result = new Label()
            {
                Parent = contentFlowPanel,
                AutoSizeHeight = false,
                Height = 50,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
            };

            this.oCRResultImage = new Image()
            {
                Parent = contentFlowPanel,
            };

            this.oCRResultImageBlackWhite = new Image()
            {
                Parent = contentFlowPanel,
            };

            this.container = new SizeablePanel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                Location = Characters.ModuleInstance.Settings.ActiveOCRRegion.Location,
                Size = Characters.ModuleInstance.Settings.ActiveOCRRegion.Size,
                TintOnHover = false,
                ShowResizeOnlyOnMouseOver = true,
            };
            this.container.Resized += this.Container_Changed;
            this.container.Moved += this.Container_Changed;
            this.container.LeftMouseButtonReleased += this.Container_LeftMouseButtonReleased;
            this.container.MouseLeft += this.Container_LeftMouseButtonReleased;

            this.contentContainer.Location = new Point(this.container.Left, this.container.Top - (this.container.Height * 3) - 60);
        }

        private Rectangle CustomOffset
        {
            get => Characters.ModuleInstance.Settings.OCRCustomOffset.Value;
            set => Characters.ModuleInstance.Settings.OCRCustomOffset.Value = value;
        }

        private int CustomThreshold
        {
            get => Characters.ModuleInstance.Settings.OCRNoPixelColumns.Value;
            set => Characters.ModuleInstance.Settings.OCRNoPixelColumns.Value = value;
        }

        private string BasePath
        {
            get => Characters.ModuleInstance.BasePath;
        }

        public void ToggleContainer()
        {
            this.contentContainer?.ToggleVisibility();
            this.container?.ToggleVisibility();

            if (this.container.Visible)
            {
                this.Read(true);
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.container?.Dispose();
                this.instructions?.Dispose();
                this.oCRResultImage?.Dispose();
                this.oCRResultImageBlackWhite?.Dispose();
                this.result?.Dispose();
                this.contentContainer?.Dispose();
            }
        }

#nullable enable
        public string? Read(bool show = false)
        {
            string? plainText = null;
            string? finalText = null;

            if (this.container.Visible && !show)
            {
                this.ToggleContainer();
                return null;
            }

            var windowed = Characters.ModuleInstance.Settings.WindowedMode.Value;
            var wndBounds = Characters.ModuleInstance.WindowRectangle;
            var titleBarHeight = !windowed ? 0 : Characters.ModuleInstance.TitleBarHeight;
            var sideBarWidth = !windowed ? 0 : Characters.ModuleInstance.SideBarWidth;

            double factor = GameService.Graphics.UIScaleMultiplier;
            var size = new Point(Math.Min((int)((this.container.Width + 5) * factor), 499), Math.Min((int)((this.container.Height + 5) * factor), 499));

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(size.X, size.Y))
            {
                System.Drawing.Bitmap spacingVisibleBitmap = new System.Drawing.Bitmap(size.X, size.Y);

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var left = (int)(wndBounds.Left + sideBarWidth);
                    var top = (int)(wndBounds.Top + titleBarHeight);

                    var x = (int)Math.Ceiling((this.container.Left - 10 + this.CustomOffset.Left) * factor);
                    var y = (int)Math.Ceiling((this.container.Top - 10 + this.CustomOffset.Top) * factor);

                    g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X - this.CustomOffset.Right, size.Y - this.CustomOffset.Bottom));

                    if (show)
                    {
                        using (MemoryStream s = new MemoryStream())
                        {
                            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                            this.oCRResultImage.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                            this.oCRResultImage.Texture = s.CreateTexture2D();
                        }
                    }

                    var black = System.Drawing.Color.FromArgb(255, 0, 0, 0);
                    var white = System.Drawing.Color.FromArgb(255, 255, 255, 255);

                    var emptyPixelRow = 0;
                    for (int i = 0; i < bitmap.Width; i++)
                    {
                        var containsPixel = false;

                        for (int j = 0; j < bitmap.Height; j++)
                        {
                            System.Drawing.Color oc = bitmap.GetPixel(i, j);
                            var threshold = 150;

                            if (oc.R >= threshold && oc.G >= threshold && oc.B >= threshold && emptyPixelRow < this.CustomThreshold)
                            {
                                bitmap.SetPixel(i, j, black);
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, black);
                                }

                                containsPixel = true;
                            }
                            else if (emptyPixelRow >= this.CustomThreshold)
                            {
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, this.ignoredColor);
                                }

                                bitmap.SetPixel(i, j, white);
                            }
                            else
                            {
                                if (show)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, white);
                                }

                                bitmap.SetPixel(i, j, white);
                            }
                        }

                        if (emptyPixelRow < this.CustomThreshold && show)
                        {
                            if (!containsPixel)
                            {
                                for (int j = 0; j < bitmap.Height; j++)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, this.spacingColor);
                                }

                                emptyPixelRow++;
                            }
                            else
                            {
                                emptyPixelRow = 0;
                            }
                        }
                    }

                    using (MemoryStream s = new MemoryStream())
                    {
                        spacingVisibleBitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                        if (show)
                        {
                            this.oCRResultImageBlackWhite.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                            this.oCRResultImageBlackWhite.Texture = s.CreateTexture2D();
                        }
                    }
                }

                plainText = this.ocrApi.GetTextFromImage(bitmap);

                foreach (string word in plainText.Split(' '))
                {
                    var wordText = word.Trim();

                    if (wordText.StartsWith("l"))
                    {
                        wordText = 'I' + wordText.Remove(0, 1);
                    }

                    finalText = finalText == null ? wordText : finalText + " " + wordText;
                }

                finalText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(finalText?.ToLower());

                this.result.Text = finalText;
            }

            return finalText;
        }
#nullable disable

        private void ColumnThresholdChanged(object sender, EventArgs e)
        {
            if (int.TryParse(this.columnBox.Text, out int threshold))
            {
                this.CustomThreshold = threshold;
            }
        }

        private async void Container_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (!this.container.MouseOver)
            {
                await this.DelayedRead();
            }
        }

        private async Task DelayedRead()
        {
            await Task.Delay(5);
            this.Read(true);
        }

        private void OffsetChanged(object sender, EventArgs e)
        {
            int left = this.CustomOffset.Left, top = this.CustomOffset.Top, right = this.CustomOffset.Right, bottom = this.CustomOffset.Bottom;

            int.TryParse(this.leftBox.Text, out left);
            int.TryParse(this.topBox.Text, out top);
            int.TryParse(this.rightBox.Text, out right);
            int.TryParse(this.bottomBox.Text, out bottom);

            this.CustomOffset = new Rectangle(left, top, right, bottom);
            this.Read(true);
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            var res = GameService.Graphics.Resolution.ToString();
            var regions = Characters.ModuleInstance.Settings.OCRRegions.Value;

            this.Read(true);

            if (!regions.ContainsKey(res))
            {
                regions.Add(res, this.container.LocalBounds);
            }
            else
            {
                regions[res] = this.container.LocalBounds;
            }

            this.contentContainer.Location = new Point(this.container.Left, this.container.Top - this.contentContainer.Height - 5);
        }
    }
}
