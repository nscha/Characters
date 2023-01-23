using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Kenedia.Modules.Characters.Extensions;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Kenedia.Modules.Characters.Services.SettingsModel;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Controls
{
    public class CharacterControl : Panel
    {
        private readonly List<Control> _dataControls = new();

        private readonly AsyncTexture2D _iconFrame = AsyncTexture2D.FromAssetId(1414041);
        private readonly AsyncTexture2D _loginTexture = AsyncTexture2D.FromAssetId(157092);
        private readonly AsyncTexture2D _loginTextureHovered = AsyncTexture2D.FromAssetId(157094);
        private readonly AsyncTexture2D _cogTexture = AsyncTexture2D.FromAssetId(157109);
        private readonly AsyncTexture2D _cogTextureHovered = AsyncTexture2D.FromAssetId(157111);
        private readonly AsyncTexture2D _presentTexture = AsyncTexture2D.FromAssetId(593864);
        private readonly AsyncTexture2D _presentTextureOpen = AsyncTexture2D.FromAssetId(593865);

        private readonly IconLabel _nameLabel;
        private readonly IconLabel _levelLabel;
        private readonly IconLabel _professionLabel;
        private readonly IconLabel _raceLabel;
        private readonly IconLabel _mapLabel;
        private readonly IconLabel _lastLoginLabel;
        private readonly FlowPanel _tagPanel;

        private readonly CraftingControl _craftingControl;
        private readonly BasicTooltip _textTooltip;
        private readonly CharacterTooltip _characterTooltip;
        private readonly FlowPanel _contentPanel;
        private readonly Dummy _iconDummy;

        private Rectangle _loginRect;
        private Rectangle _iconRect;
        private Rectangle _cogRect;

        private Rectangle _iconRectangle;
        private Rectangle _contentRectangle;
        private bool _dragging;
        private Character_Model _character;

        public CharacterControl()
        {
            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(0, 2);

            _contentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,

                // WidthSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(0, 5),
            };
            _iconDummy = new Dummy()
            {
                Parent = this,
            };

            _nameLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _levelLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _raceLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _professionLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            _mapLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _craftingControl = new CraftingControl()
            {
                Parent = _contentPanel,
                Width = _contentPanel.Width,
                Height = 20,
                Character = Character,
            };

            _lastLoginLabel = new IconLabel()
            {
                Parent = _contentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            _tagPanel = new FlowPanel()
            {
                Parent = _contentPanel,
                FlowDirection = ControlFlowDirection.LeftToRight,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
                Height = Font.LineHeight + 5,
                Visible = false,
            };
            _tagPanel.Resized += Tag_Panel_Resized;

            _dataControls = new List<Control>()
            {
                _nameLabel,
                _levelLabel,
                _raceLabel,
                _professionLabel,
                _mapLabel,
                _lastLoginLabel,
                _craftingControl,
                _tagPanel,
            };

            _textTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };
            _textTooltip.Shown += TextTooltip_Shown;

            _characterTooltip = new CharacterTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };

            Characters.ModuleInstance.LanguageChanged += ApplyCharacter;
            Characters.ModuleInstance.MainWindow.Hidden += MainWindow_Hidden;
            Characters.ModuleInstance.Settings.DisplayToggles.SettingChanged += DisplayToggles_SettingChanged;
        }

        private void DisplayToggles_SettingChanged(object sender, ValueChangedEventArgs<Dictionary<string, ShowCheckPair>> e)
        {
            UpdateLabelLayout();
            UpdateSize();
            Invalidate();
        }

        private void MainWindow_Hidden(object sender, EventArgs e)
        {
            _textTooltip.Hide();
            _characterTooltip.Hide();
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

        public int TotalWidth => _iconRectangle.Width + _contentRectangle.Width;

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
            get => _character;
            set
            {
                if (_character != null)
                {
                    _character.Updated -= ApplyCharacter;
                    _character.Deleted -= CharacterDeleted;
                }

                _character = value;
                _characterTooltip.Character = value;

                if (value != null)
                {
                    _character.Updated += ApplyCharacter;
                    _character.Deleted += CharacterDeleted;
                    ApplyCharacter(null, null);
                }
            }
        }

        private SettingsModel Settings => Characters.ModuleInstance.Settings;

        public void UpdateLabelLayout()
        {
            bool onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            //_iconDummy.Visible = _iconRectangle != Rectangle.Empty;
            _iconDummy.Size = _iconRectangle.Size;
            _iconDummy.Location = _iconRectangle.Location;

            _nameLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Name"].Show;
            _nameLabel.Font = NameFont;

            _levelLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Level"].Show;
            _levelLabel.Font = Font;

            _raceLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Race"].Show;
            _raceLabel.Font = Font;

            _professionLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Profession"].Show;
            _professionLabel.Font = Font;

            _lastLoginLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["LastLogin"].Show;
            _lastLoginLabel.Font = Font;

            _mapLabel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Map"].Show;
            _mapLabel.Font = Font;

            _craftingControl.Visible = !onlyIcon && Settings.DisplayToggles.Value["CraftingProfession"].Show;
            _craftingControl.Font = Font;

            _tagPanel.Visible = !onlyIcon && Settings.DisplayToggles.Value["Tags"].Show && Character.Tags.Count > 0;
            foreach (Tag tag in _tagPanel.Children)
            {
                tag.Font = Font;
            }

            _craftingControl.Height = Font.LineHeight + 2;
        }

        public void UpdateLayout()
        {
            bool onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;
            _iconRectangle = Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText
                ? new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)))
                : Rectangle.Empty;

            UpdateLabelLayout();
            UpdateSize();

            _characterTooltip.NameFont = NameFont;
            _characterTooltip.Font = Font;
            _characterTooltip.UpdateLayout();

            _contentRectangle = onlyIcon ? Rectangle.Empty : new Rectangle(new Point(_iconRectangle.Right, 0), _contentPanel.Size);
            _contentPanel.Location = _contentRectangle.Location;

            _contentPanel.Visible = !onlyIcon;
        }

        public void UpdateSize()
        {
            IEnumerable<Control> visibleControls = _dataControls.Where(e => e.Visible);
            int amount = visibleControls.Count();

            int height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)_contentPanel.ControlPadding.Y) : 0;
            int width =  Math.Max(_tagPanel.Children.Count > 0 ? _tagPanel.Children.Max(ctrl => ctrl.Width) : 0, visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0);

            _contentPanel.Height = Math.Max(NameFont.LineHeight + 8, height);
            _contentPanel.Width = width + ((int)_contentPanel.ControlPadding.X * 2);            
            _tagPanel.Width = width;
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
                            _iconRectangle,
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
                                _iconFrame,
                                new Rectangle(_iconRectangle.X, _iconRectangle.Y, _iconRectangle.Width, _iconRectangle.Height),
                                _iconFrame.Bounds,
                                Color.White,
                                0f,
                                default);

                            spriteBatch.DrawOnCtrl(
                                this,
                                _iconFrame,
                                new Rectangle(_iconRectangle.Width, _iconRectangle.Height, _iconRectangle.Width, _iconRectangle.Height),
                                _iconFrame.Bounds,
                                Color.White,
                                6.28f / 2,
                                default);

                            spriteBatch.DrawOnCtrl(
                                this,
                                texture,
                                new Rectangle(8, 8, _iconRectangle.Width - 16, _iconRectangle.Height - 16),
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
                        _iconRectangle,
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
            CalculateRectangles();

            if (MouseOver)
            {
                _textTooltip.Visible = false;

                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(
                        this,
                        ContentService.Textures.Pixel,
                        _iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(Strings.common.Birthday_Text, Character.Name, Character.Age) : string.Format(Strings.common.LoginWith, Character.Name);
                    _textTooltip.Visible = _loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? _loginRect.Contains(RelativeMousePosition) ? _presentTextureOpen : _presentTexture : _loginRect.Contains(RelativeMousePosition) ? _loginTextureHovered : _loginTexture,
                        _loginRect,
                        _loginTexture.Bounds,
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

                    _textTooltip.Text = Character.HasBirthdayPresent ? string.Format(Strings.common.Birthday_Text, Character.Name, Character.Age) : string.Format(Strings.common.LoginWith, Character.Name);
                    _textTooltip.Visible = _loginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(
                        this,
                        Character.HasBirthdayPresent ? _loginRect.Contains(RelativeMousePosition) ? _presentTextureOpen : _presentTexture : _loginRect.Contains(RelativeMousePosition) ? _loginTextureHovered : _loginTexture,
                        _loginRect,
                        _loginTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }

                spriteBatch.DrawOnCtrl(
                    this,
                    _cogRect.Contains(RelativeMousePosition) ? _cogTextureHovered : _cogTexture,
                    _cogRect,
                    new Rectangle(5, 5, 22, 22),
                    Color.White,
                    0f,
                    default);
                if (_cogRect.Contains(RelativeMousePosition))
                {
                    _textTooltip.Text = string.Format(Strings.common.AdjustSettings, Character.Name);
                    _textTooltip.Visible = true;
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
                        _iconRect,
                        Rectangle.Empty,
                        Color.Black * 0.5f,
                        0f,
                        default);

                    spriteBatch.DrawOnCtrl(
                        this,
                        _presentTexture,
                        _loginRect,
                        _presentTexture.Bounds,
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
                        _presentTexture,
                        _loginRect,
                        _presentTexture.Bounds,
                        Color.White,
                        0f,
                        default);
                }
            }
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Character != null && _lastLoginLabel.Visible)
            {
                if (Characters.ModuleInstance.CurrentCharacterModel != Character)
                {
                    TimeSpan ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                    if (Character.HasBirthdayPresent)
                    {
                        // ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                    }
                }
                else
                {
                    _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, 0, 0, 0, 0);
                }
            }

            if (!MouseOver && _textTooltip.Visible)
            {
                _textTooltip.Visible = MouseOver;
            }

            if (!MouseOver && _characterTooltip.Visible)
            {
                _characterTooltip.Visible = MouseOver;
            }

            // CharacterTooltip.Visible = MouseOver;
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e)
        {
            base.OnRightMouseButtonPressed(e);

            var mainWindow = Characters.ModuleInstance.MainWindow;            
            mainWindow.ShowAttachedWindow(mainWindow.CharacterEdit.Character != Character || !mainWindow.CharacterEdit.Visible  ? mainWindow.CharacterEdit : null);
            Characters.ModuleInstance.MainWindow.CharacterEdit.Character = Character;
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
            if (_loginRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.SwapTo(Character);
            }

            // Cog Icon Clicked!
            if (_cogRect.Contains(RelativeMousePosition))
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
                _dragging = true;
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (_dragging)
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = null;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            if (_textTooltip == null || (!_textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                _characterTooltip.Show();
            }

            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            if (_textTooltip == null || (!_textTooltip.Visible && Characters.ModuleInstance.Settings.ShowDetailedTooltip.Value))
            {
                _characterTooltip.Show();
            }

            // if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            _ = Characters.ModuleInstance.MainWindow.CharacterControls.Remove(this);
            Characters.ModuleInstance.MainWindow.Hidden -= MainWindow_Hidden;
            Characters.ModuleInstance.LanguageChanged -= ApplyCharacter;
            Characters.ModuleInstance.Settings.DisplayToggles.SettingChanged -= DisplayToggles_SettingChanged;

            _dataControls?.DisposeAll();
            _contentPanel?.Dispose();
            _textTooltip?.Dispose();
            _characterTooltip?.Dispose();
        }

        private void CalculateRectangles()
        {
            // Math.Min(25, Math.Max(this.Font.LineHeight - 4, this.Height / 5));
            int cogSize = Math.Min(25, Font.LineHeight);

            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                _iconRect = _iconRectangle;
                _cogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                int pad = _iconRect.Width / 5;
                int size = Math.Min(_iconRect.Width - (pad * 2), _iconRect.Height - (pad * 2));
                _loginRect = new Rectangle(pad, pad, size, size);
            }
            else
            {
                _iconRect = _iconRectangle;
                _cogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                int textureSize = Math.Min(42, Math.Min(LocalBounds.Width - 4, LocalBounds.Height - 4));
                _loginRect = new Rectangle((LocalBounds.Width - textureSize) / 2, (LocalBounds.Height - textureSize) / 2, textureSize, textureSize);
            }
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
            if (_tagPanel.Visible && Character.Tags.Count > 0)
            {
                UpdateSize();
            }
        }

        private void TextTooltip_Shown(object sender, EventArgs e)
        {
            _characterTooltip?.Hide();
        }

        private void CharacterDeleted(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            _nameLabel.Text = Character.Name;
            _nameLabel.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            _levelLabel.Text = string.Format(Strings.common.LevelAmount, Character.Level);
            _levelLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _levelLabel.Icon = AsyncTexture2D.FromAssetId(157085);

            _professionLabel.Icon = Character.SpecializationIcon;
            _professionLabel.Text = Character.SpecializationName;

            if (_professionLabel.Icon != null)
            {
                _professionLabel.TextureRectangle = _professionLabel.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);
            }

            _raceLabel.Text = Characters.ModuleInstance.Data.Races[Character.Race].Name;
            _raceLabel.Icon = Characters.ModuleInstance.Data.Races[Character.Race].Icon;

            _mapLabel.Text = Characters.ModuleInstance.Data.GetMapById(Character.Map).Name;
            _mapLabel.TextureRectangle = new Rectangle(2, 2, 28, 28);
            _mapLabel.Icon = AsyncTexture2D.FromAssetId(358406); // 358406 //517180 //157122;

            //_lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(841721);
            _lastLoginLabel.Icon = AsyncTexture2D.FromAssetId(155035);
            _lastLoginLabel.TextureRectangle = new Rectangle(10, 10, 44, 44);
            _lastLoginLabel.Text = string.Format("{1} {0} {2:00}:{3:00}:{4:00}", Strings.common.Days, 0, 0, 0, 0);

            _tagPanel.ClearChildren();
            foreach (string tagText in Character.Tags)
            {
                _ = new Tag()
                {
                    Parent = _tagPanel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                    CanInteract = false,
                };
            }

            _craftingControl.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            // UpdateLayout();
        }
    }
}
