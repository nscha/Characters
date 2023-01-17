using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    public class CharacterEdit : Container
    {
        double OpacityTick = 0;
        DateTime LastMouseOver = DateTime.Now;

        private Character_Model _character;
        public Character_Model Character
        {
            get => _character;
            set
            {
                _character = value;
                if (value != null)
                {
                    _character.Updated += ApplyCharacter;
                    ApplyCharacter(null, null);
                }
            }
        }

        public Rectangle TextureRectangle;
        public Point TextureOffset;
        public AsyncTexture2D Background;
        private AsyncTexture2D _iconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        public Color BackgroundTint = Color.Honeydew * 0.95f;

        private Checkbox _show;
        private ImageButton _image;
        private ImageButton _delete;
        private Label _name;
        private ImageSelector _imagePanel;
        private FlowPanel _tagPanel;
        private TextBox _tagBox;
        private ImageButton _addTag;
        private StandardButton _captureImages;
        private StandardButton _openFolder;

        private int _width = 350;
        private int _expandedWidth = 725;
        private bool _initialized;

        private List<Tag> _tags = new List<Tag>();

        public CharacterEdit()
        {
            Width = _width;
            new Dummy()
            {
                Width = 350,
            };
            Parent = GameService.Graphics.SpriteScreen;
            HeightSizingMode = SizingMode.AutoSize;
            AutoSizePadding = new Point(5, 5);
            ZIndex = 999;

            Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); //155985

            var tM = Characters.ModuleInstance.TextureManager;

            _image = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                Location = new Point(5 + 2, 5 + 2),
                Size = new Point(64, 64),
            };
            _image.Click += _image_Click;

            _delete = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Delete_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Delete_Button_Hovered),
                Location = new Point(Right - 24 - 5, 5),
                Size = new Point(24, 24),
                BasicTooltipText = "Delete Character",
            };
            _delete.Click += _delete_Click;

            _name = new Label()
            {
                Text = "Character Name",
                Parent = this,
                TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255),
                Font = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                Location = new Point(_image.Right + 5 + 2, 5),
            };

            _show = new Checkbox()
            {
                Parent = this,
                Location = new Point(_image.Right + 5 + 2, _name.Bottom + 5 + 2),
                Size = new Point(100, 21),
                Text = "Show in List",
            };
            _show.Click += _show_Click;

            _captureImages = new StandardButton()
            {
                Parent = this,
                Location = new Point(_image.Right + 4, _image.Bottom - 28),
                Size = new Point(130, 30),
                Text = "Capture Images",
                Icon = tM.getIcon(_Icons.Camera),
                ResizeIcon = true,
            };
            _captureImages.Click += _captureImages_Click;

            _openFolder = new StandardButton()
            {
                Parent = this,
                Location = new Point(_captureImages.Right + 4, _image.Bottom - 28),
                Size = new Point(125, 30),
                Text = "Open Folder",
                Icon = tM.getIcon(_Icons.Folder),
                ResizeIcon = true,
            };
            _openFolder.Click += _openFolder_Click; ;

            _addTag = new ImageButton()
            {
                Parent = this,
                Texture = tM.getControlTexture(_Controls.Plus_Button),
                HoveredTexture = tM.getControlTexture(_Controls.Plus_Button_Hovered),
                Location = new Point(Right - 24 - 5, _image.Bottom + 5 + 2),
                Size = new Point(24, 24),
                BasicTooltipText = "add Tag",
            };
            _addTag.Click += _addTag_Click;

            _tagBox = new TextBox()
            {
                Parent = this,
                Location = new Point(5, _image.Bottom + 5 + 2),
                Size = new Point(Width - 10 - 24 - 2, 24),
                PlaceholderText = "PvE, WvW, Main, ERP ...",
            };
            _tagBox.EnterPressed += _addTag_Click;
            _tagBox.TextChanged += SetInteracted; ;

            _tagPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(5, _tagBox.Bottom + 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2)
            };

            _imagePanel = new ImageSelector()
            {
                Parent = this,
                Location = new Point(5, _image.Bottom + 5),
                ControlPadding = new Vector2(3, 2),
                Visible = false,
                Width = _expandedWidth - 10,
            };
            _imagePanel.Shown += LoadImages;

            new Dummy()
            {
                Parent = _imagePanel,
                Size = new Point(0, 64),
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                _tags.Add(AddTag(t, true));
            }

            _initialized = true;
        }

        private void _show_Click(object sender, MouseEventArgs e)
        {
            _character.Show = _show.Checked;
        }

        private void SetInteracted(object sender, EventArgs e)
        {
            LastMouseOver = DateTime.Now;
            Opacity = 1f;
        }

        private void _openFolder_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            Process.Start(startInfo);
        }

        private void _delete_Click(object sender, MouseEventArgs e)
        {
            Character.Delete();
            Character = null;
            Hide();
        }

        public void LoadImages(object sender, EventArgs e)
        {
            var path = Characters.ModuleInstance.AccountImagesPath;
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

            var mHeight = Characters.ModuleInstance.MainWindow.ContentRegion.Height - 65;
            _imagePanel.Height = Math.Min((int)Math.Ceiling(images.Count() / 7.0) * (96 + (int)_imagePanel.ControlPadding.Y) + (int)_imagePanel.OuterControlPadding.Y, mHeight) + 5;

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                _imagePanel.ClearChildren();

                AsyncTexture2D noImgTexture = null;

                if (Visible && Character != null)
                {
                    noImgTexture = Characters.ModuleInstance.Data.Professions[Character.Profession].IconBig;

                    if (Character.Specialization != SpecializationType.None)
                    {
                        noImgTexture = Characters.ModuleInstance.Data.Specializations[Character.Specialization].TempIcon;
                    }

                    var noImg = new Image()
                    {
                        Size = new Point(96, 96),
                        Parent = _imagePanel,
                        Texture = noImgTexture,
                    };

                    noImg.Click += delegate
                    {
                        _character.IconPath = null;
                        _character.Icon = null;
                        ApplyCharacter(null, null);
                    };
                }

                foreach (string p in images)
                {
                    var texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                    var img = new Image()
                    {
                        Size = new Point(96, 96),
                        Parent = _imagePanel,
                        Texture = texture,
                    };
                    img.Click += delegate
                    {
                        _character.IconPath = p.Replace(Characters.ModuleInstance.BasePath, "");
                        _character.Icon = texture;
                        ApplyCharacter(null, null);
                    };
                }
            });

        }

        private void _captureImages_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.Visible = !Characters.ModuleInstance.PotraitCapture.Visible;
        }

        private void _image_Click(object sender, MouseEventArgs e)
        {
            Width = Width == _width ? _expandedWidth : _width;
        }

        private void _addTag_Click(object sender, EventArgs e)
        {
            if (_tagBox.Text != null && _tagBox.Text.Length > 0 && !Characters.ModuleInstance.Tags.Contains(_tagBox.Text))
            {
                Characters.ModuleInstance.Tags.Add(_tagBox.Text);
                Character.Tags.Add(_tagBox.Text);
                _tags.Add(AddTag(_tagBox.Text, true));

                _tagBox.Text = null;
            }
        }

        private Tag AddTag(string txt, bool active = false)
        {
            var tag = new Tag()
            {
                Text = txt,
                Parent = _tagPanel,
                Active = active,
            };

            tag.Deleted += Tag_Deleted;
            tag.Click += Tag_Click;

            return tag;
        }

        private void Tag_Click(object sender, MouseEventArgs e)
        {
            var tag = (Tag)sender;

            if (tag.Active)
            {
                _character.Tags.Remove(tag.Text);
            }
            else
            {
                _character.Tags.Add(tag.Text);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Width = _width;

            SetInteracted(null, null);
        }
        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            SetInteracted(null, null);
        }

        private void Tag_Deleted(object sender, EventArgs e)
        {
            var tag = (Tag)sender;
            _tags.Remove(tag);
            Characters.ModuleInstance.Tags.Remove(tag.Text);

            tag.Deleted -= Tag_Deleted;
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            foreach (Tag t in _tags)
            {
                t.Active = _character.Tags.Contains(t.Text);
            }

            _name.Text = _character.Name;
            _image.Texture = _character.Icon;
            _show.Checked = _character.Show;

            if (_imagePanel.Visible)
            {
                var noImgTexture = Characters.ModuleInstance.Data.Professions[Character.Profession].IconBig;

                if (Character.Specialization != SpecializationType.None)
                {
                    noImgTexture = Characters.ModuleInstance.Data.Specializations[Character.Specialization].TempIcon;
                }

                var noImg = (Image)_imagePanel.Children[0];
                noImg.Texture = noImgTexture;
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (_initialized)
            {
                _delete.Location = new Point(e.CurrentSize.X - 24 - 5, 5);

                _show.Visible = Width == _width;
                _tagBox.Visible = Width == _width;
                _tagPanel.Visible = Width == _width;
                _addTag.Visible = Width == _width;
                _imagePanel.Visible = Width == _expandedWidth;
                _captureImages.Visible = Width == _expandedWidth;
                _openFolder.Visible = Width == _expandedWidth;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Background != null)
            {
                //var rect = TextureRectangle != Rectangle.Empty ? TextureRectangle : new Rectangle(TextureOffset.X, TextureOffset.Y, Background.Bounds.Width, Background.Bounds.Height);
                //var rect = new Rectangle(TextureOffset.X, TextureOffset.Y, Background.Bounds.Width - (TextureOffset.X * 3), Background.Bounds.Height - (TextureOffset.Y * 3 ));
                var rect = new Rectangle(TextureOffset.X, TextureOffset.Y, bounds.Width, bounds.Height);

                spriteBatch.DrawOnCtrl(this,
                        Background,
                        bounds,
                        rect,
                        BackgroundTint,
                        0f,
                        default);
            }

            var color = Color.Black;

            var b = _image.LocalBounds.Add(new Rectangle(-2, -2, 4, 4));

            spriteBatch.DrawOnCtrl(this,
                                 ContentService.Textures.Pixel,
                                 b,
                                 Rectangle.Empty,
                                 Color.Black * 0.5f,
                                 0f,
                                 default);


            //Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width, 1), Rectangle.Empty, color * 0.6f);

            //Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 2, b.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 1, b.Width, 1), Rectangle.Empty, color * 0.6f);

            //Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, 2, b.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, 1, b.Height), Rectangle.Empty, color * 0.6f);

            //Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 2, b.Top, 2, b.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 1, b.Top, 1, b.Height), Rectangle.Empty, color * 0.6f);



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

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - OpacityTick > 50)
            {
                OpacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!MouseOver && DateTime.Now.Subtract(LastMouseOver).TotalMilliseconds >= 5000 && !Characters.ModuleInstance.PotraitCapture.Visible)
                {
                    Opacity = Opacity - (float)0.05;
                    if (Opacity <= (float)0) Hide();
                }
            }
        }
    }
}
