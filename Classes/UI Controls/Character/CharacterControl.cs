namespace Kenedia.Modules.Characters.Classes.MainWindow
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Blish_HUD;
    using Blish_HUD.Content;
    using Blish_HUD.Controls;
    using Blish_HUD.Input;
    using Kenedia.Modules.Characters.Classes.UI_Controls;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using MonoGame.Extended.BitmapFonts;
    using Color = Microsoft.Xna.Framework.Color;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class CharacterControl : Panel
    {
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

        private readonly List<Control> dataControls = new List<Control>();

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

        private Rectangle loginRect;
        private Rectangle iconRect;
        private Rectangle cogRect;

        public int TotalWidth
        {
            get
            {
                return this._IconRectangle.Width + this._ContentRectangle.Width;
            }
        }

        public CharacterControl()
        {
            this.HeightSizingMode = SizingMode.AutoSize;

            this.BackgroundColor = new Color(0, 0, 0, 75);
            this.AutoSizePadding = new Point(0, 2);

            this.contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                // WidthSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(0, 5),
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

            this.textTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };
            this.textTooltip.Shown += this.TextTooltip_Shown;

            this.characterTooltip = new CharacterTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };

            Characters.ModuleInstance.LanguageChanged += this.ApplyCharacter;
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
            if (this.tagPanel.Visible && this.Character.Tags.Count > 0)
            {
                this.UpdateSize();
            }
        }

        private void TextTooltip_Shown(object sender, EventArgs e)
        {
            this.characterTooltip?.Hide();
        }

        public double Index
        {
            get => this.Character != null ? this.Character.Index : 0;
            set
            {
                if (this.Character != null)
                {
                    this.Character.Index = (int)value;
                }
            }
        }

        private bool dragging;
        private Character_Model _character;

        public Character_Model Character
        {
            get => this._character;
            set
            {
                if (this._character != null)
                {
                    this._character.Updated -= this.ApplyCharacter;
                    this._character.Deleted -= this.CharacterDeleted;
                }

                this._character = value;
                this.characterTooltip.Character = value;

                if (value != null)
                {
                    this._character.Updated += this.ApplyCharacter;
                    this._character.Deleted += this.CharacterDeleted; ;
                    this.ApplyCharacter(null, null);
                }
            }
        }

        private void CharacterDeleted(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            this.nameLabel.Text = this.Character.Name;
            this.nameLabel.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            this.levelLabel.Text = "Level " + this.Character.Level.ToString();
            this.levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            this.levelLabel.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(157085);

            if (Enum.IsDefined(typeof(SpecializationType), this.Character.Specialization) && this.Character.Specialization != SpecializationType.None)
            {
                this.professionLabel.Icon = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].IconBig;
                this.professionLabel.Text = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].Name;
            }
            else
            {
                this.professionLabel.Icon = Characters.ModuleInstance.Data.Professions[this.Character.Profession].IconBig;
                this.professionLabel.Text = Characters.ModuleInstance.Data.Professions[this.Character.Profession].Name;
            }

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
            this.lastLoginLabel.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", 0, 0, 0, 0);
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
                    CanInteract = false,
                };
            }

            this.craftingControl.Character = this.Character;
            this.UpdateLabelLayout();
            this.UpdateSize();

            // UpdateLayout();
        }

        public Color HoverColor = Color.LightBlue;

        public BitmapFont NameFont = GameService.Content.DefaultFont14;
        public BitmapFont Font = GameService.Content.DefaultFont14;

        private readonly FlowPanel contentPanel;
        private readonly Dummy iconDummy;
        private Rectangle _IconRectangle;
        private Rectangle _ContentRectangle;

        public void UpdateLabelLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            this.iconDummy.Visible = this._IconRectangle != Rectangle.Empty;
            this.iconDummy.Size = this._IconRectangle.Size;
            this.iconDummy.Location = this._IconRectangle.Location;

            this.nameLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowName.Value;
            this.nameLabel.Font = this.NameFont;

            this.levelLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowLevel.Value;
            this.levelLabel.Font = this.Font;

            this.professionLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowProfession.Value;
            this.professionLabel.Font = this.Font;

            this.raceLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowRace.Value;
            this.raceLabel.Font = this.Font;

            this.mapLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowMap.Value;
            this.mapLabel.Font = this.Font;

            this.lastLoginLabel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowLastLogin.Value;
            this.lastLoginLabel.Font = this.Font;

            this.craftingControl.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowCrafting.Value;
            this.craftingControl.Font = this.Font;

            this.tagPanel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.ShowTags.Value && this.Character.Tags.Count > 0;
            foreach (Tag tag in this.tagPanel.Children)
            {
                tag.Font = this.Font;
            }

            this.craftingControl.Height = this.Font.LineHeight + 2;
        }

        public void UpdateLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;
            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                this._IconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(this.Width, this.Height), Math.Min(this.Width, this.Height)));
            }
            else
            {
                this._IconRectangle = Rectangle.Empty;
            }

            this.UpdateLabelLayout();
            this.UpdateSize();

            this.characterTooltip.NameFont = this.NameFont;
            this.characterTooltip.Font = this.Font;
            this.characterTooltip.UpdateLayout();

            this._ContentRectangle = onlyIcon ? Rectangle.Empty : new Rectangle(new Point(this._IconRectangle.Right, 0), this.contentPanel.Size);
            this.contentPanel.Location = this._ContentRectangle.Location;

            this.contentPanel.Visible = !onlyIcon;
        }

        public void UpdateSize()
        {
            var visibleControls = this.dataControls.Where(e => e.Visible);
            var amount = visibleControls.Count();

            var height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)this.contentPanel.ControlPadding.Y) : 0;
            var width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            this.contentPanel.Height = Math.Max(this.NameFont.LineHeight + 8, height);
            this.contentPanel.Width = width + (int)this.contentPanel.ControlPadding.X;
            this.tagPanel.Width = width;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (this.Character != null)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    if (this.Character.IconPath != null && this.Character.Icon != null)
                    {
                        spriteBatch.DrawOnCtrl(
                            this,
                            this.Character.Icon,
                            this._IconRectangle,
                            this.Character.Icon.Bounds,
                            Color.White,
                            0f,
                            default);
                    }
                    else
                    {
                        var texture = Characters.ModuleInstance.Data.Professions[this.Character.Profession].IconBig;

                        if (this.Character.Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), this.Character.Specialization))
                        {
                            texture = Characters.ModuleInstance.Data.Specializations[this.Character.Specialization].IconBig;
                        }

                        if (texture != null)
                        {
                            spriteBatch.DrawOnCtrl(
                                this,
                                this.iconFrame,
                                new Rectangle(this._IconRectangle.X, this._IconRectangle.Y, this._IconRectangle.Width, this._IconRectangle.Height),
                                this.iconFrame.Bounds,
                                Color.White,
                                0f,
                                default);

                            spriteBatch.DrawOnCtrl(
                                this,
                                this.iconFrame,
                                new Rectangle(this._IconRectangle.Width, this._IconRectangle.Height, this._IconRectangle.Width, this._IconRectangle.Height),
                                this.iconFrame.Bounds,
                                Color.White,
                                6.28f / 2,
                                default);

                            spriteBatch.DrawOnCtrl(
                                this,
                                texture,
                                new Rectangle(8, 8, this._IconRectangle.Width - 16, this._IconRectangle.Height - 16),
                                texture.Bounds,
                                Color.White,
                                0f,
                                default);
                        }
                    }
                }
                else if (this.MouseOver)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        this._IconRectangle,
                        Rectangle.Empty,
                        Color.Transparent,
                        0f,
                        default);
                }
            }
        }

        private void CalculateRectangles(Rectangle bounds)
        {
            var cogSize = Math.Min(25, Math.Max(this.Font.LineHeight - 4, this.Height / 5));

            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                this.iconRect = this._IconRectangle;
                this.cogRect = new Rectangle(this.Width - cogSize - 4, 4, cogSize, cogSize);

                var pad = this.iconRect.Width / 5;
                var size = Math.Min(this.iconRect.Width - (pad * 2), this.iconRect.Height - (pad * 2));
                this.loginRect = new Rectangle(pad, pad, size, size);
            }
            else
            {
                this.iconRect = this._IconRectangle;
                this.cogRect = new Rectangle(this.Width - cogSize - 4, 4, cogSize, cogSize);

                var textureSize = Math.Min(42, Math.Min(this.LocalBounds.Width - 4, this.LocalBounds.Height - 4));
                this.loginRect = new Rectangle((this.LocalBounds.Width - textureSize) / 2, (this.LocalBounds.Height - textureSize) / 2, textureSize, textureSize);
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            this.CalculateRectangles(bounds);

            if (this.MouseOver)
            {
                this.textTooltip.Visible = false;

                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        this.iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    this.textTooltip.Text = this.Character.HasBirthdayPresent ? String.Format("It was {0}'s birthday! They are now {1} years old!", this.Character.Name, this.Character.Age) : String.Format("Log in with '{0}'!", this.Character.Name);
                    this.textTooltip.Visible = this.loginRect.Contains(this.RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        this.Character.HasBirthdayPresent ? this.loginRect.Contains(this.RelativeMousePosition) ? this.presentTextureOpen : this.presentTexture : this.loginRect.Contains(this.RelativeMousePosition) ? this.loginTextureHovered : this.loginTexture,
                        this.loginRect,
                        this.loginTexture.Bounds,
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

                    var padX = Math.Max(2, (bounds.Width - (this.loginTexture.Bounds.Width * 2)) / 2);
                    var padY = Math.Max(2, (bounds.Height - (this.loginTexture.Bounds.Height * 2)) / 2);

                    var size = Math.Min(bounds.Width - (padX * 2), bounds.Height - (padY * 2));

                    this.textTooltip.Text = this.Character.HasBirthdayPresent ? String.Format("It was {0}'s birthday! They are now {1} years old!", this.Character.Name, this.Character.Age) : String.Format("Log in with '{0}'!", this.Character.Name);
                    this.textTooltip.Visible = this.loginRect.Contains(this.RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        this.Character.HasBirthdayPresent ? this.loginRect.Contains(this.RelativeMousePosition) ? this.presentTextureOpen : this.presentTexture : this.loginRect.Contains(this.RelativeMousePosition) ? this.loginTextureHovered : this.loginTexture,
                        this.loginRect,
                        this.loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    this.cogRect.Contains(this.RelativeMousePosition) ? this.cogTextureHovered : this.cogTexture,
                    this.cogRect,
                    new Rectangle(5, 5, 22, 22),
                    Color.White,
                    0f,
                    default);
                if (this.cogRect.Contains(this.RelativeMousePosition))
                {
                    this.textTooltip.Text = String.Format("Adjust {0} settings and tags!", this.Character.Name);
                    this.textTooltip.Visible = true;
                };

                var color = ContentService.Colors.ColonialWhite;

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

            if (!this.MouseOver && this.Character != null && this.Character.HasBirthdayPresent)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        this.iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        this.presentTexture,
                        this.loginRect,
                        this.presentTexture.Bounds,
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
                        this.presentTexture,
                        this.loginRect,
                        this.presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            if (e.IsDoubleClick && Characters.ModuleInstance.Settings.DoubleClickToEnter.Value)
            {
                Characters.ModuleInstance.SwapTo(this.Character);
                return;
            }

            // Logout Icon Clicked!
            if (this.loginRect.Contains(this.RelativeMousePosition))
            {
                Characters.ModuleInstance.SwapTo(this.Character);
            }

            // Cog Icon Clicked!
            if (this.cogRect.Contains(this.RelativeMousePosition))
            {
                Characters.ModuleInstance.MainWindow.CharacterEdit.Visible = !Characters.ModuleInstance.MainWindow.CharacterEdit.Visible || Characters.ModuleInstance.MainWindow.CharacterEdit.Character != this.Character;
                Characters.ModuleInstance.MainWindow.CharacterEdit.Character = this.Character;
            }
        }

        protected override void OnLeftMouseButtonPressed(MouseEventArgs e)
        {
            base.OnLeftMouseButtonPressed(e);
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = this;
                this.dragging = true;
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (this.dragging)
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = null;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            if (this.textTooltip == null || !this.textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value)
            {
                this.characterTooltip.Show();
            }
            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            if (this.textTooltip == null || !this.textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value)
            {
                this.characterTooltip.Show();
            }
            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (this.Character != null && this.lastLoginLabel.Visible && Characters.ModuleInstance.CurrentCharacterModel != this.Character)
            {
                var ts = DateTimeOffset.UtcNow.Subtract(this.Character.LastLogin);
                this.lastLoginLabel.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                if (this.Character.HasBirthdayPresent)
                {
                    // ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                }
            }

            if (!this.MouseOver && this.textTooltip.Visible)
            {
                this.textTooltip.Visible = this.MouseOver;
            }

            if (!this.MouseOver && this.characterTooltip.Visible)
            {
                this.characterTooltip.Visible = this.MouseOver;
            }
            // CharacterTooltip.Visible = MouseOver;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Characters.ModuleInstance.MainWindow.CharacterControls.Remove(this);
            this.dataControls?.DisposeAll();
            this.contentPanel?.Dispose();
            this.textTooltip?.Dispose();
            this.characterTooltip?.Dispose();
        }
    }
}
