namespace Kenedia.Modules.Characters.Classes.Classes.UI_Controls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.Characters.Classes.UI_Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class BasicFrameContainer : Container
    {
        public Color FrameColor = Color.Honeydew;
        public AsyncTexture2D Background;
        public Rectangle TextureRectangle = Rectangle.Empty;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.Background != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this.Background,
                    bounds,
                    this.TextureRectangle != Rectangle.Empty ? this.TextureRectangle : this.Background.Bounds,
                    Color.Black * 0.9f,
                    0f,
                    default);
            }

            var color = this.FrameColor;

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }
    }

    public class CharacterPotraitCapture : Container
    {
        private DateTime _capturePotraits;
        private int _characterPotraitSize = 130;
        private int _gap = 13;
        private readonly Checkbox _windowedCheckbox;
        private readonly ImageButton _captureButton;
        private readonly ImageButton _addButton;
        private readonly ImageButton _removeButton;
        private readonly Label _disclaimer;
        private readonly BasicFrameContainer _disclaimerBackground;

        private bool _dragging;
        private Point _draggingStart;
        private readonly ImageButton _dragButton;
        private readonly TextBox _sizeBox;
        private readonly TextBox _gapBox;
        private readonly FlowPanel _portraitsPanel;
        private readonly List<BasicFrameContainer> _characterPotraits = new List<BasicFrameContainer>();

        /// <summary>
        /// 358353 Potrait
        /// 255297 Plus
        /// 255301 Minus
        /// 358376 Zoom In
        /// 358378 Zoom Out
        /// </summary>
        public CharacterPotraitCapture()
        {
            var res = GameService.Graphics.Resolution;
            this.Size = new Point(100, 100);
            this.WidthSizingMode = SizingMode.AutoSize;
            this.HeightSizingMode = SizingMode.AutoSize;

            this.Location = new Point((res.X - this.Size.X) / 2, res.Y - 125 - this.Size.Y);
            var tM = Characters.ModuleInstance.TextureManager;
            this._addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Plus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 1, 0),
                BasicTooltipText = "Add Potrait Frame",
            };
            this._addButton.Click += this.AddButton_Click;

            this._removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Minus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 2, 0),
                BasicTooltipText = "Remove Potrait Frame",
            };
            this._removeButton.Click += this.RemoveButton_Click;

            this._dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Drag_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                BasicTooltipText = "Drag over your character Potraits",
            };
            this._dragButton.LeftMouseButtonPressed += this.DragButton_LeftMouseButtonPressed;
            this._dragButton.LeftMouseButtonReleased += this.DragButton_LeftMouseButtonReleased;

            this._captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Potrait_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 3, 0),
                BasicTooltipText = "Capture Potraits",
            };
            this._captureButton.Click += this.CaptureButton_Click;

            this._disclaimerBackground = new BasicFrameContainer()
            {
                Parent = this,
                Location = new Point((32 + 5) * 4, 0),
                FrameColor = Color.Black,// new Color(32, 32 , 32),
                Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                AutoSizePadding = new Point(15, 0),
                Height = 32,
            };

            this._windowedCheckbox = new Checkbox()
            {
                Parent = this._disclaimerBackground,
                Text = "Game is in Windowed Mode",
                Checked = Characters.ModuleInstance.Settings.WindowedMode.Value,
                Location = new Point(5, 0),
                Height = 32,
            };
            this._windowedCheckbox.CheckedChanged += this.WindowedCheckbox_CheckedChanged;
            Characters.ModuleInstance.Settings.WindowedMode.SettingChanged += (s, e) => { this._windowedCheckbox.Checked = Characters.ModuleInstance.Settings.WindowedMode.Value; };

            this._disclaimer = new Label()
            {
                Parent = this._disclaimerBackground,
                Location = new Point(this._windowedCheckbox.Right + 5, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                Text = "Best results with Interface set to 'Larger'!",
                Padding = new Thickness(0f, 0f),
            };
            this._disclaimer.Resized += this.Disclaimer_Resized;
            // _disclaimerBackground.Size = (_disclaimer.Size).Add(new Point(_windowedCheckbox.Width + 15, 0));

            this._sizeBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 35),
                Size = new Point(50, 25),
                Text = this._characterPotraitSize.ToString(),
                BasicTooltipText = "Potrait Size",
            };
            this._sizeBox.TextChanged += this.SizeBox_TextChanged;
            this._gapBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 65),
                Size = new Point(50, 25),
                Text = this._gap.ToString(),
                BasicTooltipText = "Potrait Gap",
            };
            this._gapBox.TextChanged += this.GapBox_TextChanged;

            this._portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(55, 35),
                Size = new Point(110, 110),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(this._gap, 0),
            };

            this.AddPotrait();
            this.AddPotrait();
        }

        private void WindowedCheckbox_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Characters.ModuleInstance.Settings.WindowedMode.Value = this._windowedCheckbox.Checked;
        }

        private void Disclaimer_Resized(object sender, ResizedEventArgs e)
        {
            this._disclaimerBackground.Size = this._disclaimer.Size.Add(new Point(10, 0));
        }

        private void CaptureButton_Click(object sender, MouseEventArgs e)
        {
            this._capturePotraits = DateTime.Now;

            foreach (Control c in this._portraitsPanel.Children)
            {
                c.Hide();
            }
        }

        private void SizeBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (Int32.TryParse(this._sizeBox.Text, out s))
            {
                this._characterPotraitSize = s;
                var p = new Point(s, s);
                foreach (Control c in this._portraitsPanel.Children)
                {
                    c.Size = p;
                }
            }
        }

        private void GapBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (Int32.TryParse(this._gapBox.Text, out s))
            {
                this._gap = s;
                this._portraitsPanel.ControlPadding = new Vector2(s, 0);
            }
        }

        private void RemoveButton_Click(object sender, MouseEventArgs e)
        {
            if (this._portraitsPanel.Children.Count > 1)
            {
                this._portraitsPanel.Children.RemoveAt(this._portraitsPanel.Children.Count - 1);
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
                Parent = this._portraitsPanel,
                Size = new Point(this._characterPotraitSize, this._characterPotraitSize),
            };
        }

        private void DragButton_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            this._dragging = false;
        }

        private void DragButton_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            this._dragging = true;
            this._draggingStart = this._dragging ? this.RelativeMousePosition : Point.Zero;
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            this._dragging = this._dragging && this.MouseOver;

            if (this._dragging)
            {
                this.Location = Input.Mouse.Position.Add(new Point(-this._draggingStart.X, -this._draggingStart.Y));
            }

            if (this._capturePotraits > DateTime.MinValue && DateTime.Now.Subtract(this._capturePotraits).TotalMilliseconds >= 5)
            {
                foreach (Control c in this._portraitsPanel.Children)
                {
                    this.CapturePotrait(c.AbsoluteBounds);
                }

                foreach (Control c in this._portraitsPanel.Children)
                {
                    c.Show();
                }

                this._capturePotraits = DateTime.MinValue;
                Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
            }
        }

        private void CapturePotrait(Rectangle bounds)
        {

            var path = Characters.ModuleInstance.AccountImagesPath;

            Regex regex = new Regex("Image.*[0-9].png");
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            var wndBounds = new RECT();
            var clientRectangle = new RECT();
            GetWindowRect(hWnd, ref wndBounds);
            GetClientRect(hWnd, out clientRectangle);

            var fullscreen = !Characters.ModuleInstance.Settings.WindowedMode.Value;

            var titleBarHeight = fullscreen ? 0 : wndBounds.Bottom - wndBounds.Top - (clientRectangle.Bottom - clientRectangle.Top) - 6;
            var sideBarWidth = fullscreen ? 0 : wndBounds.Right - wndBounds.Left - (clientRectangle.Right - clientRectangle.Left) - 7;

            var cPos = bounds;
            double factor = GameService.Graphics.UIScaleMultiplier;

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)((this._characterPotraitSize - 2) * factor), (int)((this._characterPotraitSize - 2) * factor)))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var x = (int)(bounds.X * factor);
                    var y = (int)(bounds.Y * factor);

                    g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left + x + sideBarWidth, clientRectangle.Top + y + titleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(this._characterPotraitSize - 2, this._characterPotraitSize));
                }

                bitmap.Save(path + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
