using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.Classes.UI_Controls
{

    public class BasicFrameContainer : Container
    {
        public Color FrameColor = Color.Honeydew;
        public AsyncTexture2D Background;
        public Rectangle TextureRectangle = Rectangle.Empty;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Background != null)
            {
                spriteBatch.DrawOnCtrl(this,
                        Background,
                        bounds,
                        TextureRectangle != Rectangle.Empty ? TextureRectangle : Background.Bounds,
                        Color.Black * 0.9f,
                        0f,
                        default);
            }

            var color = FrameColor;

            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 2, bounds.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Bottom - 1, bounds.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Left, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 2, bounds.Top, 2, bounds.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(bounds.Right - 1, bounds.Top, 1, bounds.Height), Rectangle.Empty, color * 0.6f);
        }
    }

    public class CharacterPotraitCapture : Container
    {
        DateTime _capturePotraits;
        int _characterPotraitSize = 130;
        int _gap = 13;

        Checkbox _windowedCheckbox;
        ImageButton _captureButton;
        ImageButton _addButton;
        ImageButton _removeButton;

        Label _disclaimer;
        BasicFrameContainer _disclaimerBackground;

        bool _dragging;
        Point _draggingStart;
        ImageButton _dragButton;

        TextBox _sizeBox;
        TextBox _gapBox;

        FlowPanel _portraitsPanel;
        List<BasicFrameContainer> _characterPotraits = new List<BasicFrameContainer>();

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
            Size = new Point(100, 100);
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            Location = new Point((res.X - Size.X) / 2, (res.Y - 125 - Size.Y));
            var tM = Characters.ModuleInstance.TextureManager;
            _addButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Plus_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Plus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 1, 0),
                BasicTooltipText = "Add Potrait Frame",
            };
            _addButton.Click += _addButton_Click;

            _removeButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Minus_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Minus_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 2, 0),
                BasicTooltipText = "Remove Potrait Frame",
            };
            _removeButton.Click += _removeButton_Click;

            _dragButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Drag_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Drag_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 0, 0),
                BasicTooltipText = "Drag over your character Potraits",
            };
            _dragButton.LeftMouseButtonPressed += _dragButton_LeftMouseButtonPressed;
            _dragButton.LeftMouseButtonReleased += _dragButton_LeftMouseButtonReleased;

            _captureButton = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Potrait_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Potrait_Button_Hovered),
                Size = new Point(32, 32),
                Location = new Point((32 + 5) * 3, 0),
                BasicTooltipText = "Capture Potraits",
            };
            _captureButton.Click += _captureButton_Click;

            _disclaimerBackground = new BasicFrameContainer()
            {
                Parent = this,
                Location = new Point((32 + 5) * 4, 0),
                FrameColor = Color.Black,// new Color(32, 32 , 32),
                Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003),
                TextureRectangle = new Rectangle(50, 50, 500, 500),
                WidthSizingMode = SizingMode.AutoSize,
                AutoSizePadding= new Point(15, 0),
                Height = 32,
            };

            _windowedCheckbox = new Checkbox()
            {
                Parent = _disclaimerBackground,
                Text = "Game is in Windowed Mode",
                Checked = Characters.ModuleInstance.Settings._WindowedMode.Value,
                Location = new Point(5, 0),
                Height = 32,
            };
            _windowedCheckbox.CheckedChanged += _windowedCheckbox_CheckedChanged;
            Characters.ModuleInstance.Settings._WindowedMode.SettingChanged += (s, e) => { _windowedCheckbox.Checked = Characters.ModuleInstance.Settings._WindowedMode.Value; };

            _disclaimer = new Label()
            {
                Parent = _disclaimerBackground,
                Location = new Point(_windowedCheckbox.Right + 5, 0),
                TextColor = ContentService.Colors.ColonialWhite,
                AutoSizeWidth = true,
                Height = 32,
                Font = GameService.Content.DefaultFont16,
                Text = "Best results with Interface set to 'Larger'!",
                Padding = new Thickness(0f, 0f),
            };
            _disclaimer.Resized += _disclaimer_Resized;
            //_disclaimerBackground.Size = (_disclaimer.Size).Add(new Point(_windowedCheckbox.Width + 15, 0));

            _sizeBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 35),
                Size = new Point(50, 25),
                Text = _characterPotraitSize.ToString(),
                BasicTooltipText = "Potrait Size",
            };
            _sizeBox.TextChanged += _sizeBox_TextChanged;
            _gapBox = new TextBox()
            {
                Parent = this,
                Location = new Point(0, 65),
                Size = new Point(50, 25),
                Text = _gap.ToString(),
                BasicTooltipText = "Potrait Gap",
            };
            _gapBox.TextChanged += _gapBox_TextChanged;

            _portraitsPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(55, 35),
                Size = new Point(110, 110),
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(_gap, 0),
            };

            AddPotrait();
            AddPotrait();
        }

        private void _windowedCheckbox_CheckedChanged(object sender, CheckChangedEvent e)
        {
            Characters.ModuleInstance.Settings._WindowedMode.Value = _windowedCheckbox.Checked;
        }

        private void _disclaimer_Resized(object sender, ResizedEventArgs e)
        {
            _disclaimerBackground.Size = _disclaimer.Size.Add(new Point(10, 0));
        }

        private void _captureButton_Click(object sender, MouseEventArgs e)
        {
            _capturePotraits = DateTime.Now;

            foreach (Control c in _portraitsPanel.Children)
            {
                c.Hide();
            }
        }

        private void _sizeBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (Int32.TryParse(_sizeBox.Text, out s))
            {
                _characterPotraitSize = s;
                var p = new Point(s, s);
                foreach (Control c in _portraitsPanel.Children)
                {
                    c.Size = p;
                }
            }
        }
        private void _gapBox_TextChanged(object sender, EventArgs e)
        {
            int s;
            if (Int32.TryParse(_gapBox.Text, out s))
            {
                _gap = s;
                _portraitsPanel.ControlPadding = new Vector2(s, 0);
            }
        }

        private void _removeButton_Click(object sender, MouseEventArgs e)
        {
            if (_portraitsPanel.Children.Count > 1) _portraitsPanel.Children.RemoveAt(_portraitsPanel.Children.Count - 1);
        }

        private void _addButton_Click(object sender, MouseEventArgs e)
        {
            AddPotrait();
        }
        private void AddPotrait()
        {
            new BasicFrameContainer()
            {
                Parent = _portraitsPanel,
                Size = new Point(_characterPotraitSize, _characterPotraitSize),
            };
        }
        private void _dragButton_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void _dragButton_LeftMouseButtonPressed(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _draggingStart = _dragging ? RelativeMousePosition : Point.Zero;
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

            _dragging = _dragging && MouseOver;

            if (_dragging)
            {
                Location = Input.Mouse.Position.Add(new Point(-_draggingStart.X, -_draggingStart.Y));
            }

            if (_capturePotraits > DateTime.MinValue && DateTime.Now.Subtract(_capturePotraits).TotalMilliseconds >= 5)
            {
                foreach (Control c in _portraitsPanel.Children)
                {
                    CapturePotrait(c.AbsoluteBounds);
                }

                foreach (Control c in _portraitsPanel.Children)
                {
                    c.Show();
                }

                _capturePotraits = DateTime.MinValue;
                Characters.ModuleInstance.MainWindow.CharacterEdit.LoadImages(null, null);
            }
        }

        void CapturePotrait(Rectangle bounds)
        {

            var path = Characters.ModuleInstance.AccountImagesPath;

            Regex regex = new Regex("Image.*[0-9].png");
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories).Where(path => regex.IsMatch(path)).ToList();

            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;

            var wndBounds = new RECT();
            var clientRectangle = new RECT();
            GetWindowRect(hWnd, ref wndBounds);
            GetClientRect(hWnd, out clientRectangle);

            var fullscreen = !Characters.ModuleInstance.Settings._WindowedMode.Value;

            var TitleBarHeight = fullscreen ? 0 : wndBounds.Bottom - wndBounds.Top - (clientRectangle.Bottom - clientRectangle.Top) - 6;
            var SideBarWidth = fullscreen ? 0 : wndBounds.Right - wndBounds.Left - (clientRectangle.Right - clientRectangle.Left) - 7;

            var cPos = bounds;
            double factor = GameService.Graphics.UIScaleMultiplier;

            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap((int)((_characterPotraitSize - 2) * factor), (int)((_characterPotraitSize - 2) * factor)))
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    var x = (int)(bounds.X * factor);
                    var y = (int)(bounds.Y * factor);

                    g.CopyFromScreen(new System.Drawing.Point(wndBounds.Left  + x + SideBarWidth, clientRectangle.Top + y + TitleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(_characterPotraitSize - 2, _characterPotraitSize));
                }

                bitmap.Save(path + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
