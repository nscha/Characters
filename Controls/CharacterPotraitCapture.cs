using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterPotraitCapture : Container
    {
        private readonly Checkbox windowedCheckbox;
        private readonly ImageButton captureButton;
        private readonly ImageButton addButton;
        private readonly ImageButton removeButton;
        private readonly Label disclaimer;
        private readonly BasicFrameContainer disclaimerBackground;
        private readonly ImageButton dragButton;
        private readonly TextBox sizeBox;
        private readonly TextBox gapBox;
        private readonly FlowPanel portraitsPanel;

        private bool dragging;
        private Point draggingStart;

        private DateTime capturePotraits;
        private int characterPotraitSize = 130;
        private int gap = 13;

        public CharacterPotraitCapture()
        {
            Point res = GameService.Graphics.Resolution;
            Size = new Point(100, 100);
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            Location = new Point((res.X - Size.X) / 2, res.Y - 125 - Size.Y);
            Services.TextureManager tM = Characters.ModuleInstance.TextureManager;
            addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Plus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 1, 0),
                BasicTooltipText = string.Format(Strings.common.AddItem, Strings.common.PotraitFrame),
            };
            addButton.Click += AddButton_Click;

            removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Minus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 2, 0),
                BasicTooltipText = string.Format(Strings.common.RemoveItem, Strings.common.PotraitFrame),
            };
            removeButton.Click += RemoveButton_Click;

            dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Drag_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                BasicTooltipText = Strings.common.DragOverCharacter_Instructions,
            };
            dragButton.LeftMouseButtonPressed += DragButton_LeftMouseButtonPressed;
            dragButton.LeftMouseButtonReleased += DragButton_LeftMouseButtonReleased;

            captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Potrait_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 3, 0),
                BasicTooltipText = Strings.common.CapturePotraits,
            };
            captureButton.Click += CaptureButton_Click;

            disclaimerBackground = new BasicFrameContainer()
            {
                Parent = this,
                Location = new Point((32 + 5) * 4, 0),
                FrameColor = Color.Black, // new Color(32, 32 , 32),
                Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(15, 0),
                Height = 32,
            };

            windowedCheckbox = new Checkbox()
            {
                Parent = disclaimerBackground,
                Text = Strings.common.WindowedMode,
                BasicTooltipText = Strings.common.WindowedMode_Tooltip,
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Location = new Point(5, 0),
                Height = 32,
            };
            windowedCheckbox.CheckedChanged += WindowedCheckbox_CheckedChanged;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { windowedCheckbox.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            disclaimer = new Label()
            {
                Parent = disclaimerBackground,
                Location = new Point(windowedCheckbox.Right + 5, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                Text = Strings.common.BestResultLargerDisclaimer,
                Padding = new Thickness(0f, 0f),
            };
            disclaimer.Resized += Disclaimer_Resized;

            // _disclaimerBackground.Size = (_disclaimer.Size).Add(new Point(_windowedCheckbox.Width + 15, 0));
            sizeBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 35),
                Size = new Point(50, 25),
                Text = characterPotraitSize.ToString(),
                BasicTooltipText = Strings.common.PotraitSize,
            };
            sizeBox.TextChanged += SizeBox_TextChanged;
            gapBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 65),
                Size = new Point(50, 25),
                Text = gap.ToString(),
                BasicTooltipText = Strings.common.PotraitGap,
            };
            gapBox.TextChanged += GapBox_TextChanged;

            portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(55, 35),
                Size = new Point(110, 110),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(gap, 0),
            };

            AddPotrait();
            AddPotrait();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            dragging = dragging && MouseOver;

            if (dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-draggingStart.X, -draggingStart.Y));
            }

            if (capturePotraits > DateTime.MinValue && DateTime.Now.Subtract(capturePotraits).TotalMilliseconds >= 5)
            {
                foreach (Control c in portraitsPanel.Children)
                {
                    CapturePotrait(c.AbsoluteBounds);
                }

                foreach (Control c in portraitsPanel.Children)
                {
                    c.Show();
                }

                capturePotraits = DateTime.MinValue;
                Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
            }
        }

        protected override void OnResized(ResizedEventArgs e) => base.OnResized(e);

        protected override void OnClick(MouseEventArgs e) => base.OnClick(e);

        private void WindowedCheckbox_CheckedChanged(object sender, CheckChangedEvent e) => Characters.ModuleInstance.Settings.WindowedMode.Value = windowedCheckbox.Checked;

        private void Disclaimer_Resized(object sender, ResizedEventArgs e) => disclaimerBackground.Size = disclaimer.Size.Add(new Point(10, 0));

        private void CaptureButton_Click(object sender, MouseEventArgs e)
        {
            capturePotraits = DateTime.Now;

            foreach (Control c in portraitsPanel.Children)
            {
                c.Hide();
            }
        }

        private void SizeBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (int.TryParse(sizeBox.Text, out s))
            {
                characterPotraitSize = s;
                Point p = new(s, s);
                foreach (Control c in portraitsPanel.Children)
                {
                    c.Size = p;
                }
            }
        }

        private void GapBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (int.TryParse(gapBox.Text, out s))
            {
                gap = s;
                portraitsPanel.ControlPadding = new Vector2(s, 0);
            }
        }

        private void RemoveButton_Click(object sender, MouseEventArgs e)
        {
            if (portraitsPanel.Children.Count > 1)
            {
                portraitsPanel.Children.RemoveAt(portraitsPanel.Children.Count - 1);
            }
        }

        private void AddButton_Click(object sender, MouseEventArgs e) => AddPotrait();

        private void AddPotrait() => _ = new BasicFrameContainer()
        {
            Parent = portraitsPanel,
            Size = new Point(characterPotraitSize, characterPotraitSize),
        };

        private void DragButton_LeftMouseButtonReleased(object sender, MouseEventArgs e) => dragging = false;

        private void DragButton_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            dragging = true;
            draggingStart = dragging ? RelativeMousePosition : Point.Zero;
        }

        private void CapturePotrait(Rectangle bounds)
        {
            string path = Characters.ModuleInstance.AccountImagesPath;

            Regex regex = new("Image.*[0-9].png");
            System.Collections.Generic.List<string> images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

            IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            RECT wndBounds = default(RECT);
            RECT clientRectangle = default(RECT);
            _ = GetWindowRect(hWnd, ref wndBounds);
            _ = GetClientRect(hWnd, out clientRectangle);

            bool fullscreen = !Characters.ModuleInstance.Settings.WindowedMode.Value;

            int titleBarHeight = fullscreen ? 0 : wndBounds.Bottom - wndBounds.Top - (clientRectangle.Bottom - clientRectangle.Top) - 6;
            int sideBarWidth = fullscreen ? 0 : wndBounds.Right - wndBounds.Left - (clientRectangle.Right - clientRectangle.Left) - 7;

            Rectangle cPos = bounds;
            double factor = GameService.Graphics.UIScaleMultiplier;

            using (System.Drawing.Bitmap bitmap = new((int)((characterPotraitSize - 2) * factor), (int)((characterPotraitSize - 2) * factor)))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    int x = (int)(bounds.X * factor);
                    int y = (int)(bounds.Y * factor);

                    g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + x + sideBarWidth, clientRectangle.Top + y + titleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(characterPotraitSize - 2, characterPotraitSize));
                }

                bitmap.Save(path + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
