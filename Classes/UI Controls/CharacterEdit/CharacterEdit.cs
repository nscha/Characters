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
        private readonly Checkbox show;
        private readonly ImageButton image;
        private readonly ImageButton delete;
        private readonly Label name;
        private readonly ImageSelector imagePanel;
        private readonly FlowPanel tagPanel;
        private readonly TextBox tagBox;
        private readonly ImageButton addTag;
        private readonly StandardButton captureImages;
        private readonly StandardButton openFolder;

        private readonly int width = 350;
        private readonly int expandedWidth = 725;
        private readonly bool initialized;

        private readonly List<Tag> tags = new List<Tag>();

        private double opacityTick = 0;
        private DateTime lastMouseOver = DateTime.Now;

        private Character_Model character;

        public CharacterEdit()
        {
            this.Width = this.width;
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

            this.image = new ImageButton()
            {
                Parent = this,
                Texture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                HoveredTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(358353),
                Location = new Point(5 + 2, 5 + 2),
                Size = new Point(64, 64),
            };
            this.image.Click += this.Image_Click;

            this.delete = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Delete_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Delete_Button_Hovered),
                Location = new Point(this.Right - 24 - 5, 5),
                Size = new Point(24, 24),
                BasicTooltipText = "Delete Character",
            };
            this.delete.Click += this.Delete_Click;

            this.name = new Label()
            {
                Text = "Character Name",
                Parent = this,
                TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255),
                Font = GameService.Content.DefaultFont16,
                AutoSizeWidth = true,
                Location = new Point(this.image.Right + 5 + 2, 5),
            };

            this.show = new Checkbox()
            {
                Parent = this,
                Location = new Point(this.image.Right + 5 + 2, this.name.Bottom + 5 + 2),
                Size = new Point(100, 21),
                Text = "Show in List",
            };
            this.show.Click += this.Show_Click;

            this.captureImages = new StandardButton()
            {
                Parent = this,
                Location = new Point(this.image.Right + 4, this.image.Bottom - 28),
                Size = new Point(130, 30),
                Text = "Capture Images",
                Icon = tM.GetIcon(Icons.Camera),
                ResizeIcon = true,
            };
            this.captureImages.Click += this.CaptureImages_Click;

            this.openFolder = new StandardButton()
            {
                Parent = this,
                Location = new Point(this.captureImages.Right + 4, this.image.Bottom - 28),
                Size = new Point(125, 30),
                Text = "Open Folder",
                Icon = tM.GetIcon(Icons.Folder),
                ResizeIcon = true,
            };
            this.openFolder.Click += this.OpenFolder_Click;

            this.addTag = new ImageButton()
            {
                Parent = this,
                Texture = tM.GetControlTexture(Controls.Plus_Button),
                HoveredTexture = tM.GetControlTexture(Controls.Plus_Button_Hovered),
                Location = new Point(this.Right - 24 - 5, this.image.Bottom + 5 + 2),
                Size = new Point(24, 24),
                BasicTooltipText = "add Tag",
            };
            this.addTag.Click += this.AddTag_Click;

            this.tagBox = new TextBox()
            {
                Parent = this,
                Location = new Point(5, this.image.Bottom + 5 + 2),
                Size = new Point(this.Width - 10 - 24 - 2, 24),
                PlaceholderText = "PvE, WvW, Main, ERP ...",
            };
            this.tagBox.EnterPressed += this.AddTag_Click;
            this.tagBox.TextChanged += this.SetInteracted;

            this.tagPanel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(5, this.tagBox.Bottom + 5),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
            };

            this.imagePanel = new ImageSelector()
            {
                Parent = this,
                Location = new Point(5, this.image.Bottom + 5),
                ControlPadding = new Vector2(3, 2),
                Visible = false,
                Width = this.expandedWidth - 10,
            };
            this.imagePanel.Shown += this.LoadImages;

            new Dummy()
            {
                Parent = this.imagePanel,
                Size = new Point(0, 64),
            };

            foreach (string t in Characters.ModuleInstance.Tags)
            {
                this.tags.Add(this.AddTag(t, true));
            }

            this.initialized = true;
        }

        public Character_Model Character
        {
            get => this.character;
            set
            {
                this.character = value;
                if (value != null)
                {
                    this.character.Updated += this.ApplyCharacter;
                    this.ApplyCharacter(null, null);
                }
            }
        }

        public Rectangle TextureRectangle { get; set; }

        public Point TextureOffset { get; set; }

        public AsyncTexture2D Background { get; set; }

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

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

            var b = this.image.LocalBounds.Add(new Rectangle(-2, -2, 4, 4));

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
                    this.Opacity = this.Opacity - 0.05F;
                    if (this.Opacity <= 0F)
                    {
                        this.Hide();
                    }
                }
            }
        }

        public void LoadImages(object sender, EventArgs eArgs)
        {
            var path = Characters.ModuleInstance.AccountImagesPath;
            var images = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);

            var mHeight = Characters.ModuleInstance.MainWindow.ContentRegion.Height - 65;
            this.imagePanel.Height = Math.Min(((int)Math.Ceiling(images.Count() / 7.0) * (96 + (int)this.imagePanel.ControlPadding.Y)) + (int)this.imagePanel.OuterControlPadding.Y, mHeight) + 5;

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
            {
                this.imagePanel.ClearChildren();

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
                        Parent = this.imagePanel,
                        Texture = noImgTexture,
                    };

                    noImg.Click += (s, e) =>
                    {
                        this.character.IconPath = null;
                        this.character.Icon = null;
                        this.ApplyCharacter(null, null);
                    };
                }

                foreach (string p in images)
                {
                    var texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                    var img = new Image()
                    {
                        Size = new Point(96, 96),
                        Parent = this.imagePanel,
                        Texture = texture,
                    };
                    img.Click += (s, e) =>
                    {
                        this.character.IconPath = p.Replace(Characters.ModuleInstance.BasePath, string.Empty);
                        this.character.Icon = texture;
                        this.ApplyCharacter(null, null);
                    };
                }
            });
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Width = this.width;

            this.SetInteracted(null, null);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);

            this.SetInteracted(null, null);
        }

        protected override void OnResized(ResizedEventArgs e)
        {
            base.OnResized(e);

            if (this.initialized)
            {
                this.delete.Location = new Point(e.CurrentSize.X - 24 - 5, 5);

                this.show.Visible = this.Width == this.width;
                this.tagBox.Visible = this.Width == this.width;
                this.tagPanel.Visible = this.Width == this.width;
                this.addTag.Visible = this.Width == this.width;
                this.imagePanel.Visible = this.Width == this.expandedWidth;
                this.captureImages.Visible = this.Width == this.expandedWidth;
                this.openFolder.Visible = this.Width == this.expandedWidth;
            }
        }

        private void Tag_Deleted(object sender, EventArgs e)
        {
            var tag = (Tag)sender;
            this.tags.Remove(tag);
            Characters.ModuleInstance.Tags.Remove(tag.Text);

            tag.Deleted -= this.Tag_Deleted;
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            foreach (Tag t in this.tags)
            {
                t.Active = this.character.Tags.Contains(t.Text);
            }

            this.name.Text = this.character.Name;
            this.image.Texture = this.character.Icon;
            this.show.Checked = this.character.Show;

            if (this.imagePanel.Visible)
            {
                var noImgTexture = Characters.ModuleInstance.Data.Professions[this.Character.Profession].IconBig;

                if (this.Character.Specialization != SpecializationType.None)
                {
                    noImgTexture = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].IconBig;
                }

                var noImg = (Image)this.imagePanel.Children[0];
                noImg.Texture = noImgTexture;
            }
        }

        private void Show_Click(object sender, MouseEventArgs e)
        {
            this.character.Show = this.show.Checked;
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

        private void CaptureImages_Click(object sender, MouseEventArgs e)
        {
            Characters.ModuleInstance.PotraitCapture.Visible = !Characters.ModuleInstance.PotraitCapture.Visible;
        }

        private void Image_Click(object sender, MouseEventArgs e)
        {
            this.Width = this.Width == this.width ? this.expandedWidth : this.width;
        }

        private void AddTag_Click(object sender, EventArgs e)
        {
            if (this.tagBox.Text != null && this.tagBox.Text.Length > 0 && !Characters.ModuleInstance.Tags.Contains(this.tagBox.Text))
            {
                Characters.ModuleInstance.Tags.Add(this.tagBox.Text);
                this.Character.Tags.Add(this.tagBox.Text);
                this.tags.Add(this.AddTag(this.tagBox.Text, true));

                this.tagBox.Text = null;
            }
        }

        private Tag AddTag(string txt, bool active = false)
        {
            var tag = new Tag()
            {
                Text = txt,
                Parent = this.tagPanel,
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
                this.character.Tags.Remove(tag.Text);
            }
            else
            {
                this.character.Tags.Add(tag.Text);
            }
        }
    }
}
