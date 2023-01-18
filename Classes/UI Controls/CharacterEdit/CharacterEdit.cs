namespace Kenedia.Modules.Characters.Classes.UI_Controls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class CharacterEdit : Container
    {
        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        private Character_Model _character;

        public Character_Model Character
        {
            get => this._character;
            set
            {
                this._character = value;
                if (value != null)
                {
                    this._character.Updated += this.ApplyCharacter;
                    this.ApplyCharacter(null, null);
                }
            }
        }

        public Rectangle TextureRectangle;
        public Point TextureOffset;
        public AsyncTexture2D Background;
        private readonly AsyncTexture2D _iconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        public Color BackgroundTint = Color.Honeydew * 0.95f;

        private readonly Checkbox _show;
        private readonly ImageButton _image;
        private readonly ImageButton _delete;
        private readonly Label _name;
        private readonly ImageSelector _imagePanel;
        private readonly FlowPanel _tagPanel;
        private readonly TextBox _tagBox;
        private readonly ImageButton _addTag;
        private readonly StandardButton _captureImages;
        private readonly StandardButton _openFolder;

        private readonly int _width = 350;
        private readonly int _expandedWidth = 725;
        private readonly bool _initialized;

        private readonly List<Tag> _tags = new List<Tag>();

        public CharacterEdit()
        {
            this.Width = this._width;
            new Dummy()
            {
                Width = 350,
            };
            this.Parent = GameService.Graphics.SpriteScreen;
            this.HeightSizingMode = SizingMode.AutoSize;
            this.AutoSizePadding = new Point(5, 5);
            this.ZIndex = 999;

            this.Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003); // 155985

            var tM = Characters.ModuleInstance.TextureManager;

            this._image = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                Location = new Point(5 + 2, 5 + 2),
                Size = new Point(64, 64),
            };
            this._image.Click += this.Image_Click;

            this._delete = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Delete_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Delete_Button_Hovered),
                Location = new Point(this.Right - 24 - 5, 5),
                Size = new Point(24, 24),
                BasicTooltipText = "Delete Character",
            };
            this._delete.Click += this.Delete_Click;

            this._name = new Label()
            {
                Text = "Character Name",
                Parent = this,
                TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255),
                Font = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                Location = new Point(this._image.Right + 5 + 2, 5),
            };

            this._show = new Checkbox()
            {
                Parent = this,
                Location = new Point(this._image.Right + 5 + 2, this._name.Bottom + 5 + 2),
                Size = new Point(100, 21),
                Text = "Show in List",
            };
            this._show.Click += this.Show_Click;

            this._captureImages = new StandardButton()
            {
                Parent = this,
                Location = new Point(this._image.Right + 4, this._image.Bottom - 28),
                Size = new Point(130, 30),
                Text = "Capture Images",
                Icon = tM.GetIcon(Icons.Camera),
                ResizeIcon = true,
            };
            this._captureImages.Click += this.CaptureImages_Click;

            this._openFolder = new StandardButton()
            {
                Parent = this,
                Location = new Point(this._captureImages.Right + 4, this._image.Bottom - 28),
                Size = new Point(125, 30),
                Text = "Open Folder",
                Icon = tM.GetIcon(Icons.Folder),
                ResizeIcon = true,
            };
            this._openFolder.Click += this.OpenFolder_Click; ;

            this._addTag = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Plus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Plus_Button_Hovered),
                Location = new Point(this.Right - 24 - 5, this._image.Bottom + 5 + 2),
                Size = new Point(24, 24),
                BasicTooltipText = "add Tag",
            };
            this._addTag.Click += this.AddTag_Click;

            this._tagBox = new TextBox()
            {
                Parent = this,
                Location = new Point(5, this._image.Bottom + 5 + 2),
                Size = new Point(this.Width - 10 - 24 - 2, 24),
                PlaceholderText = "PvE, WvW, Main, ERP ...",
            };
            this._tagBox.EnterPressed += this.AddTag_Click;
            this._tagBox.TextChanged += this.SetInteracted; ;

            this._tagPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(5, this._tagBox.Bottom + 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
            };

            this._imagePanel = new ImageSelector()
            {
                Parent = this,
                Location = new Point(5, this._image.Bottom + 5),
                ControlPadding = new Vector2(3, 2),
                Visible = false,
                Width = this._expandedWidth - 10,
            };
            this._imagePanel.Shown += this.LoadImages;

            new Dummy()
            {
                Parent = this._imagePanel,
                Size = new Point(0, 64),
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                this._tags.Add(this.AddTag(t, true));
            }

            this._initialized = true;
        }

        private void Show_Click(object sender, MouseEventArgs e)
        {
            this._character.Show = this._show.Checked;
        }

        private void SetInteracted(object sender, EventArgs e)
        {
            this.lastMouseOver = DateTime.Now;
            this.Opacity = 1f;
        }

        private void OpenFolder_Click(object sender, MouseEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Characters.ModuleInstance.AccountImagesPath,
                FileName = "explorer.exe",
            };

            Process.Start(startInfo);
        }

        private void Delete_Click(object sender, MouseEventArgs e)
        {
            this.Character.Delete();
            this.Character = null;
            this.Hide();
        }

        public void LoadImages(object sender, EventArgs e)
        {
            var path = Characters.ModuleInstance.AccountImagesPath;
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

            var mHeight = Characters.ModuleInstance.MainWindow.ContentRegion.Height - 65;
            this._imagePanel.Height = Math.Min(((int)Math.Ceiling(images.Count() / 7.0) * (96 + (int)this._imagePanel.ControlPadding.Y)) + (int)this._imagePanel.OuterControlPadding.Y, mHeight) + 5;

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                this._imagePanel.ClearChildren();

                AsyncTexture2D noImgTexture = null;

                if (this.Visible && this.Character != null)
                {
                    noImgTexture = Characters.ModuleInstance.Data.Professions[this.Character.Profession].IconBig;

                    if (this.Character.Specialization != SpecializationType.None)
                    {
                        noImgTexture = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].IconBig;
                    }

                    var noImg = new Image()
                    {
                        Size = new Point(96, 96),
                        Parent = this._imagePanel,
                        Texture = noImgTexture,
                    };

                    noImg.Click += delegate
                    {
                        this._character.IconPath = null;
                        this._character.Icon = null;
                        this.ApplyCharacter(null, null);
                    };
                }

                foreach (string p in images)
                {
                    var texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                    var img = new Image()
                    {
                        Size = new Point(96, 96),
                        Parent = this._imagePanel,
                        Texture = texture,
                    };
                    img.Click += delegate
                    {
                        this._character.IconPath = p.Replace(Characters.ModuleInstance.BasePath, "");
                        this._character.Icon = texture;
                        this.ApplyCharacter(null, null);
                    };
                }
            });
        }

        private void CaptureImages_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.Visible = !Characters.ModuleInstance.PotraitCapture.Visible;
        }

        private void Image_Click(object sender, MouseEventArgs e)
        {
            this.Width = this.Width == this._width ? this._expandedWidth : this._width;
        }

        private void AddTag_Click(object sender, EventArgs e)
        {
            if (this._tagBox.Text != null && this._tagBox.Text.Length > 0 && !Characters.ModuleInstance.Tags.Contains(this._tagBox.Text))
            {
                Characters.ModuleInstance.Tags.Add(this._tagBox.Text);
                this.Character.Tags.Add(this._tagBox.Text);
                this._tags.Add(this.AddTag(this._tagBox.Text, true));

                this._tagBox.Text = null;
            }
        }

        private Tag AddTag(string txt, bool active = false)
        {
            var tag = new Tag()
            {
                Text = txt,
                Parent = this._tagPanel,
                Active = active,
            };

            tag.Deleted += this.Tag_Deleted;
            tag.Click += this.Tag_Click;

            return tag;
        }

        private void Tag_Click(object sender, MouseEventArgs e)
        {
            var tag = (Tag)sender;

            if (tag.Active)
            {
                this._character.Tags.Remove(tag.Text);
            }
            else
            {
                this._character.Tags.Add(tag.Text);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Width = this._width;

            this.SetInteracted(null, null);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            this.SetInteracted(null, null);
        }

        private void Tag_Deleted(object sender, EventArgs e)
        {
            var tag = (Tag)sender;
            this._tags.Remove(tag);
            Characters.ModuleInstance.Tags.Remove(tag.Text);

            tag.Deleted -= this.Tag_Deleted;
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            foreach (Tag t in this._tags)
            {
                t.Active = this._character.Tags.Contains(t.Text);
            }

            this._name.Text = this._character.Name;
            this._image.Texture = this._character.Icon;
            this._show.Checked = this._character.Show;

            if (this._imagePanel.Visible)
            {
                var noImgTexture = Characters.ModuleInstance.Data.Professions[this.Character.Profession].IconBig;

                if (this.Character.Specialization != SpecializationType.None)
                {
                    noImgTexture = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].IconBig;
                }

                var noImg = (Image)this._imagePanel.Children[0];
                noImg.Texture = noImgTexture;
            }
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (this._initialized)
            {
                this._delete.Location = new Point(e.CurrentSize.X - 24 - 5, 5);

                this._show.Visible = this.Width == this._width;
                this._tagBox.Visible = this.Width == this._width;
                this._tagPanel.Visible = this.Width == this._width;
                this._addTag.Visible = this.Width == this._width;
                this._imagePanel.Visible = this.Width == this._expandedWidth;
                this._captureImages.Visible = this.Width == this._expandedWidth;
                this._openFolder.Visible = this.Width == this._expandedWidth;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.Background != null)
            {
                // var rect = TextureRectangle != Rectangle.Empty ? TextureRectangle : new Rectangle(TextureOffset.X, TextureOffset.Y, Background.Bounds.Width, Background.Bounds.Height);
                // var rect = new Rectangle(TextureOffset.X, TextureOffset.Y, Background.Bounds.Width - (TextureOffset.X * 3), Background.Bounds.Height - (TextureOffset.Y * 3 ));
                var rect = new Rectangle(this.TextureOffset.X, this.TextureOffset.Y, bounds.Width, bounds.Height);

                spriteBatch.DrawOnCtrl(
                    this,
                    this.Background,
                    bounds,
                    rect,
                    this.BackgroundTint,
                    0f,
                    default);
            }

            var color = Color.Black;

            var b = this._image.LocalBounds.Add(new Rectangle(-2, -2, 4, 4));

            spriteBatch.DrawOnCtrl(
                this,
                ContentService.Textures.Pixel,
                b,
                Rectangle.Empty,
                Color.Black * 0.5f,
                0f,
                default);


            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, b.Width, 1), Rectangle.Empty, color * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 2, b.Width, 2), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Bottom - 1, b.Width, 1), Rectangle.Empty, color * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, 2, b.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Left, b.Top, 1, b.Height), Rectangle.Empty, color * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 2, b.Top, 2, b.Height), Rectangle.Empty, color * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(b.Right - 1, b.Top, 1, b.Height), Rectangle.Empty, color * 0.6f);



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

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (gameTime.TotalGameTime.TotalMilliseconds - this.opacityTick > 50)
            {
                this.opacityTick = gameTime.TotalGameTime.TotalMilliseconds;

                if (!this.MouseOver && DateTime.Now.Subtract(this.lastMouseOver).TotalMilliseconds >= 5000 && !Characters.ModuleInstance.PotraitCapture.Visible)
                {
                    this.Opacity = this.Opacity - (float)0.05;
                    if (this.Opacity <= (float)0)
                    {
                        this.Hide();
                    }
                }
            }
        }
    }
}
