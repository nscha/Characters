namespace Kenedia.Modules.Characters.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Kenedia.Modules.Characters.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using MonoGame.Extended.BitmapFonts;
    using static Kenedia.Modules.Characters.Services.SettingsModel;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class CharacterTooltip : Container
    {
        private readonly AsyncTexture2D iconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        private readonly FlowPanel contentPanel;
        private readonly Dummy iconDummy;

        private readonly IconLabel nameLabel;
        private readonly IconLabel levelLabel;
        private readonly IconLabel professionLabel;
        private readonly IconLabel raceLabel;
        private readonly IconLabel mapLabel;
        private readonly IconLabel lastLoginLabel;
        private readonly FlowPanel tagPanel;

        private readonly CraftingControl craftingControl;
        private readonly List<Control> dataControls;

        private Rectangle iconRectangle;
        private Rectangle contentRectangle;

        private Point textureOffset = new Point(25, 25);
        private Character_Model character;
        private BitmapFont font = GameService.Content.DefaultFont14;

        public CharacterTooltip()
        {
            this.HeightSizingMode = SizingMode.AutoSize;

            this.BackgroundColor = new Color(0, 0, 0, 75);
            this.AutoSizePadding = new Point(5, 5);

            this.contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(5, 5),
            };
            this.iconDummy = new Dummy()
            {
                Parent = this,
            };

            this.nameLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            this.levelLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            this.raceLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            this.professionLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            this.mapLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            this.craftingControl = new CraftingControl()
            {
                Parent = this.contentPanel,
                Width = this.contentPanel.Width,
                Height = 20,
                Character = this.Character,
            };

            this.lastLoginLabel = new IconLabel()
            {
                Parent = this.contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            this.tagPanel = new FlowPanel()
            {
                Parent = this.contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
                Height = this.Font.LineHeight + 5,
                Visible = false,
            };
            this.tagPanel.Resized += this.Tag_Panel_Resized;

            this.dataControls = new List<Control>()
            {
                this.nameLabel,
                this.levelLabel,
                this.raceLabel,
                this.professionLabel,
                this.mapLabel,
                this.lastLoginLabel,
                this.craftingControl,
                this.tagPanel,
            };
        }

        public Rectangle TextureRectangle { get; set; } = new Rectangle(40, 25, 250, 250);

        public AsyncTexture2D Background { get; set; } = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

        public BitmapFont Font
        {
            get => this.font;
            set
            {
                this.font = value;
            }
        }

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont18;

        public Character_Model Character
        {
            get => this.character; set
            {
                this.character = value;
                this.ApplyCharacter(null, null);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            this.Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

            if (this.Character != null && this.lastLoginLabel.Visible && Characters.ModuleInstance.CurrentCharacterModel != this.Character)
            {
                var ts = DateTimeOffset.UtcNow.Subtract(this.Character.LastLogin);
                this.lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public void UpdateLayout()
        {
            if (this.iconRectangle.IsEmpty)
            {
                this.iconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(this.Width, this.Height), Math.Min(this.Width, this.Height)));
            }

            this.UpdateLabelLayout();
            this.UpdateSize();

            this.contentRectangle = new Rectangle(new Point(this.iconRectangle.Right, 0), this.contentPanel.Size);
            this.contentPanel.Location = this.contentRectangle.Location;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.Background != null)
            {
                var rect = new Rectangle(this.textureOffset.X, this.textureOffset.Y, bounds.Width, bounds.Height);

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

            if (!this.Character.HasDefaultIcon && this.Character.Icon != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    this.Character.Icon,
                    this.iconRectangle,
                    this.Character.Icon.Bounds,
                    Color.White,
                    0f,
                    default);
            }
            else
            {
                var texture = this.Character.SpecializationIcon;

                if (texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        this.iconFrame,
                        new Rectangle(this.iconRectangle.X, this.iconRectangle.Y, this.iconRectangle.Width, this.iconRectangle.Height),
                        this.iconFrame.Bounds,
                        Color.White,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        this.iconFrame,
                        new Rectangle(this.iconRectangle.Width, this.iconRectangle.Height, this.iconRectangle.Width, this.iconRectangle.Height),
                        this.iconFrame.Bounds,
                        Color.White,
                        6.28f / 2,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        texture,
                        new Rectangle(8, 8, this.iconRectangle.Width - 16, this.iconRectangle.Height - 16),
                        texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public void UpdateLabelLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            this.iconDummy.Visible = this.iconRectangle != Rectangle.Empty;
            this.iconDummy.Size = this.iconRectangle.Size;
            this.iconDummy.Location = this.iconRectangle.Location;

            this.nameLabel.Visible = true;
            this.nameLabel.Font = this.NameFont;

            this.levelLabel.Visible = true;
            this.levelLabel.Font = this.Font;

            this.professionLabel.Visible = true;
            this.professionLabel.Font = this.Font;

            this.raceLabel.Visible = true;
            this.raceLabel.Font = this.Font;

            this.mapLabel.Visible = true;
            this.mapLabel.Font = this.Font;

            this.lastLoginLabel.Visible = true;
            this.lastLoginLabel.Font = this.Font;

            this.craftingControl.Visible = true;
            this.craftingControl.Font = this.Font;

            this.tagPanel.Visible = this.Character.Tags.Count > 0;
            foreach (Tag tag in this.tagPanel.Children)
            {
                tag.Font = this.Font;
            }

            this.craftingControl.Height = this.Font.LineHeight + 2;
        }

        public void UpdateSize()
        {
            var visibleControls = this.dataControls.Where(e => e.Visible);
            var amount = visibleControls.Count();

            var height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)this.contentPanel.ControlPadding.Y) : 0;
            var width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            this.contentPanel.Height = height;
            this.contentPanel.Width = width + (int)this.contentPanel.ControlPadding.X;
            this.tagPanel.Width = width;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            this.nameLabel.Text = this.Character.Name;
            this.nameLabel.TextColor = new Microsoft.Xna.Framework.Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            this.levelLabel.Text = string.Format(Strings.common.Level, this.Character.Level);
            this.levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            this.levelLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(157085);

            this.professionLabel.Icon = this.Character.SpecializationIcon;
            this.professionLabel.Text = this.Character.SpecializationName;

            if (this.professionLabel.Icon != null)
            {
                this.professionLabel.TextureRectangle = this.professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            this.raceLabel.Text = Characters.ModuleInstance.Data.Races[this.Character.Race].Name;
            this.raceLabel.Icon = Characters.ModuleInstance.Data.Races[this.Character.Race].Icon;

            this.mapLabel.Text = Characters.ModuleInstance.Data.GetMapById(this.Character.Map).Name;
            this.mapLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            this.mapLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(358406); // 358406 //517180 //157122;

            this.lastLoginLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(841721);
            this.lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, 0, 0, 0, 0);
            this.lastLoginLabel.TextureRectangle = Rectangle.Empty;

            this.tagPanel.ClearChildren();
            foreach (var tagText in this.Character.Tags)
            {
                new Tag()
                {
                    Parent = this.tagPanel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                };
            }

            this.craftingControl.Character = this.Character;
            this.UpdateLabelLayout();
            this.UpdateSize();

            // UpdateLayout();
        }
    }
}
