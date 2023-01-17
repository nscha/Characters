using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes.MainWindow
{
    public class CharacterTooltip : Container
    {
        public Rectangle TextureRectangle = new Rectangle(40, 25, 250, 250);
        private AsyncTexture2D IconFrame = GameService.Content.DatAssetCache.GetTextureFromAssetId(1414041);
        public AsyncTexture2D Background = GameService.Content.DatAssetCache.GetTextureFromAssetId(156003);
        public Color BackgroundTint = Color.Honeydew * 0.95f;
        public BitmapFont _Font = GameService.Content.DefaultFont14;
        public BitmapFont Font
        {
            get => _Font;
            set
            {
                _Font = value;
            }
        }
        public BitmapFont NameFont = GameService.Content.DefaultFont18;

        public Character_Model Character { get => character; set { character = value; ApplyCharacter(null, null); } }

        private FlowPanel ContentPanel;
        private Dummy IconDummy;

        private IconLabel Name_Label;
        private IconLabel Level_Label;
        private IconLabel Profession_Label;
        private IconLabel Race_Label;
        private IconLabel Map_Label;
        private IconLabel LastLogin_Label;
        private FlowPanel Tag_Panel;

        private CraftingControl Crafting_Control;
        private List<Control> DataControls;

        private Rectangle _IconRectangle;
        private Rectangle _ContentRectangle;

        private Point TextureOffset = new Point(25, 25);
        private Character_Model character;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);
        }

        public override void UpdateContainer(GameTime gameTime)
        {
            base.UpdateContainer(gameTime);
            Location = new Point(Input.Mouse.Position.X, Input.Mouse.Position.Y + 35);

            if (Character != null && LastLogin_Label.Visible && Characters.ModuleInstance.CurrentCharacter_Model != Character)
            {
                var ts = DateTimeOffset.UtcNow.Subtract(Character.LastLogin);
                LastLogin_Label.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", Math.Floor(ts.TotalDays), ts.Hours, ts.Minutes, ts.Seconds);

                if (Character.HasBirthdayPresent)
                {
                    //     ScreenNotification.ShowNotification(String.Format("It is {0}'s birthday! They are now {1} years old!", Character.Name, Character.Age));
                }
            }
        }

        public void UpdateLayout()
        {

            if (_IconRectangle.IsEmpty)
            {
                _IconRectangle = new Rectangle(Point.Zero, new Point(Math.Min(Width, Height), Math.Min(Width, Height)));
            }

            UpdateLabelLayout();
            UpdateSize();

            _ContentRectangle = new Rectangle(new Point(_IconRectangle.Right, 0), ContentPanel.Size);
            ContentPanel.Location = _ContentRectangle.Location;
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

                if (Character.Specialization == SpecializationType.None)
                {
                    texture = Characters.ModuleInstance.Data.Professions[Character.Profession].IconBig;
                }
                else
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

        public CharacterTooltip()
        {
            HeightSizingMode = SizingMode.AutoSize;

            BackgroundColor = new Color(0, 0, 0, 75);
            AutoSizePadding = new Point(5, 5);

            ContentPanel = new FlowPanel()
            {
                Parent = this,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                //WidthSizingMode = SizingMode.AutoSize,
                ControlPadding = new Vector2(5, 2),
                OuterControlPadding = new Vector2(5, 0),
                AutoSizePadding = new Point(5, 5),
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
                Visible = false,
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
        }

        private void Tag_Panel_Resized(object sender, ResizedEventArgs e)
        {

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

            if (Profession_Label.Icon != null) Profession_Label.TextureRectangle = Profession_Label.Icon.Width == 32 ? new Rectangle(2, 2, 28, 28) : new Rectangle(4, 4, 56, 56);

            Race_Label.Text = Characters.ModuleInstance.Data.Races[Character.Race].Name;
            Race_Label.Icon = Characters.ModuleInstance.Data.Races[Character.Race].Icon;

            Map_Label.Text = Characters.ModuleInstance.Data.GetMapById(Character.Map).Name;
            Map_Label.TextureRectangle = new Rectangle(2, 2, 28, 28);
            Map_Label.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(358406); //358406 //517180 //157122;

            LastLogin_Label.Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(841721);
            LastLogin_Label.Text = String.Format("{0} days {1:00}:{2:00}:{3:00}", 0, 0, 0, 0);
            LastLogin_Label.TextureRectangle = Rectangle.Empty;

            Tag_Panel.ClearChildren();
            foreach (var tagText in Character.Tags)
            {
                new Tag()
                {
                    Parent = Tag_Panel,
                    Text = tagText,
                    Active = true,
                    ShowDelete = false,
                };
            }

            Crafting_Control.Character = Character;
            UpdateLabelLayout();
            UpdateSize();

            //                    UpdateLayout();
        }

        public void UpdateLabelLayout()
        {
            var onlyIcon = Characters.ModuleInstance.Settings.PanelLayout.Value == CharacterPanelLayout.OnlyIcons;

            IconDummy.Visible = _IconRectangle != Rectangle.Empty;
            IconDummy.Size = _IconRectangle.Size;
            IconDummy.Location = _IconRectangle.Location;

            Name_Label.Visible = true;
            Name_Label.Font = NameFont;

            Level_Label.Visible = true;
            Level_Label.Font = Font;

            Profession_Label.Visible = true;
            Profession_Label.Font = Font;

            Race_Label.Visible = true;
            Race_Label.Font = Font;

            Map_Label.Visible = true;
            Map_Label.Font = Font;

            LastLogin_Label.Visible = true;
            LastLogin_Label.Font = Font;

            Crafting_Control.Visible = true;
            Crafting_Control.Font = Font;

            Tag_Panel.Visible = Character.Tags.Count > 0;
            foreach (Tag tag in Tag_Panel.Children)
            {
                tag.Font = Font;
            }

            Crafting_Control.Height = Font.LineHeight + 2;
        }

        public void UpdateSize()
        {
            var visibleControls = DataControls.Where(e => e.Visible);
            var amount = visibleControls.Count();

            var height = visibleControls.Count() > 0 ? visibleControls.Aggregate(0, (result, ctrl) => result + ctrl.Height + (int)ContentPanel.ControlPadding.Y) : 0;
            var width = visibleControls.Count() > 0 ? visibleControls.Max(ctrl => ctrl.Width) : 0;

            ContentPanel.Height = height;
            ContentPanel.Width = width + (int)ContentPanel.ControlPadding.X;
            Tag_Panel.Width = width;

        }

    }
}
