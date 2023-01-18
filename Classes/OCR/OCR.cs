using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Classes.Classes.UI_Controls;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Patagames.Ocr;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes
{
    public class OCR : IDisposable
    {
        BasicFrameContainer ContentContainer;

        private Rectangle CustomOffset
        {
            get => Characters.ModuleInstance.Settings._OCRCustomOffset.Value;
            set => Characters.ModuleInstance.Settings._OCRCustomOffset.Value = value;
        }
        private int CustomThreshold
        {
            get => Characters.ModuleInstance.Settings._OCRNoPixelColumns.Value;
            set => Characters.ModuleInstance.Settings._OCRNoPixelColumns.Value = value;
        }

        private Point WindowOffset = new Point(0, 0);
        private System.Drawing.Color spacingColor = System.Drawing.Color.FromArgb(255, 200, 200, 200);
        private System.Drawing.Color ignoredColor = System.Drawing.Color.FromArgb(255, 100, 100, 100);

        TextBox LeftBox;
        TextBox TopBox;
        TextBox RightBox;
        TextBox BottomBox;
        TextBox ColumnBox;

        Label Instructions;
        Label Result;
        Image OCR_ResultImage;
        Image OCR_ResultImageBlackWhite;
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

            var tM = Characters.ModuleInstance.TextureManager;


            ContentContainer = new BasicFrameContainer()
            {
                Parent = GameService.Graphics.SpriteScreen,
                FrameColor = Color.Black,// new Color(32, 32 , 32),
                Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ZIndex = 999,
                Visible = false,
            };

            var contentFlowPanel = new FlowPanel()
            {
                Parent = ContentContainer,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(5, 5),
                OuterControlPadding = new Vector2(5, 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            Instructions = new Label()
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

            var OffsetLabel = new Label()
            {
                Parent = flowPanel,
                Text = "Offset",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Custom Offset to make sure the frame is not included in the capture."
            };

            LeftBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Left.ToString(),
                BasicTooltipText = "Left Offset",
            };
            LeftBox.TextChanged += OffsetChanged;

            TopBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Top.ToString(),
                BasicTooltipText = "Top Offset",
            };
            TopBox.TextChanged += OffsetChanged;

            RightBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Right.ToString(),
                BasicTooltipText = "Right Offset",
            };
            RightBox.TextChanged += OffsetChanged;

            BottomBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = CustomOffset.Bottom.ToString(),
                BasicTooltipText = "Bottom Offset",
            };
            BottomBox.TextChanged += OffsetChanged;


            var ThresholdLabel = new Label()
            {
                Parent = flowPanel,
                Text = "Empty Columns",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Threshold of how many pixel columns are allowed to contain no content"
            };
            ColumnBox = new TextBox()
            {
                Parent = flowPanel,
                Size = new Point(50, 25),
                Text = CustomThreshold.ToString(),
                BasicTooltipText = "Number of rows",
            };
            ColumnBox.TextChanged += ColumnThresholdChanged;

            new Panel()
            {
                Parent = flowPanel,
                BackgroundColor = new Color(spacingColor.R, spacingColor.G, spacingColor.B, spacingColor.A),
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
                BackgroundColor = new Color(ignoredColor.R, ignoredColor.G, ignoredColor.B, ignoredColor.A),
                Size = new Point(25, 25),
            };
            new Label()
            {
                Parent = flowPanel,
                Text = "Ignored Part",
                Height = 25,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                BasicTooltipText = "Ignored part of the image due to the empty column threshold"
            };


            Result = new Label()
            {
                Parent = contentFlowPanel,
                AutoSizeHeight = false,
                Height = 50,
                AutoSizeWidth = true,
                TextColor = ContentService.Colors.ColonialWhite,
                Font = GameService.Content.DefaultFont32,
            };

            OCR_ResultImage = new Image()
            {
                Parent = contentFlowPanel,
            };

            OCR_ResultImageBlackWhite = new Image()
            {
                Parent = contentFlowPanel,
            };

            Container = new SizeablePanel()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 999,
                Visible = false,
                Location = Characters.ModuleInstance.Settings.OCRRegion.Location,
                Size = Characters.ModuleInstance.Settings.OCRRegion.Size,
                TintOnHover = false,
                ShowResizeOnlyOnMouseOver = true,
            };
            Container.Resized += Container_Changed;
            Container.Moved += Container_Changed;
            Container.LeftMouseButtonReleased += Container_LeftMouseButtonReleased;
            Container.MouseLeft += Container_LeftMouseButtonReleased;

            ContentContainer.Location = new Point(Container.Left, Container.Top - (Container.Height * 3) - 60);
        }

        private void ColumnThresholdChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ColumnBox.Text, out int threshold))
            {
                CustomThreshold = threshold;
            }
        }

        private async void Container_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            if (!Container.MouseOver)
            {
                await DelayedRead();
            }
        }

        private async Task DelayedRead()
        {
            await Task.Delay(5);
            Read(true);
        }

        private void OffsetChanged(object sender, EventArgs e)
        {
            int left = CustomOffset.Left, top = CustomOffset.Top, right = CustomOffset.Right, bottom = CustomOffset.Bottom;

            int.TryParse(LeftBox.Text, out left);
            int.TryParse(TopBox.Text, out top);
            int.TryParse(RightBox.Text, out right);
            int.TryParse(BottomBox.Text, out bottom);

            CustomOffset = new Rectangle(left, top, right, bottom);
            Read(true);
        }

        private void Container_Changed(object sender, EventArgs e)
        {
            var res = GameService.Graphics.Resolution.ToString();
            var regions = Characters.ModuleInstance.Settings._OCRRegions.Value;

            Read(true);

            if (!regions.ContainsKey(res))
            {
                regions.Add(res, Container.LocalBounds);
            }
            else
            {
                regions[res] = Container.LocalBounds;
            }

            ContentContainer.Location = new Point(Container.Left, Container.Top - ContentContainer.Height - 5);
        }

        public void ToggleContainer()
        {
            ContentContainer?.ToggleVisibility();
            Container?.ToggleVisibility();

            if (Container.Visible)
            {
                Read(true);
            }
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
                OCR_ResultImageBlackWhite?.Dispose();
                Result?.Dispose();
                ContentContainer?.Dispose();
            }
        }

        public string? Read(bool show = false)
        {
            string plainText = null;
            string finalText = null;

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
                System.Drawing.Bitmap spacingVisibleBitmap = new System.Drawing.Bitmap(size.X, size.Y);

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    WindowOffset = new Point(TitleBarHeight, SideBarWidth);

                    var left = (int)((wndBounds.Left + SideBarWidth));
                    var top = (int)((wndBounds.Top + TitleBarHeight));

                    var x = (int)Math.Ceiling((Container.Left - 10 + CustomOffset.Left) * factor);
                    var y = (int)Math.Ceiling((Container.Top - 10 + CustomOffset.Top) * factor);

                    g.CopyFromScreen(new System.Drawing.Point(left + x, top + y), System.Drawing.Point.Empty, new System.Drawing.Size(size.X - CustomOffset.Right, size.Y - CustomOffset.Bottom));


                    if (show)
                    {
                        using (MemoryStream s = new MemoryStream())
                        {
                            bitmap.Save(s, System.Drawing.Imaging.ImageFormat.Bmp);

                            OCR_ResultImage.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                            OCR_ResultImage.Texture = s.CreateTexture2D();
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

                            if (oc.R >= threshold && oc.G >= threshold && oc.B >= threshold && emptyPixelRow < CustomThreshold)
                            {
                                bitmap.SetPixel(i, j, black);
                                if (show) spacingVisibleBitmap.SetPixel(i, j, black);
                                containsPixel = true;
                            }
                            else if (emptyPixelRow >= CustomThreshold)
                            {
                                if(show) spacingVisibleBitmap.SetPixel(i, j, ignoredColor);
                                bitmap.SetPixel(i, j, white);
                            }
                            else
                            {
                                if (show) spacingVisibleBitmap.SetPixel(i, j, white);
                                bitmap.SetPixel(i, j, white);
                            }
                        }

                        if (emptyPixelRow < CustomThreshold && show)
                        {
                            if (!containsPixel)
                            {
                                for (int j = 0; j < bitmap.Height; j++)
                                {
                                    spacingVisibleBitmap.SetPixel(i, j, spacingColor);
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
                            OCR_ResultImageBlackWhite.Size = new Point(bitmap.Size.Width, bitmap.Size.Height);
                            OCR_ResultImageBlackWhite.Texture = s.CreateTexture2D();
                        }
                    }
                }

                plainText = _ocrApi.GetTextFromImage(bitmap);
                //plainText = plainText?.TrimEnd();

                foreach (string word in plainText.Split(' '))
                {
                    var wordText = word.Trim();

                    if (wordText.StartsWith("l"))
                    {
                        wordText = 'I' + wordText.Remove(0, 1);
                    }

                    finalText = finalText == null ? wordText : finalText + " " + wordText;
                }

                finalText = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(finalText.ToLower());

                Result.Text = finalText;
            }

            return finalText;
        }
    }
}
