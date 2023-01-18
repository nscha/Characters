namespace Kenedia.Modules.Characters.Classes.Classes.UI_Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.Characters.Classes.UI_Controls;
    using Microsoft.Xna.Framework;
    using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

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
            var res = GameService.Graphics.Resolution;
            this.Size = new Point(100, 100);
            this.WidthSizingMode = SizingMode.AutoSize;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.Location = new Point((res.X - this.Size.X) / 2, res.Y - 125 - this.Size.Y);
            var tM = Characters.ModuleInstance.TextureManager;
            this.addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Plus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 1, 0),
                BasicTooltipText = "Add Potrait Frame",
            };
            this.addButton.Click += this.AddButton_Click;

            this.removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Minus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 2, 0),
                BasicTooltipText = "Remove Potrait Frame",
            };
            this.removeButton.Click += this.RemoveButton_Click;

            this.dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Drag_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                BasicTooltipText = "Drag over your character Potraits",
            };
            this.dragButton.LeftMouseButtonPressed += this.DragButton_LeftMouseButtonPressed;
            this.dragButton.LeftMouseButtonReleased += this.DragButton_LeftMouseButtonReleased;

            this.captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Potrait_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 3, 0),
                BasicTooltipText = "Capture Potraits",
            };
            this.captureButton.Click += this.CaptureButton_Click;

            this.disclaimerBackground = new BasicFrameContainer()
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

            this.windowedCheckbox = new Checkbox()
            {
                Parent = this.disclaimerBackground,
                Text = "Game is in Windowed Mode",
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Location = new Point(5, 0),
                Height = 32,
            };
            this.windowedCheckbox.CheckedChanged += this.WindowedCheckbox_CheckedChanged;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { this.windowedCheckbox.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            this.disclaimer = new Label()
            {
                Parent = this.disclaimerBackground,
                Location = new Point(this.windowedCheckbox.Right + 5, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                Text = "Best results with Interface set to 'Larger'!",
                Padding = new Thickness(0f, 0f),
            };
            this.disclaimer.Resized += this.Disclaimer_Resized;

            // _disclaimerBackground.Size = (_disclaimer.Size).Add(new Point(_windowedCheckbox.Width + 15, 0));
            this.sizeBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 35),
                Size = new Point(50, 25),
                Text = this.characterPotraitSize.ToString(),
                BasicTooltipText = "Potrait Size",
            };
            this.sizeBox.TextChanged += this.SizeBox_TextChanged;
            this.gapBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 65),
                Size = new Point(50, 25),
                Text = this.gap.ToString(),
                BasicTooltipText = "Potrait Gap",
            };
            this.gapBox.TextChanged += this.GapBox_TextChanged;

            this.portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(55, 35),
                Size = new Point(110, 110),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(this.gap, 0),
            };

            this.AddPotrait();
            this.AddPotrait();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            this.dragging = this.dragging && this.MouseOver;

            if (this.dragging)
            {
                this.Location = Input.Mouse.Position.Add(new Point(-this.draggingStart.X, -this.draggingStart.Y));
            }

            if (this.capturePotraits > DateTime.MinValue && DateTime.Now.Subtract(this.capturePotraits).TotalMilliseconds >= 5)
            {
                foreach (Control c in this.portraitsPanel.Children)
                {
                    this.CapturePotrait(c.AbsoluteBounds);
                }

                foreach (Control c in this.portraitsPanel.Children)
                {
                    c.Show();
                }

                this.capturePotraits = DateTime.MinValue;
                Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
        }

        private void WindowedCheckbox_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Characters.ModuleInstance.Settings.WindowedMode.Value = this.windowedCheckbox.Checked;
        }

        private void Disclaimer_Resized(object sender, ResizedEventArgs e)
        {
            this.disclaimerBackground.Size = this.disclaimer.Size.Add(new Point(10, 0));
        }

        private void CaptureButton_Click(object sender, MouseEventArgs e)
        {
            this.capturePotraits = DateTime.Now;

            foreach (Control c in this.portraitsPanel.Children)
            {
                c.Hide();
            }
        }

        private void SizeBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (int.TryParse(this.sizeBox.Text, out s))
            {
                this.characterPotraitSize = s;
                var p = new Point(s, s);
                foreach (Control c in this.portraitsPanel.Children)
                {
                    c.Size = p;
                }
            }
        }

        private void GapBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (int.TryParse(this.gapBox.Text, out s))
            {
                this.gap = s;
                this.portraitsPanel.ControlPadding = new Vector2(s, 0);
            }
        }

        private void RemoveButton_Click(object sender, MouseEventArgs e)
        {
            if (this.portraitsPanel.Children.Count > 1)
            {
                this.portraitsPanel.Children.RemoveAt(this.portraitsPanel.Children.Count - 1);
            }
        }

        private void AddButton_Click(object sender, MouseEventArgs e)
        {
            this.AddPotrait();
        }

        private void AddPotrait()
        {
            new BasicFrameContainer()
            {
                Parent = this.portraitsPanel,
                Size = new Point(this.characterPotraitSize, this.characterPotraitSize),
            };
        }

        private void DragButton_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            this.dragging = false;
        }

        private void DragButton_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            this.dragging = true;
            this.draggingStart = this.dragging ? this.RelativeMousePosition : Point.Zero;
        }

        private void CapturePotrait(Rectangle bounds)
        {
            var path = Characters.ModuleInstance.AccountImagesPath;

            Regex regex = new Regex("Image.*[0-9].png");
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            var wndBounds = default(RECT);
            var clientRectangle = default(RECT);
            GetWindowRect(hWnd, ref wndBounds);
            GetClientRect(hWnd, out clientRectangle);

            var fullscreen = !Characters.ModuleInstance.Settings.WindowedMode.Value;

            var titleBarHeight = fullscreen ? 0 : wndBounds.Bottom - wndBounds.Top - (clientRectangle.Bottom - clientRectangle.Top) - 6;
            var sideBarWidth = fullscreen ? 0 : wndBounds.Right - wndBounds.Left - (clientRectangle.Right - clientRectangle.Left) - 7;

            var cPos = bounds;
            double factor = GameService.Graphics.UIScaleMultiplier;

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)((this.characterPotraitSize - 2) * factor), (int)((this.characterPotraitSize - 2) * factor)))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var x = (int)(bounds.X * factor);
                    var y = (int)(bounds.Y * factor);

                    g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + x + sideBarWidth, clientRectangle.Top + y + titleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(this.characterPotraitSize - 2, this.characterPotraitSize));
                }

                bitmap.Save(path + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
