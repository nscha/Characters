using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
    public class CharacterControl : Panel
    {
        private readonly List<Control> dataControls = new();

        private readonly AsyncTexture2D iconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        private readonly AsyncTexture2D loginTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157092);
        private readonly AsyncTexture2D loginTextureHovered = GameService.Content.DatAssetCache.GetTextureFromAssetId(157094);
        private readonly AsyncTexture2D cogTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157109);
        private readonly AsyncTexture2D cogTextureHovered = GameService.Content.DatAssetCache.GetTextureFromAssetId(157111);
        private readonly AsyncTexture2D presentTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(593864);
        private readonly AsyncTexture2D presentTextureOpen = GameService.Content.DatAssetCache.GetTextureFromAssetId(593865);

        private readonly IconLabel nameLabel;
        private readonly IconLabel levelLabel;
        private readonly IconLabel professionLabel;
        private readonly IconLabel raceLabel;
        private readonly IconLabel mapLabel;
        private readonly IconLabel lastLoginLabel;
        private readonly FlowPanel tagPanel;

        private readonly CraftingControl craftingControl;
        private readonly BasicTooltip textTooltip;
        private readonly CharacterTooltip characterTooltip;
        private readonly FlowPanel contentPanel;
        private readonly Dummy iconDummy;

        private Rectangle loginRect;
        private Rectangle iconRect;
        private Rectangle cogRect;

        private Rectangle iconRectangle;
        private Rectangle contentRectangle;
        private bool dragging;
        private Character_Model character;

        public CharacterControl()
        {
            HeightSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(0, 2);

            contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,

                // WidthSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(0, 5),
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

            textTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };
            textTooltip.Shown += TextTooltip_Shown;

            characterTooltip = new CharacterTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };

            Characters.ModuleInstance.LanguageChanged += ApplyCharacter;
        }

        private enum InfoControls
        {
            Name,
            Level,
            Race,
            Profession,
            LastLogin,
            Map,
            Crafting,
        }

        public int TotalWidth
        {
            get
            {
                return iconRectangle.Width + contentRectangle.Width;
            }
        }

        public Color HoverColor { get; set; } = Color.LightBlue;

        public BitmapFont NameFont { get; set; } = GameService.Content.DefaultFont14;

        public BitmapFont Font { get; set; } = GameService.Content.DefaultFont14;

        public double Index
        {
            get => Character != null ? Character.Index : 0;
            set
            {
                if (Character != null)
                {
                    Character.Index = (int)value;
                }
            }
        }

        public Character_Model Character
        {
            get => character;
            set
            {
                if (character != null)
                {
                    character.Updated -= ApplyCharacter;
                    character.Deleted -= CharacterDeleted;
                }

                character = value;
                characterTooltip.Character = value;

                if (value != null)
                {
                    character.Updated += ApplyCharacter;
                    character.Deleted += CharacterDeleted;
                    ApplyCharacter(null, null);
                }
            }
        }

        public void UpdateLabelLayout()
        {
            bool onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            iconDummy.Visible = iconRectangle != Rectangle.Empty;
            iconDummy.Size = iconRectangle.Size;
            iconDummy.Location = iconRectangle.Location;

            nameLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowName.Value;
            nameLabel.Font = NameFont;

            levelLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowLevel.Value;
            levelLabel.Font = Font;

            professionLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowProfession.Value;
            professionLabel.Font = Font;

            raceLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowRace.Value;
            raceLabel.Font = Font;

            mapLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowMap.Value;
            mapLabel.Font = Font;

            lastLoginLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowLastLogin.Value;
            lastLoginLabel.Font = Font;

            craftingControl.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowCrafting.Value;
            craftingControl.Font = Font;

            tagPanel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowTags.Value && Character.Tags.Count > 0;
            foreach (Tag tag in tagPanel.Children)
            {
                tag.Font = Font;
            }

            craftingControl.Height = Font.LineHeight + 2;
        }

        public void UpdateLayout()
        {
            bool onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;
            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                iconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)));
            }
            else
            {
                iconRectangle = Rectangle.Empty;
            }

            UpdateLabelLayout();
            UpdateSize();

            characterTooltip.NameFont = NameFont;
            characterTooltip.Font = Font;
            characterTooltip.UpdateLayout();

            contentRectangle = onlyIcon ? Rectangle.Empty : new Rectangle(new Point(iconRectangle.Right, 0), contentPanel.Size);
            contentPanel.Location = contentRectangle.Location;

            contentPanel.Visible = !onlyIcon;
        }

        public void UpdateSize()
        {
            IEnumerable<Control> visibleControls = dataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)contentPanel.ControlPadding.Y) : 0;
            int width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            contentPanel.Height = Math.Max(NameFont.LineHeight + 8, height);
            contentPanel.Width = width + (int)contentPanel.ControlPadding.X;
            tagPanel.Width = width;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Character != null)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
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
                else if (MouseOver)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        iconRectangle,
                        Rectangle.Empty,
                        Color.Transparent,
                        0f,
                        default);
                }
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            CalculateRectangles(bounds);

            if (MouseOver)
            {
                textTooltip.Visible = false;

                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    textTooltip.Text = Character.HasBirthdayPresent ? string.Format(Strings.common.Birthday_Text, Character.Name, Character.Age) : string.Format(Strings.common.LoginWith, Character.Name);
                    textTooltip.Visible = loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? loginRect.Contains(RelativeMousePosition) ? presentTextureOpen : presentTexture : loginRect.Contains(RelativeMousePosition) ? loginTextureHovered : loginTexture,
                        loginRect,
                        loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        bounds,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    int padX = Math.Max(2, (bounds.Width - (loginTexture.Bounds.Width * 2)) / 2);
                    int padY = Math.Max(2, (bounds.Height - (loginTexture.Bounds.Height * 2)) / 2);

                    int size = Math.Min(bounds.Width - (padX * 2), bounds.Height - (padY * 2));

                    textTooltip.Text = Character.HasBirthdayPresent ? string.Format(Strings.common.Birthday_Text, Character.Name, Character.Age) : string.Format(Strings.common.LoginWith, Character.Name);
                    textTooltip.Visible = loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? loginRect.Contains(RelativeMousePosition) ? presentTextureOpen : presentTexture : loginRect.Contains(RelativeMousePosition) ? loginTextureHovered : loginTexture,
                        loginRect,
                        loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    cogRect.Contains(RelativeMousePosition) ? cogTextureHovered : cogTexture,
                    cogRect,
                    new Rectangle(5, 5, 22, 22),
                    Color.White,
                    0f,
                    default);
                if (cogRect.Contains(RelativeMousePosition))
                {
                    textTooltip.Text = string.Format(Strings.common.AdjustSettings, Character.Name);
                    textTooltip.Visible = true;
                }

                Color color = ContentService.Colors.ColonialWhite;

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

            if (!MouseOver && Character != null && Character.HasBirthdayPresent)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        presentTexture,
                        loginRect,
                        presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        bounds,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        presentTexture,
                        loginRect,
                        presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Character != null && lastLoginLabel.Visible && Characters.ModuleInstance.CurrentCharacterModel != Character)
            {
                TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                if (Character.HasBirthdayPresent)
                {
                    // ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                }
            }

            if (!MouseOver && textTooltip.Visible)
            {
                textTooltip.Visible = MouseOver;
            }

            if (!MouseOver && characterTooltip.Visible)
            {
                characterTooltip.Visible = MouseOver;
            }

            // CharacterTooltip.Visible = MouseOver;
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (e.IsDoubleClick && Characters.ModuleInstance.Settings.DoubleClickToEnter.Value)
            {
                Characters.ModuleInstance.SwapTo(Character);
                return;
            }

            // Logout Icon Clicked!
            if (loginRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.SwapTo(Character);
            }

            // Cog Icon Clicked!
            if (cogRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.MainWindow.CharacterEdit.Visible = !Characters.ModuleInstance.MainWindow.CharacterEdit.Visible || Characters.ModuleInstance.MainWindow.CharacterEdit.Character != Character;
                Characters.ModuleInstance.MainWindow.CharacterEdit.Character = Character;
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = this;
                dragging = true;
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (dragging)
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = null;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            if (textTooltip == null || (!textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                characterTooltip.Show();
            }

            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            if (textTooltip == null || (!textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                characterTooltip.Show();
            }

            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _ = Characters.ModuleInstance.MainWindow.CharacterControls.Remove(this);
            dataControls?.DisposeAll();
            contentPanel?.Dispose();
            textTooltip?.Dispose();
            characterTooltip?.Dispose();
        }

        private void CalculateRectangles(Rectangle bounds)
        {
            // Math.Min(25, Math.Max(this.Font.LineHeight - 4, this.Height / 5));
            int cogSize = Math.Min(25, Font.LineHeight);

            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                iconRect = iconRectangle;
                cogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                int pad = iconRect.Width / 5;
                int size = Math.Min(iconRect.Width - (pad * 2), iconRect.Height - (pad * 2));
                loginRect = new Rectangle(pad, pad, size, size);
            }
            else
            {
                iconRect = iconRectangle;
                cogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                int textureSize = Math.Min(42, Math.Min(LocalBounds.Width - 4, LocalBounds.Height - 4));
                loginRect = new Rectangle((LocalBounds.Width - textureSize) / 2, (LocalBounds.Height - textureSize) / 2, textureSize, textureSize);
            }
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
            if (tagPanel.Visible && Character.Tags.Count > 0)
            {
                UpdateSize();
            }
        }

        private void TextTooltip_Shown(object sender, EventArgs e) => characterTooltip?.Hide();

        private void CharacterDeleted(object sender, EventArgs e) => Dispose();

        private void ApplyCharacter(object sender, EventArgs e)
        {
            nameLabel.Text = Character.Name;
            nameLabel.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

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
                    CanInteract = false,
                };
            }

            craftingControl.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            // UpdateLayout();
        }
    }
}
