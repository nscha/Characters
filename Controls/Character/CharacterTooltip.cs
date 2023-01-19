using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
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

        private Point textureOffset = new(25, 25);
        private Character_Model character;
        private BitmapFont font = GameService.Content.DefaultFont14;

        public CharacterTooltip()
        {
            HeightSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(5, 5);

            contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(5, 5),
            };
            iconDummy = new Dummy()
            {
                Parent = this,
            };

            nameLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            levelLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            raceLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            professionLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            mapLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            craftingControl = new CraftingControl()
            {
                Parent = contentPanel,
                Width = contentPanel.Width,
                Height = 20,
                Character = Character,
            };

            lastLoginLabel = new IconLabel()
            {
                Parent = contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            tagPanel = new FlowPanel()
            {
                Parent = contentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
                Height = Font.LineHeight + 5,
                Visible = false,
            };
            tagPanel.Resized += Tag_Panel_Resized;

            dataControls = new List<Control>()
            {
                nameLabel,
                levelLabel,
                raceLabel,
                professionLabel,
                mapLabel,
                lastLoginLabel,
                craftingControl,
                tagPanel,
            };
        }

        public Rectangle TextureRectangle { get; set; } = new Rectangle(40, 25, 250, 250);

        public AsyncTexture2D Background { get; set; } = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);

        public Color BackgroundTint { get; set; } = Color.Honeydew * 0.95f;

        public BitmapFont Font
        {
            get => font;
            set
            {
                font = value;
            }
        }

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont18;

        public Character_Model Character
        {
            get => character; set
            {
                character = value;
                ApplyCharacter(null, null);
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

            if (Character != null && lastLoginLabel.Visible && Characters.ModuleInstance.CurrentCharacterModel != Character)
            {
                TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public void UpdateLayout()
        {
            if (iconRectangle.IsEmpty)
            {
                iconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)));
            }

            UpdateLabelLayout();
            UpdateSize();

            contentRectangle = new Rectangle(new Point(iconRectangle.Right, 0), contentPanel.Size);
            contentPanel.Location = contentRectangle.Location;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Background != null)
            {
                Rectangle rect = new(textureOffset.X, textureOffset.Y, bounds.Width, bounds.Height);

                spriteBatch.DrawOnCtrl(
                    this,
                    Background,
                    bounds,
                    rect,
                    BackgroundTint,
                    0f,
                    default);
            }

            Color color = Color.Black;

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

            if (!Character.HasDefaultIcon && Character.Icon != null)
            {
                spriteBatch.DrawOnCtrl(
                    this,
                    Character.Icon,
                    iconRectangle,
                    Character.Icon.Bounds,
                    Color.White,
                    0f,
                    default);
            }
            else
            {
                AsyncTexture2D texture = Character.SpecializationIcon;

                if (texture != null)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        iconFrame,
                        new Rectangle(iconRectangle.X, iconRectangle.Y, iconRectangle.Width, iconRectangle.Height),
                        iconFrame.Bounds,
                        Color.White,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        iconFrame,
                        new Rectangle(iconRectangle.Width, iconRectangle.Height, iconRectangle.Width, iconRectangle.Height),
                        iconFrame.Bounds,
                        Color.White,
                        6.28f / 2,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        texture,
                        new Rectangle(8, 8, iconRectangle.Width - 16, iconRectangle.Height - 16),
                        texture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public void UpdateLabelLayout()
        {
            bool onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            iconDummy.Visible = iconRectangle != Rectangle.Empty;
            iconDummy.Size = iconRectangle.Size;
            iconDummy.Location = iconRectangle.Location;

            nameLabel.Visible = true;
            nameLabel.Font = NameFont;

            levelLabel.Visible = true;
            levelLabel.Font = Font;

            professionLabel.Visible = true;
            professionLabel.Font = Font;

            raceLabel.Visible = true;
            raceLabel.Font = Font;

            mapLabel.Visible = true;
            mapLabel.Font = Font;

            lastLoginLabel.Visible = true;
            lastLoginLabel.Font = Font;

            craftingControl.Visible = true;
            craftingControl.Font = Font;

            tagPanel.Visible = Character.Tags.Count > 0;
            foreach (Tag tag in tagPanel.Children)
            {
                tag.Font = Font;
            }

            craftingControl.Height = Font.LineHeight + 2;
        }

        public void UpdateSize()
        {
            IEnumerable<Control> visibleControls = dataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)contentPanel.ControlPadding.Y) : 0;
            int width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            contentPanel.Height = height;
            contentPanel.Width = width + (int)contentPanel.ControlPadding.X;
            tagPanel.Width = width;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            nameLabel.Text = Character.Name;
            nameLabel.TextColor = new Microsoft.Xna.Framework.Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            levelLabel.Text = string.Format(Strings.common.Level, Character.Level);
            levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            levelLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(157085);

            professionLabel.Icon = Character.SpecializationIcon;
            professionLabel.Text = Character.SpecializationName;

            if (professionLabel.Icon != null)
            {
                professionLabel.TextureRectangle = professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            raceLabel.Text = Characters.ModuleInstance.Data.Races[Character.Race].Name;
            raceLabel.Icon = Characters.ModuleInstance.Data.Races[Character.Race].Icon;

            mapLabel.Text = Characters.ModuleInstance.Data.GetMapById(Character.Map).Name;
            mapLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            mapLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(358406); // 358406 //517180 //157122;

            lastLoginLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(841721);
            lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, 0, 0, 0, 0);
            lastLoginLabel.TextureRectangle = Rectangle.Empty;

            tagPanel.ClearChildren();
            foreach (string tagText in Character.Tags)
            {
                _ = new Tag()
                {
                    Parent = tagPanel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                };
            }

            craftingControl.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            // UpdateLayout();
        }
    }
}
