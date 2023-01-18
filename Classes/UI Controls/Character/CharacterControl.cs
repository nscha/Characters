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

namespace Kenedia.Modules.Characters.Classes.MainWindow
{
    public class CharacterControl : Panel
    {
        enum InfoControls
        {
            Name,
            Level,
            Race,
            Profession,
            LastLogin,
            Map,
            Crafting,
        }
        private List<Control> DataControls = new List<Control>();

        private AsyncTexture2D IconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        private AsyncTexture2D LoginTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157092);
        private AsyncTexture2D LoginTextureHovered = GameService.Content.DatAssetCache.GetTextureFromAssetId(157094);
        private AsyncTexture2D CogTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(157109);
        private AsyncTexture2D CogTextureHovered = GameService.Content.DatAssetCache.GetTextureFromAssetId(157111);
        private AsyncTexture2D PresentTexture = GameService.Content.DatAssetCache.GetTextureFromAssetId(593864);
        private AsyncTexture2D PresentTextureOpen = GameService.Content.DatAssetCache.GetTextureFromAssetId(593865);

        private IconLabel Name_Label;
        private IconLabel Level_Label;
        private IconLabel Profession_Label;
        private IconLabel Race_Label;
        private IconLabel Map_Label;
        private IconLabel LastLogin_Label;
        private FlowPanel Tag_Panel;

        private CraftingControl Crafting_Control;
        private BasicTooltip TextTooltip;
        private CharacterTooltip CharacterTooltip;

        private Rectangle LoginRect;
        private Rectangle IconRect;
        private Rectangle CogRect;

        public int TotalWidth
        {
            get
            {
                return _IconRectangle.Width + _ContentRectangle.Width;
            }
        }

        public CharacterControl()
        {
            HeightSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(0, 2);

            ContentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                //WidthSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(0, 5),
            };
            IconDummy = new Dummy()
            {
                Parent = this,
            };

            Name_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            Level_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            Race_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            Profession_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };
            Map_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            Crafting_Control = new CraftingControl()
            {
                Parent = ContentPanel,
                Width = ContentPanel.Width,
                Height = 20,
                Character = Character,
            };

            LastLogin_Label = new IconLabel()
            {
                Parent = ContentPanel,
                AutoSizeWidth = true,
                AutoSizeHeight = true,
            };

            Tag_Panel = new FlowPanel()
            {
                Parent = ContentPanel,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(3, 2),
                Height = Font.LineHeight + 5,
                Visible= false,
            };
            Tag_Panel.Resized += Tag_Panel_Resized;

            DataControls = new List<Control>()
            {
                Name_Label,
                Level_Label,
                Race_Label,
                Profession_Label,
                Map_Label,
                LastLogin_Label,
                Crafting_Control,
                Tag_Panel,
            };

            TextTooltip = new BasicTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1000,

                Size = new Point(300, 50),
                Visible = false,
            };
            TextTooltip.Shown += TextTooltip_Shown;

            CharacterTooltip = new CharacterTooltip()
            {
                Parent = GameService.Graphics.SpriteScreen,
                ZIndex = 1001,

                Size = new Point(300, 50),
                Visible = false,
            };

            Characters.ModuleInstance.LanguageChanged += ApplyCharacter;
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {
            if(Tag_Panel.Visible && Character.Tags.Count > 0) UpdateSize();
        }

        private void TextTooltip_Shown(object sender, EventArgs e)
        {
            CharacterTooltip?.Hide();
        }

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
        private bool Dragging;
        private Character_Model _character;
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
                CharacterTooltip.Character = value;

                if (value != null)
                {
                    _character.Updated += ApplyCharacter;
                    _character.Deleted += CharacterDeleted; ;
                    ApplyCharacter(null, null);
                }
            }
        }

        private void CharacterDeleted(object sender, EventArgs e)
        {
            Dispose();
        }

        private void ApplyCharacter(object sender, EventArgs e)
        {
            Name_Label.Text = Character.Name;
            Name_Label.TextColor = new Color(168 + 15 + 25, 143 + 20 + 25, 102 + 15 + 25, 255);

            Level_Label.Text = "Level " + Character.Level.ToString();
            Level_Label.TextureRectangle = new Rectangle(2, 2, 28, 28);
            Level_Label.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(157085);

            if (Character.Specialization == SpecializationType.None)
            {
                Profession_Label.Text = Characters.ModuleInstance.Data.Professions[Character.Profession].Name;
                Profession_Label.Icon = Characters.ModuleInstance.Data.Professions[Character.Profession].IconBig;
            }
            else if (Characters.ModuleInstance.Data.Specializations.ContainsKey(Character.Specialization))
            {
                Profession_Label.Text = Characters.ModuleInstance.Data.Specializations[Character.Specialization].Name;
                Profession_Label.Icon = Characters.ModuleInstance.Data.Specializations[Character.Specialization].TempIcon;
            }

            if(Profession_Label.Icon != null) Profession_Label.TextureRectangle = Profession_Label.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);

            Race_Label.Text = Characters.ModuleInstance.Data.Races[Character.Race].Name;
            Race_Label.Icon = Characters.ModuleInstance.Data.Races[Character.Race].Icon;

            Map_Label.Text = Characters.ModuleInstance.Data.GetMapById(Character.Map).Name;
            Map_Label.TextureRectangle = new Rectangle(2, 2, 28, 28);
            Map_Label.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(358406); //358406 //517180 //157122;

            LastLogin_Label.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(841721);
            LastLogin_Label.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", 0, 0, 0, 0);
            LastLogin_Label.TextureRectangle = Rectangle.Empty;

            Tag_Panel.ClearChildren();
            foreach(var tagText in Character.Tags)
            {
                new Tag()
                {
                    Parent = Tag_Panel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                    CanInteract = false,
                };
            }            

            Crafting_Control.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            //                    UpdateLayout();
        }
        public Color HoverColor = Color.LightBlue;

        public BitmapFont NameFont = GameService.Content.DefaultFont14;
        public BitmapFont Font = GameService.Content.DefaultFont14;

        private FlowPanel ContentPanel;
        private Dummy IconDummy;
        private Rectangle _IconRectangle;
        private Rectangle _ContentRectangle;

        public void UpdateLabelLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            IconDummy.Visible = _IconRectangle != Rectangle.Empty;
            IconDummy.Size = _IconRectangle.Size;
            IconDummy.Location = _IconRectangle.Location;

            Name_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Name.Value;
            Name_Label.Font = NameFont;

            Level_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Level.Value;
            Level_Label.Font = Font;

            Profession_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Profession.Value;
            Profession_Label.Font = Font;

            Race_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Race.Value;
            Race_Label.Font = Font;

            Map_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Map.Value;
            Map_Label.Font = Font;

            LastLogin_Label.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_LastLogin.Value;
            LastLogin_Label.Font = Font;

            Crafting_Control.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Crafting.Value;
            Crafting_Control.Font = Font;

            Tag_Panel.Visible = !onlyIcon && Characters.ModuleInstance.Settings.Show_Tags.Value && Character.Tags.Count > 0;
            foreach (Tag tag in Tag_Panel.Children)
            {
                tag.Font = Font;
            }

            Crafting_Control.Height = Font.LineHeight + 2;
        }

        public void UpdateLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;
            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                _IconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)));
            }
            else
            {
                _IconRectangle = Rectangle.Empty;
            }

            UpdateLabelLayout();
            UpdateSize();

            CharacterTooltip.NameFont = NameFont;
            CharacterTooltip.Font = Font;
            CharacterTooltip.UpdateLayout();

            _ContentRectangle = onlyIcon ? Rectangle.Empty : new Rectangle(new Point(_IconRectangle.Right, 0), ContentPanel.Size);
            ContentPanel.Location = _ContentRectangle.Location;

            ContentPanel.Visible = !onlyIcon;
        }

        public void UpdateSize()
        {
            var visibleControls = DataControls.Where(e => e.Visible);
            var amount = visibleControls.Count();

            var height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)ContentPanel.ControlPadding.Y) : 0;
            var width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            ContentPanel.Height = Math.Max(NameFont.LineHeight + 8, height);
            ContentPanel.Width = width + (int)ContentPanel.ControlPadding.X;
            Tag_Panel.Width = width;
            
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintBeforeChildren(spriteBatch, bounds);

            if (Character != null)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    if (Character.IconPath != null && Character.Icon != null)
                    {
                        spriteBatch.DrawOnCtrl(this,
                                                Character.Icon,
                                                _IconRectangle,
                                                Character.Icon.Bounds,
                                                Color.White,
                                                0f,
                                                default);
                    }
                    else
                    {
                        var texture = Characters.ModuleInstance.Data.Professions[Character.Profession].IconBig;

                        if (Character.Specialization != SpecializationType.None && Enum.IsDefined(typeof(SpecializationType), Character.Specialization))
                        {
                            texture = Characters.ModuleInstance.Data.Specializations[Character.Specialization].TempIcon;
                        }

                        if (texture != null)
                        {
                            spriteBatch.DrawOnCtrl(this,
                                                 IconFrame,
                                                 new Rectangle(_IconRectangle.X, _IconRectangle.Y, _IconRectangle.Width, _IconRectangle.Height),
                                                 IconFrame.Bounds,
                                                 Color.White,
                                                 0f,
                                                 default);

                            spriteBatch.DrawOnCtrl(this,
                                                 IconFrame,
                                                 new Rectangle(_IconRectangle.Width, _IconRectangle.Height, _IconRectangle.Width, _IconRectangle.Height),
                                                 IconFrame.Bounds,
                                                 Color.White,
                                                 6.28f / 2,
                                                 default);

                            spriteBatch.DrawOnCtrl(this,
                                                 texture,
                                                 new Rectangle(8, 8, _IconRectangle.Width - 16, _IconRectangle.Height - 16),
                                                 texture.Bounds,
                                                 Color.White,
                                                 0f,
                                                 default);
                        }
                    }
                }
                else if (MouseOver)
                {
                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            _IconRectangle,
                                            Rectangle.Empty,
                                            Color.Transparent,
                                            0f,
                                            default);
                }
            }
        }

        private void CalculateRectangles(Rectangle bounds)
        {
            var cogSize = Math.Min(25, Math.Max(Font.LineHeight - 4, Height / 5));

            if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
            {
                IconRect = _IconRectangle;
                CogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                var pad = IconRect.Width / 5;
                var size = Math.Min(IconRect.Width - pad * 2, IconRect.Height - pad * 2);
                LoginRect = new Rectangle(pad, pad, size, size);
            }
            else
            {
                IconRect = _IconRectangle;
                CogRect = new Rectangle(Width - cogSize - 4, 4, cogSize, cogSize);

                var textureSize = Math.Min(42, Math.Min(LocalBounds.Width - 4, LocalBounds.Height - 4));
                LoginRect = new Rectangle((LocalBounds.Width - textureSize) / 2, (LocalBounds.Height - textureSize) / 2, textureSize, textureSize);
            }
        }

        public override void PaintAfterChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            base.PaintAfterChildren(spriteBatch, bounds);
            CalculateRectangles(bounds);

            if (MouseOver)
            {
                TextTooltip.Visible = false;

                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            IconRect,
                                            Rectangle.Empty,
                                            Color.Black * 0.5f,
                                            0f,
                                            default);

                    TextTooltip.Text = Character.HasBirthdayPresent ? String.Format("It was {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age) : String.Format("Log in with '{0}'!", Character.Name);
                    TextTooltip.Visible = LoginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(this,
                                            Character.HasBirthdayPresent ? LoginRect.Contains(RelativeMousePosition) ? PresentTextureOpen : PresentTexture : LoginRect.Contains(RelativeMousePosition) ? LoginTextureHovered : LoginTexture,
                                            LoginRect,
                                            LoginTexture.Bounds,
                                            Color.White,
                                            0f,
                                            default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            bounds,
                                            Rectangle.Empty,
                                            Color.Black * 0.5f,
                                            0f,
                                            default);

                    var padX = Math.Max(2, (bounds.Width - LoginTexture.Bounds.Width * 2) / 2);
                    var padY = Math.Max(2, (bounds.Height - LoginTexture.Bounds.Height * 2) / 2);

                    var size = Math.Min(bounds.Width - padX * 2, bounds.Height - padY * 2);

                    TextTooltip.Text = Character.HasBirthdayPresent ? String.Format("It was {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age) : String.Format("Log in with '{0}'!", Character.Name);
                    TextTooltip.Visible = LoginRect.Contains(RelativeMousePosition);

                    spriteBatch.DrawOnCtrl(this,
                                            Character.HasBirthdayPresent ? LoginRect.Contains(RelativeMousePosition) ? PresentTextureOpen : PresentTexture : LoginRect.Contains(RelativeMousePosition) ? LoginTextureHovered : LoginTexture,
                                            LoginRect,
                                            LoginTexture.Bounds,
                                            Color.White,
                                            0f,
                                            default);
                }

                spriteBatch.DrawOnCtrl(this,
                                        CogRect.Contains(RelativeMousePosition) ? CogTextureHovered : CogTexture,
                                        CogRect,
                                        new Rectangle(5, 5, 22, 22),
                                        Color.White,
                                        0f,
                                        default);
                if (CogRect.Contains(RelativeMousePosition))
                {
                    TextTooltip.Text = String.Format("Adjust {0} settings and tags!", Character.Name);
                    TextTooltip.Visible = true;
                };

                var color = ContentService.Colors.ColonialWhite;

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

            if (!MouseOver && Character != null && Character.HasBirthdayPresent)
            {
                if (Characters.ModuleInstance.Settings.PanelLayout.Value != CharacterPanelLayout.OnlyText)
                {
                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            IconRect,
                                            Rectangle.Empty,
                                            Color.Black * 0.5f,
                                            0f,
                                            default);

                    spriteBatch.DrawOnCtrl(this,
                                            PresentTexture,
                                            LoginRect,
                                            PresentTexture.Bounds,
                                            Color.White,
                                            0f,
                                            default);
                }
                else
                {
                    spriteBatch.DrawOnCtrl(this,
                                            ContentService.Textures.Pixel,
                                            bounds,
                                            Rectangle.Empty,
                                            Color.Black * 0.5f,
                                            0f,
                                            default);

                    spriteBatch.DrawOnCtrl(this,
                                            PresentTexture,
                                            LoginRect,
                                            PresentTexture.Bounds,
                                            Color.White,
                                            0f,
                                            default);
                }
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            base.OnClick(e);

            //Logout Icon Clicked!
            if (LoginRect.Contains(RelativeMousePosition))
            {
                Characters.ModuleInstance.SwapTo(Character);
            }

            //Cog Icon Clicked!
            if (CogRect.Contains(RelativeMousePosition))
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
                Dragging = true;
            }
        }

        protected override void OnLeftMouseButtonReleased(MouseEventArgs e)
        {
            base.OnLeftMouseButtonReleased(e);
            if (Dragging)
            {
                Characters.ModuleInstance.MainWindow.DraggingControl.CharacterControl = null;
            }
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            base.OnMouseMoved(e);
            if(TextTooltip == null ||!TextTooltip.Visible && Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
            //if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        protected override void OnMouseEntered(MouseEventArgs e)
        {
            base.OnMouseEntered(e);
            if (TextTooltip == null || !TextTooltip.Visible && Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
            //if (Characters.ModuleInstance.Settings.Show_DetailedTooltip.Value) CharacterTooltip.Show();
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);

            if (Character != null && LastLogin_Label.Visible && Characters.ModuleInstance.CurrentCharacter_Model != Character)
            {
                var ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                LastLogin_Label.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                if (Character.HasBirthdayPresent)
                {
                    //     ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                }
            }

           if(!MouseOver && TextTooltip.Visible) TextTooltip.Visible = MouseOver;
           if(!MouseOver && CharacterTooltip.Visible) CharacterTooltip.Visible = MouseOver;
           //CharacterTooltip.Visible = MouseOver;
        }

        protected override void DisposeControl()
        {
            base.DisposeControl();

            Characters.ModuleInstance.MainWindow.CharacterControls.Remove(this);
            DataControls?.DisposeAll();
            ContentPanel?.Dispose();
            TextTooltip?.Dispose();
            CharacterTooltip?.Dispose();
        }
    }
}
