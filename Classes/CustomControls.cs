using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BitmapFont = MonoGame.Extended.BitmapFonts.BitmapFont;
using System.IO;

namespace Kenedia.Modules.Characters
{
    public class DataImage : Image
    {
        public int Id;
        public CharacterCrafting Crafting;
    }

    public class CharacterControl : Panel
    {
        public Character assignedCharacter;
        public Image character_Image;
        public Image profession_Image;
        public Image birthday_Image;
        public Image switch_Image;
        public Image separator_Image;

        public Image border_TopRight;
        public Image border_BottomLeft;

        public Label name_Label;
        public Label time_Label;
        public Label level_Label;

        public Panel background_Panel;
        public FlowPanel crafting_Panel;
        public List<DataImage> crafting_Images = new List<DataImage>();

        private int _FramePadding = 6;
        private int _Padding = 4;
        public int Padding {
            get { return _Padding; }
            set 
            {
                _Padding = value;
                AdjustLayout();
            }
        }

        public CharacterControl(Character c)
        {
            ContentService contentService = new ContentService();
            assignedCharacter = c;
            Height = 76;
            WidthSizingMode = SizingMode.Fill;
            ShowBorder = false;
            Tooltip = new CharacterTooltip(assignedCharacter)
            {
                Parent = this,
            };

            var defaultIcon = (assignedCharacter.Icon == null || assignedCharacter.Icon == "");
            border_TopRight = new Image()
            {
                Parent = this,
                Location = new Point(_Padding, _Padding),
                Size = new Point(ContentRegion.Height - (_Padding * 2), ContentRegion.Height - (_Padding * 2)),
                Texture = Textures.Backgrounds[(int)_Backgrounds.BorderTopRight],
                Visible = defaultIcon,
            };
            border_BottomLeft = new Image()
            {
                Parent = this,
                Location = new Point(_Padding, _Padding),
                Size = new Point(ContentRegion.Height - (_Padding * 2), ContentRegion.Height - (_Padding * 2)),
                Texture = Textures.Backgrounds[(int)_Backgrounds.BorderBottomLeft],
                Visible = defaultIcon,
            };

            //Character Image
            character_Image = new Image()
            {
                Texture = c.getProfessionTexture(),
                Location = defaultIcon ? new Point(_Padding + _FramePadding, _Padding + _FramePadding) : new Point(_Padding, _Padding),
                Size = defaultIcon ? new Point(ContentRegion.Height - (_Padding * 2) - (_FramePadding * 2), ContentRegion.Height - (_Padding * 2) - (_FramePadding * 2)) : new Point(ContentRegion.Height - (_Padding * 2), ContentRegion.Height - (_Padding * 2)),
                Parent = this,
                Tooltip = Tooltip,
            };

            background_Panel = new Panel()
            {
                Location = new Point(character_Image.Location.X, character_Image.Location.Y + character_Image.Height - 38),
                Size = new Point(22, character_Image.Height - (character_Image.Location.Y + character_Image.Height - 38)),
                Parent = this,
                BackgroundColor = new Color(43,43, 43, 255),
            };


            //Level Label
            level_Label = new Label()
            {
                Parent = this,
                AutoSizeWidth = true,
                Location = new Point(character_Image.Location.X + 5, character_Image.Location.Y + character_Image.Height - 12),
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size11, ContentService.FontStyle.Regular),

                Text = c.Level.ToString(),
                Tooltip = Tooltip,

                Height = (ContentRegion.Height - (_Padding * 2)) / 2,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            //Profession Image
            profession_Image = new Image()
            {
                Texture = c.getProfessionTexture(false, true),
                Location = new Point(character_Image.Location.X, character_Image.Location.Y + character_Image.Height - 36),
                Size = new Point(24, 24),
                Parent = this,
                Tooltip = Tooltip,
            };

            //Character Name
            name_Label = new Label()
            {
                Parent = this,
                AutoSizeWidth = true,
                Location = new Point(character_Image.Location.X + character_Image.Width + (_Padding * 2) + _FramePadding, 0 + _Padding),
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),

                Text = c.Name,
                Tooltip = Tooltip,

                Height = (ContentRegion.Height - (_Padding * 2)) / 2,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            separator_Image = new Image()
            {
                Location = new Point(character_Image.Location.X + character_Image.Width + _Padding + _FramePadding, (ContentRegion.Height / 2) - (int)Math.Ceiling((decimal)_Padding / 2)),
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = this,
                Size = new Point(Width - character_Image.Width - 2 - (_Padding * 3), (int) Math.Ceiling((decimal) _Padding/2)),
                Tooltip = Tooltip,
            };

            //Time since Login
            time_Label = new Label()
            {
                Location = new Point(character_Image.Location.X + character_Image.Width + (_Padding * 2) + _FramePadding, (ContentRegion.Height / 2) + _Padding - (contentService.DefaultFont18.LineHeight - contentService.DefaultFont12.LineHeight)),
                Text = "00:00:00",
                Parent = this,
                Height = (ContentRegion.Height - (_Padding * 2)) / 2,
                AutoSizeWidth = true,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
                Tooltip = Tooltip,
            };

            //Birthday Image
            birthday_Image = new Image()
            {
                Texture = Textures.Icons[(int)Icons.BirthdayGift],
                Parent = this,
                Location = new Point(Width - 150, (ContentRegion.Height / 2) - 2),
                Size = new Point(32, 32),
                Visible = true,
            };

            var iBegin = character_Image.Location.X + character_Image.Width + (_Padding * 3) + _FramePadding;
            var iWidth = (Width - iBegin) / 2;

            //Crafting Professions
            if (c.Crafting.Count > 0)
            {
                crafting_Panel = new FlowPanel()
                {
                    Location = new Point(iBegin + iWidth, time_Label.Location.Y),
                    Parent = this,
                    Height = ContentRegion.Height,
                    Width = ((Width - (character_Image.Location.X + character_Image.Width + (_Padding * 2) + _FramePadding)) / 2),
                    FlowDirection = ControlFlowDirection.SingleLeftToRight,
                    Tooltip = Tooltip,
                };
                string ttp = "";

                foreach (CharacterCrafting crafting in c.Crafting)
                {
                    if (crafting.Active)
                    {
                        crafting_Images.Add(new DataImage()
                        {
                            Texture = Textures.Crafting[crafting.Id],
                            Size = new Point(24, 24),
                            Parent = crafting_Panel,
                            Visible = (!Module.Settings.OnlyMaxCrafting.Value) || (crafting.Id == 4 || crafting.Id == 7 && crafting.Rating == 400) || (crafting.Rating == 500),
                            Id = crafting.Id,
                            Crafting = crafting,
                            Tooltip = Tooltip,
                        });
                    }
                }
            }

            switch_Image = new Image()
            {
                Location = new Point(Width - 45, 10),
                Texture = Textures.Icons[(int)Icons.Logout],
                Size = new Point(32, 32),
                Parent = this,
                BasicTooltipText = string.Format(Strings.common.Switch, c.Name),
                Visible = false,
            };
            switch_Image.Click += delegate { if (Module.Settings.SwapModifier.Value.PrimaryKey == Keys.None || Module.Settings.SwapModifier.Value.IsTriggering) c.Swap(); };
            switch_Image.MouseEntered += delegate { if(Module.Settings.SwapModifier.Value.PrimaryKey == Keys.None || Module.Settings.SwapModifier.Value.IsTriggering) switch_Image.Texture = Textures.Icons[(int)Icons.LogoutWhite]; };
            switch_Image.MouseLeft += delegate { switch_Image.Texture = Textures.Icons[(int)Icons.Logout]; };

            MouseEntered += delegate {
                BackgroundTexture = Textures.Icons[(int)Icons.RectangleHighlight];
                switch_Image.Visible = (Module.Settings.SwapModifier.Value.PrimaryKey == Keys.None || Module.Settings.SwapModifier.Value.IsTriggering);
            };
            MouseMoved += delegate {
                BackgroundTexture = Textures.Icons[(int)Icons.RectangleHighlight];
                switch_Image.Visible = (Module.Settings.SwapModifier.Value.PrimaryKey == Keys.None || Module.Settings.SwapModifier.Value.IsTriggering);
            };
            MouseLeft += delegate {
                BackgroundTexture = null;
                switch_Image.Visible = false;
            };

            Click += delegate {
                switch (Module.subWindow.Visible)
                {
                    case true:
                        if (!switch_Image.MouseOver && Module.subWindow.assignedCharacter == assignedCharacter)
                        {
                            Module.subWindow.Hide();
                        }

                        if (Module.subWindow.assignedCharacter != assignedCharacter)
                        {
                            Module.subWindow.setCharacter(assignedCharacter);
                        }

                        if (Module.ImageSelectorWindow.Visible) Module.ImageSelectorWindow.assignedCharacter = assignedCharacter;

                        break;

                    case false:
                        if (!switch_Image.MouseOver)
                        {
                            Module.subWindow.Show();
                            Module.filterWindow.Hide();

                            if (Module.subWindow.assignedCharacter != assignedCharacter)
                            {
                                Module.subWindow.setCharacter(assignedCharacter);
                            }

                            if (Module.ImageSelectorWindow.Visible) Module.ImageSelectorWindow.assignedCharacter = assignedCharacter;
                        }
                        break;
                }
            };
            Click += isDoubleClicked;
            Resized += delegate { AdjustLayout(); };
        }
        void isDoubleClicked(object sender, MouseEventArgs e)
        {
                if (e.IsDoubleClick && Module.Settings.DoubleClickToEnter.Value && (Module.Settings.SwapModifier.Value.PrimaryKey == Keys.None || Module.Settings.SwapModifier.Value.IsTriggering))
            {
                assignedCharacter.Swap();
            }
        }
        public void UpdateLanguage()
        {
            CharacterTooltip tooltp = (CharacterTooltip)Tooltip;
            tooltp._Update();
        }
        public void UpdateUI()
        {

            if(assignedCharacter.hadBirthdaySinceLogin())
            {
                birthday_Image.Visible = true;
                birthday_Image.BasicTooltipText = assignedCharacter.Name + " had Birthday! They are now " + assignedCharacter.Years + " years old.";
            }
            else
            {
                birthday_Image.Visible = false;
            }

            character_Image.Texture = assignedCharacter.getProfessionTexture();
            profession_Image.Texture = assignedCharacter.getProfessionTexture(false, true);

            var t = TimeSpan.FromSeconds(assignedCharacter.seconds);
            time_Label.Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00}", t.Hours, t.Minutes, t.Seconds, t.Days);

            if (Tooltip.Visible)
            {
                CharacterTooltip tooltp = (CharacterTooltip)Tooltip;
                tooltp._Update();
            };

            AdjustLayout();
        }
        
        void AdjustLayout()
        {
            var defaultIcon = (assignedCharacter.Icon == null || assignedCharacter.Icon == "");
            character_Image.Location = defaultIcon ? new Point(_Padding + _FramePadding, _Padding + _FramePadding) : new Point(_Padding, _Padding);
            character_Image.Size = defaultIcon ? new Point(ContentRegion.Height - (_Padding * 2) - (_FramePadding * 2), ContentRegion.Height - (_Padding * 2) - (_FramePadding * 2)) : new Point(ContentRegion.Height - (_Padding * 2), ContentRegion.Height - (_Padding * 2));
            border_BottomLeft.Visible = defaultIcon;
            border_TopRight.Visible = defaultIcon;

            var referenceImage = defaultIcon ? border_TopRight : character_Image;
            separator_Image.Location = new Point(referenceImage.Location.X + referenceImage.Width + _Padding, (ContentRegion.Height / 2) - (int)Math.Ceiling((decimal)_Padding / 2));
            separator_Image.Size = new Point(Width - referenceImage.Width - (_Padding * 3), 4);
            birthday_Image.Location = new Point(referenceImage.Location.X + referenceImage.Width - birthday_Image.Width + 4, referenceImage.Location.Y + referenceImage.Height - birthday_Image.Height + 4 );

            switch_Image.Location = new Point(Width - switch_Image.Width - _Padding, (Height - switch_Image.Height - _Padding) / 2 + 2);

            if (assignedCharacter.Crafting.Count > 0)
            {
                var iBegin = character_Image.Location.X + character_Image.Width + (_Padding * 3) + _FramePadding;
                var iWidth = (Width - iBegin) / 2;
                crafting_Panel.Location = new Point(iBegin + iWidth, time_Label.Location.Y + ((time_Label.Height - 24) / 2));
                crafting_Panel.WidthSizingMode = SizingMode.Fill;
            }

            level_Label.Location = new Point(character_Image.Location.X + 3, character_Image.Location.Y + character_Image.Height - 10 - level_Label.Font.LineHeight);
            profession_Image.Location = new Point(character_Image.Location.X - 2, character_Image.Location.Y + character_Image.Height - 38);
            profession_Image.Visible = !defaultIcon;

            background_Panel.Location = new Point(character_Image.Location.X, character_Image.Location.Y + character_Image.Height - 38);
            background_Panel.Size = new Point(22, character_Image.Height - (character_Image.Location.Y + character_Image.Height - 41));
            background_Panel.Visible = !defaultIcon;
        }
    }
    public class ToggleImage : Image
    {
        public ToggleImage()
        {
            Click += delegate { Toggle(); };
        }

        public bool isActive;
        public int Id;
        private int __State;
        public int _State {
            get {
                return __State;
            }
            set {
                __State = value;
                _OnStateChanged();
            }
        }
        public int _MaxState { get; set; }
        public Texture2D[] _Textures;
        public event EventHandler _StateChanged;

        private void _OnStateChanged()
        {
            if (_Textures != null && _Textures.Length > __State)
            {
                Texture = _Textures[__State];
            }
        }

        public int Toggle()
        {
            _State = (_State + 1 > (_MaxState - 1)) ? 0 : _State + 1;
            return _State;
        }
    }
    public class ToggleIcon : Image
    {
        public ToggleIcon()
        {
            Click += delegate { Toggle(); Module.filterCharacterPanel = true; };
            Size = new Point(32, 32);
            Texture = Textures.Icons[(int)Icons.Bug];
        }

        public bool isActive;
        public int Id;
        private int __State;
        public int _State {
            get {
                return __State;
            }
            set {
                __State = value;
                _OnStateChanged();
            }
        }
        public int _MaxState { get; set; }
        public List<Texture2D> _Textures = new List<Texture2D>();
        public event EventHandler _StateChanged;

        private void _OnStateChanged()
        {
            if (_Textures != null && _Textures.Count > __State)
            {
                Texture = _Textures[__State];
            }
        }

        public int Toggle()
        {
            _State = (_State + 1 > (_MaxState - 1)) ? 0 : _State + 1;
            return _State;
        }
    }
    public class CharacterControl_DetailsButton : DetailsButton
    {
        public Character assignedCharacter;
    }
    public class Separator : Panel
    {
        public Image _Separator;
        public int TopPadding = 0;
        public int BottomPadding = 4;
        public int LeftPadding = 0;
        public int RightPadding = 0;
        public int Thickness = 4;

        public Separator ()
        {
            _Separator = new Image()
            {
                Location = new Point(LeftPadding, TopPadding),
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = this,
                Size = new Point(Width - (LeftPadding + RightPadding), Thickness),
            };

            Height = TopPadding + BottomPadding + _Separator.Height;
            WidthSizingMode = SizingMode.Fill;

            PropertyChanged += delegate
            {
                Height = TopPadding + BottomPadding + _Separator.Height;
                _Separator.Location = new Point(LeftPadding, TopPadding);
                _Separator.Size = new Point(Width - (LeftPadding + RightPadding), Thickness);
            };
        }

    }
    public class CharacterTooltip : Tooltip
    {
        public static ContentService ContentService = new ContentService();
        public Character assignedCharacter;

        public IconLabel _Name;
        public IconLabel _Race;
        public IconLabel _Level;
        public IconLabel _Map;
        public IconLabel _Created;
        public IconLabel _NextBirthday;
        public IconLabel _LastLogin;

        public List<IconLabel>_CraftingProfessions = new List<IconLabel>();

        public Separator _Separator;

        public FlowPanel ContentRegion;
        public FlowPanel Tags;
        public List<string> _Tags;
        public WindowBase _Parent;

        public Label _switchInfoLabel;

        public CharacterTooltip(Character character)
        {
            assignedCharacter = character;
            var c = assignedCharacter;
            var index = 0;

            Shown += delegate { _Update(); };
            Resized += delegate
            {
               // if(_Separator != null) _Separator.Width = _Name != null ? _Name.Width + 5 : Width - 20;
               //if(_switchInfoLabel!= null) _switchInfoLabel.Width = _Separator != null ? _Separator.Width : Width - 20;
            };

            ContentRegion = new FlowPanel()
            {
                Location = new Point(0, 0),
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
                Parent = this,
            };

            _Name = new IconLabel()
            {
                //Location = new Point(0, 0),
                Texture = Textures.Icons[(int) Icons.People],
                Parent = ContentRegion,
                Text = c.Name,
                Font = new ContentService().GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size20, ContentService.FontStyle.Regular),
                Gap = 4,
            };

            _Separator = new Separator()
            {
                Parent = ContentRegion,
            };            

            _Race = new IconLabel()
            {
                Texture = Textures.Races[(int)c.Race],
                Text = DataManager.getRaceName(c.Race.ToString()),
                Parent = ContentRegion,
            };

            _Level = new IconLabel()
            {
                Texture = Textures.Icons[(int)Icons.Influence],
                Text = string.Format(Strings.common.Level, c.Level),
                Parent = ContentRegion,
            };

            _Map = new IconLabel()
            {
                Texture = Textures.Icons[(int)Icons.PvE],
                Text = DataManager.getMapName(c.Map),
                Parent = ContentRegion,
            };

            DateTime zeroTime = new DateTime(1, 1, 1);
            TimeSpan span = (DateTime.UtcNow - c.Created.UtcDateTime);
            _Created = new IconLabel()
            {
                Texture = Textures.Icons[(int)Icons.Crown],
                Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year - 1) + " " + Strings.common.Years + ")",
                Parent = ContentRegion,
            };

            span = (c.NextBirthday - DateTime.UtcNow);
            _NextBirthday = new IconLabel()
            {
                Texture = Textures.Icons[(int)Icons.BirthdayGift],
                Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00} " + Strings.common.UntilBirthday,
                span.Hours,
                span.Minutes,
                span.Seconds,
                span.Days
                ),
                Parent = ContentRegion,
            };

            foreach(CharacterCrafting crafting in c.Crafting)
            {
                var ctrl = new IconLabel()
                {
                    Parent = ContentRegion,
                    Text = DataManager.getCraftingName(crafting.Id) + " (" + crafting.Rating + "/" + (crafting.Id == 4 ||crafting.Id == 7  ? 400 : 500) + ")",
                    Texture = crafting.Active ? Textures.Crafting[crafting.Id] : Textures.CraftingDisabled[crafting.Id],
                    _Crafting = crafting,
            };
                ctrl.Label.TextColor = !crafting.Active ? Color.LightGray : ctrl.Label.TextColor;

                _CraftingProfessions.Add(ctrl);
            }

            var t = TimeSpan.FromSeconds(c.seconds);
            _LastLogin = new IconLabel()
            {
                Texture = Textures.Icons[(int)Icons.Clock],
                Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00}",
                t.Hours,
                t.Minutes,
                t.Seconds,
                t.Days
                ),
                Parent = ContentRegion,
            };

            var p = new Panel()
            {
                WidthSizingMode = SizingMode.Fill,
                Parent = ContentRegion,
            };

            _switchInfoLabel = new Label()
            {
                Text = "- " + string.Format(Strings.common.DoubleClickToSwap, assignedCharacter.Name) + " -",
                Parent = p,
                Font = ContentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Regular),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.LightGray,
            };

            p.Resized += delegate { _switchInfoLabel.Width = p.Width; p.Height = _switchInfoLabel.Height + 5; };

            Tags = new FlowPanel()
            {
                Parent = ContentRegion,
                OuterControlPadding = new Vector2(2, 2),
                ControlPadding = new Vector2(5, 2),
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
            };

            this.Invalidate();
            this._Update();
        }

        public void _Update()
        {
            if (assignedCharacter != null && assignedCharacter.characterControl != null)
            {
                ContentService contentService = new ContentService();
                var c = assignedCharacter;

                var t = TimeSpan.FromSeconds(c.seconds);
               _LastLogin.Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Days
                        );

                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = (DateTime.UtcNow - c.Created.UtcDateTime);

                _Created.Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year - 1) + " " + Strings.common.Years + ")";

                span = (c.NextBirthday - DateTime.UtcNow);
                _NextBirthday.Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00} " + Strings.common.UntilBirthday,
                    span.Hours,
                    span.Minutes,
                    span.Seconds,
                    span.Days
                    );

                _Level.Text = string.Format(Strings.common.Level, c.Level);
                _Map.Text = DataManager.getMapName(c.Map);

                if (assignedCharacter.Crafting.Count > 0)
                {
                    foreach (IconLabel iconLabel in _CraftingProfessions)
                    {
                        iconLabel.Text = DataManager.getCraftingName(iconLabel._Crafting.Id) + " (" + iconLabel._Crafting.Rating + "/" + (iconLabel._Crafting.Id == 4 || iconLabel._Crafting.Id == 7 ? 400 : 500) + ")";
                    }
                }

                if (c.Tags != null && (_Tags == null || !Enumerable.SequenceEqual(_Tags, c.Tags)))
                {
                    _Tags = new List<string>(c.Tags);

                    Tags.ClearChildren();
                    foreach (string tag in c.Tags)
                    {
                        new TagEntry(tag, c, Tags, false, contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Regular));
                    }
                }
            }
        }
    }
    public class TagPanel : FlowPanel
    {
        const int PADDING = 2;
        public Texture2D Texture;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Texture == null) Texture = Textures.Backgrounds[(int)_Backgrounds.Tag];

            spriteBatch.DrawOnCtrl(this, Texture, bounds, new Rectangle(3, 4, _size.X, _size.Y), Color.White * 0.98f);

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, _size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, _size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.Black * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 3, 1, 3, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 2, 1, 1, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 3, _size.X - 2, 3).Add(-PADDING, PADDING, PADDING * 2, 0), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 2, _size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0), Color.Black * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 3, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, 1, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.Black * 0.6f);
        }
    }
    public class FilterWindow : BasicContainer
    {
        public FilterWindow()
        {
            this.Click += delegate { lastInput = DateTime.Now; Opacity = 1; };
            MouseMoved += delegate { lastInput = DateTime.Now; Opacity = 1; Module.Logger.Debug("Mouse Moved!"); };
            this.MouseEntered += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.MouseLeft += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.Shown += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.Hidden += delegate { Opacity = 1; };
        }
        public DateTime lastInput;
        public int Alpha = 255;
        public HeadedFlowRegion Utility;
        public HeadedFlowRegion Crafting;
        public HeadedFlowRegion Profession;
        public HeadedFlowRegion Specialization;
        public HeadedFlowRegion CustomTags;
        public StandardButton toggleSpecsButton;
        public ToggleIcon visibleToggle;
        public ToggleIcon birthdayToggle;
    }
    public class RectangleBorder : Container
    {
        const int PADDING = 2;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, _size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.White * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, _size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), Color.White * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 3, 1, 3, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.White * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 2, 1, 1, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), Color.White * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 3, _size.X - 2, 3).Add(-PADDING, PADDING, PADDING * 2, 0), Color.White * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 2, _size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0), Color.White * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 3, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.White * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, 1, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), Color.White * 0.6f);
        }
    }
    public class BasicContainer : Container
    {
        const int PADDING = 2;
        public Texture2D Texture;
        public bool showBackground = true;
        public Color FrameColor = Color.Black;
        public Color TextureColor = Color.White;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (showBackground)
            {
                if (Texture == null) Texture = Textures.Backgrounds[(int)_Backgrounds.Tooltip];
                spriteBatch.DrawOnCtrl(this, Texture, bounds, new Rectangle(3, 4, _size.X, _size.Y), TextureColor * 0.98f);
            }

            // Top
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 0, _size.X - 2, 3).Add(-PADDING, -PADDING, PADDING * 2, 0), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, _size.X - 2, 1).Add(-PADDING, -PADDING, PADDING * 2, 0), FrameColor * 0.6f);

            // Right
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 3, 1, 3, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(_size.X - 2, 1, 1, _size.Y - 2).Add(PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.6f);

            // Bottom
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 3, _size.X - 2, 3).Add(-PADDING, PADDING, PADDING * 2, 0), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, _size.Y - 2, _size.X - 2, 1).Add(-PADDING, PADDING, PADDING * 2, 0), FrameColor * 0.6f);

            // Left
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 1, 3, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.5f);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(1, 1, 1, _size.Y - 2).Add(-PADDING, -PADDING, 0, PADDING * 2), FrameColor * 0.6f);
        }
    }
    public class ScreenCaptureWindow : BasicContainer
    {
        public Action LoadCustomImages;

        private List<StandardButton> captureButtons = new List<StandardButton>();
        private StandardButton captureAll_Button;
        private StandardButton close_Button;
        private Label disclaimer_Label;


        public ScreenCaptureWindow(Point Dimensions)
        {
            double scale = GameService.Graphics.UIScaleMultiplier;

            var resolution = GameService.Graphics.Resolution;
            var sidePadding = 255;
            var bottomPadding = 100;

            var CharacterImageSize = (int)(140);
            var Image_Gap = -10;
            var topMenuHeight = 60;

            Size = Dimensions;

            Image_Gap = 17;
            CharacterImageSize = 124;

            captureAll_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.CaptureAll,
                Location = new Point(4 + 2 + (5 * (CharacterImageSize + Image_Gap)), 0),
                Size = new Point(CharacterImageSize, topMenuHeight - 30),
            };

            var contentService = new ContentService();
            disclaimer_Label = new Label()
            {
                Parent = this,
                Text = Strings.common.UISizeDisclaimer,
                Location = new Point(0, 30),
                Size = new Point(Width - CharacterImageSize - Image_Gap, topMenuHeight - 30),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextColor = Color.Red,
                Font = contentService.DefaultFont18,
                //BackgroundColor = Color.Black,
            };

            close_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.Close,
                Location = new Point(4 + 2 + (5 * (CharacterImageSize + Image_Gap)), 30),
                Size = new Point(CharacterImageSize, topMenuHeight - 30),
            };
            close_Button.Click += delegate
            {
                Visible = false;
                Module.MainWidow.Show();
                Module.ImageSelectorWindow.Show();
                Module.ResetGameWindow();
            };

            for (int i = 0; i < (5); i++)
            {
                int[] offsets = { -1, 0, 0, 1, 1 };
                var ctn = new BasicContainer()
                {
                    showBackground = false,
                    FrameColor = Color.Transparent,
                    Parent = this,
                    Location = new Point(4 + offsets[i] + (i * (CharacterImageSize + Image_Gap)), 1 + topMenuHeight),
                    Size = new Point(CharacterImageSize, CharacterImageSize),
                    Visible = false,
                };

                var btn = new StandardButton()
                {
                    Parent = this,
                    Text = Strings.common.Capture,
                    Location = new Point(4 + offsets[i] + (i * (CharacterImageSize + Image_Gap)), 0),
                    Size = new Point(CharacterImageSize, topMenuHeight - 30),
                };

                void click()
                {
                    var images = Directory.GetFiles(Module.GlobalImagesPath, "*.png", SearchOption.AllDirectories).ToList();

                    //Last.Tick_ImageSave = DateTime.Now;
                    CharacterImageSize = 110;
                    var TitleBarHeight = 33;
                    var SideBarWidth = 10;
                    var clientRectangle = new Module.RECT();
                    Module.GetWindowRect(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, ref clientRectangle);

                    var cPos = ctn.AbsoluteBounds;
                    double factor = GameService.Graphics.UIScaleMultiplier;

                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(CharacterImageSize, CharacterImageSize))
                    {
                        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
                        {
                            var x = (int)(cPos.X * factor);
                            var y = (int)(cPos.Y * factor);

                            g.CopyFromScreen(new System.Drawing.Point(clientRectangle.Left + x + SideBarWidth, clientRectangle.Top + y + TitleBarHeight), System.Drawing.Point.Empty, new System.Drawing.Size(CharacterImageSize, CharacterImageSize));
                        }
                        bitmap.Save(Module.GlobalImagesPath + "Image " + (images.Count + 1) + ".png", System.Drawing.Imaging.ImageFormat.Png);
                    }

                    LoadCustomImages();
                };

                btn.Click += delegate { click(); };
                captureAll_Button.Click += delegate { click(); };
                captureButtons.Add(btn);
            }
        }
        public void UpdateLanguage()
        {
            disclaimer_Label.Text = Strings.common.UISizeDisclaimer;
            close_Button.Text = Strings.common.Close;
            captureAll_Button.Text = Strings.common.CaptureAll;
            foreach (StandardButton btn in captureButtons)
            {
                btn.Text = Strings.common.Capture;
            }
    }
}
    public class ImageSelector : BasicContainer
    {
        private Character _assignedCharacter = new Character();
        public Character assignedCharacter 
        {
            get { return _assignedCharacter; }
            set {
                if (assignedCharacter != value)
                {
                    _assignedCharacter = value;
                    setCharacter();
                    selected_Image = null;
                }
            }
        }

        private Texture2D selected_Image;
        private Label name_Label;
        private Image character_Image;
        private Image selector_Image;
        private FlowPanel images_Panel;
        private List<string> ImageNames = new List<string>();
        private HeaderUnderlined header_HeaderUnderlined;

        private StandardButton save_Button;
        private StandardButton default_Button;
        private StandardButton refresh_Button;
        private StandardButton cancel_Button;
        private StandardButton create_Button;

        public ImageSelector(int width, int height) {
            Width = width;
            Height = height;

            var contentService = new ContentService();
            var iPadding = 5;

            var set_img = new Image()
            {
                Parent = this,
                Texture = Textures.Emblems[(int) _Emblems.Settings],
                Size = new Point(64,64),
            };
            set_img.Click += delegate
            {
                Hide();
            };

            header_HeaderUnderlined = new HeaderUnderlined()
            {
                Parent = this,
                Text = Strings.common.SelectImage,
                Location = new Point(72, iPadding),
                Font = contentService.DefaultFont32,
                Width = 300,
            };
            character_Image = new Image()
            {
                Parent = this,
                Size = new Point(75, 75),
                Location = new Point(Width - 75 - iPadding, iPadding),
                Texture = new Character().getProfessionTexture(),
            };
            var panel =  new Panel()
            {
                Parent = this,
                Location = new Point(0, 75 + iPadding),
                Height = Height - iPadding - 75,
                Padding = new Thickness(8,8),
                //ControlPadding = new Vector2(8, 8),
                //HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
            };
            selector_Image = new Image()
            {
                //Parent = this,
                Parent = panel,
                Size = new Point(104, 110),
                Location = new Point(0, 0),
                Texture = Textures.Backgrounds[(int)_Backgrounds.Selector],
                Visible = true,
            };

            name_Label = new Label()
            {
                Parent = this,
                Text = "Character Name",
                AutoSizeWidth = true,
                Font = contentService.DefaultFont18,
                AutoSizeHeight = true,
                VerticalAlignment = VerticalAlignment.Top,
            };
            name_Label.Location = new Point(Width - character_Image.Width - (iPadding * 3) - name_Label.Width, iPadding);

            default_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.UseDefault,
                Location = new Point(Width - character_Image.Width - (iPadding * 3) - 125, iPadding + name_Label.Height + iPadding),
                Size = new Point(125, 25),
            };
            default_Button.Click += delegate
            {
                selected_Image = null;
                character_Image.Texture = assignedCharacter.getProfessionTexture(false);
            };

            refresh_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.RefreshImages,
                Location = new Point(75, 55),
                Size = new Point(150, 25),
            };
            refresh_Button.Click += delegate
            {
                //images_Panel.ClearChildren();
                LoadImages();
            };

            create_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.CreateImages,
                Location = new Point(75 + 5 + 150, 55),
                Size = new Point(150, 25),
            };
            create_Button.Click += delegate
            {
                Module.screenCaptureWindow.Visible = true;

                if (!GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    var pos = new Module.RECT();
                    Module.GetWindowRect(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, ref pos);
                    Module.MoveWindow(GameService.GameIntegration.Gw2Instance.Gw2WindowHandle, pos.Left, pos.Top, 1100, 800, false);

                    this.Hide();
                    Module.MainWidow.Hide();
                    Module.screenCapture = true;
                }
            };

            save_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.Save,
                Location = new Point(400, 10),
                Size = new Point(125, 25),
            };
            save_Button.Click += delegate
            {
                assignedCharacter.Icon = selected_Image != null ? selected_Image.Name : "";
                assignedCharacter.characterControl.UpdateUI();
                Module.subWindow.setCharacter(assignedCharacter);
                assignedCharacter.Save();
                this.Visible = false;
            };

            cancel_Button = new StandardButton()
            {
                Parent = this,
                Text = Strings.common.Cancel,
                Location = new Point(550, 10),
                Size = new Point(125, 25),
            };
            cancel_Button.Click += delegate
            {
                this.Visible = false;
            };

            images_Panel = new FlowPanel()
            {
                Parent = this,
                Location = new Point(0, 75 + iPadding),
                Height = Height - iPadding - 75,
                CanScroll = true,
                ControlPadding = new Vector2(8, 8),
                ShowBorder = true,
                //HeightSizingMode = SizingMode.Fill,
                WidthSizingMode = SizingMode.Fill,
            };
        }
        public void LoadImages()
        {
            
            foreach (Texture2D pic in Textures.CustomImages)
            {
                if (pic != null && !ImageNames.Contains(pic.Name))
                {
                    var img = new Image()
                    {
                        Parent = images_Panel,
                        Size = new Point(96, 96),
                        Texture = pic,
                    };
                    img.MouseEntered += delegate 
                    {
                        selector_Image.Location = new Point(img.Location.X, img.Location.Y - images_Panel.VerticalScrollOffset);

                        //selector_Image.Location = new Point(images_Panel.Location.X + img.Location.X, images_Panel.Location.Y + img.Location.Y - images_Panel.VerticalScrollOffset);
                        //selector_Image.Visible = (images_Panel.AbsoluteBounds.Y <= selector_Image.AbsoluteBounds.Y);
                    };

                    img.Click += delegate
                    {
                        character_Image.Texture = img.Texture;
                        selected_Image = pic;
                    };

                    ImageNames.Add(pic.Name);
                }
            }

            images_Panel.Invalidate();
        }
        private void setCharacter()
        {
            name_Label.Text = assignedCharacter.Name;
            character_Image.Texture = assignedCharacter.getProfessionTexture();
        }
        public void UpdateLanguage() 
        {
            header_HeaderUnderlined.Text = Strings.common.SelectImage;
            save_Button.Text = Strings.common.Save;
            create_Button.Text = Strings.common.CreateImages;
            refresh_Button.Text = Strings.common.RefreshImages;
            default_Button.Text = Strings.common.UseDefault;
            cancel_Button.Text = Strings.common.Cancel;
        }
    }
    public class HeaderUnderlined : Panel
    {
        static ContentService contentService = new ContentService();
        private BitmapFont _Font;
        public BitmapFont Font {
            get { return _Font; }
            set {
                _Font = value;
                textLabel.Font = value;
                textLabel.Height = _Font.LineHeight + 4;

                if (AlignCentered)
                {
                    textLabel.Width = Math.Max((int)Math.Ceiling(_Font.MeasureString(textLabel.Text).Width) + _HorizontalPadding, Width - (_HorizontalPadding * 2));
                }

                Invalidate();
            }
        }
        private bool AlignCentered;
        public Label textLabel;
        public Image Separator_Image;
        private string _Text;
        public string Text
        {
            get { return _Text; }
            set {
                _Text = value; 
                textLabel.Text = value;
                textLabel.Height = Font.LineHeight + 4;

                if (AlignCentered)
                {
                    textLabel.Width = Math.Max((int)Math.Ceiling(Font.MeasureString(value).Width) + _HorizontalPadding, Width - (_HorizontalPadding * 2));
                    Invalidate();
                }
            }
        }
        private int _HorizontalPadding = 5;
        public int HorizontalPadding
        {
            get { return _HorizontalPadding; }
            set { _HorizontalPadding = value; textLabel.Location = new Point(_HorizontalPadding, _VerticalPadding); Separator_Image.Location = new Point(0, textLabel.Location.Y + textLabel.Height + _VerticalPadding); }
        }
        private int _VerticalPadding = 3;
        public int VerticalPadding
        {
            get { return _VerticalPadding; }
            set { _VerticalPadding = value; textLabel.Location = new Point(_HorizontalPadding, _VerticalPadding); Separator_Image.Location = new Point(0, textLabel.Location.Y + textLabel.Height + _VerticalPadding); }
        }

        public HeaderUnderlined()
        {
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            _Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);

            textLabel = new Label()
            {
                Location = new Point(_HorizontalPadding, 0),
                Text = "",
                Parent = this,
                AutoSizeWidth = true,
                Height = Font.LineHeight + 4,
                Font = Font,
            };

            Separator_Image = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = this,
                Location = new Point(0, textLabel.Location.Y + textLabel.Height + VerticalPadding),
                Size = new Point(Width, 4),
            };

            Resized += delegate
            {
                textLabel.Invalidate();
                Separator_Image.Size = new Point(Width, 4);
                Separator_Image.Location = new Point(0, textLabel.Location.Y + textLabel.Height + VerticalPadding);
            };
        }

        public HeaderUnderlined(bool centered)
        {
            AlignCentered = centered;

            WidthSizingMode = AlignCentered ? SizingMode.Standard: SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            _Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);

            textLabel = new Label()
            {
                Location = new Point(_HorizontalPadding, 0),
                Text = "",
                Parent = this,
                Height = Font.LineHeight + 4,
                Font = Font,
            };

            Separator_Image = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = this,
                Location = new Point(0, textLabel.Location.Y + textLabel.Height + VerticalPadding),
                Size = new Point(Width, 4),
            };

            Resized += delegate
            {
                Separator_Image.Size = new Point(Width, 4);
                Separator_Image.Location = new Point(0, textLabel.Location.Y + textLabel.Height + VerticalPadding);

                var newWidth = Math.Max((int)Math.Ceiling(Font.MeasureString(textLabel.Text).Width) + _HorizontalPadding, Width - _HorizontalPadding);
                textLabel.Width = newWidth;

                if (newWidth > Width - (_HorizontalPadding))
                {
                    Width = newWidth + _HorizontalPadding;
                    Invalidate();
                }
                
                Module.Logger.Debug("TEXT:" + textLabel.Text + "; LABEL WIDTH: " + textLabel.Width + "; HEADER WIDTH: " + Width);
            };
        }
    }
    public class HeadedFlowRegion : HeaderUnderlined
    {
        public FlowPanel contentFlowPanel;
        public HeadedFlowRegion()
        {
            contentFlowPanel = new FlowPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.Fill,
                HeightSizingMode = SizingMode.AutoSize,
                Location = new Point(0, Separator_Image.Location.Y + Separator_Image.Height + VerticalPadding),
                //AutoSizePadding = new Point(5, 2),
                OuterControlPadding = new Vector2(HorizontalPadding, VerticalPadding),
                //ControlPadding = new Vector2(HorizontalPadding, VerticalPadding),

            };

            Resized += delegate
            {
                Separator_Image.Size = new Point(Width, 4);
                Separator_Image.Location = new Point(0, textLabel.Location.Y + textLabel.Height + VerticalPadding);
                contentFlowPanel.Location = new Point(0, Separator_Image.Location.Y + Separator_Image.Height + VerticalPadding);
            };
        }
    }
    public class TagEntry : Panel
    {
        public Character assignedCharacter;
        public Label textLabel;
        public Image deleteButton;
        private TagPanel panel;
        private bool _showDeleteButton;
        private bool _Highlighted = true;
        public bool Highlighted
        {
            get { return _Highlighted; }
            set
            {
                _Highlighted = value;

                if (value)
                {
                    panel.Texture = Textures.Backgrounds[(int)_Backgrounds.Tag];
                    deleteButton.Texture = Textures.Icons[(int)Icons.Close];
                }
                else
                {
                    panel.Texture = Textures.Backgrounds[(int)_Backgrounds.TagDisabled];
                    deleteButton.Texture = Textures.Icons[(int)Icons.CloseDisabled];
                }
            }
        }

        public bool showDeleteButton {
            get { return _showDeleteButton; }
            set
            {
                _showDeleteButton = value;
                if (deleteButton != null) deleteButton.Visible = value;
            }
        }
        private bool Discardable;

        public TagEntry(string txt, Character character, FlowPanel parent, bool showButton = true, MonoGame.Extended.BitmapFonts.BitmapFont font = default)
        {
            ContentService contentService = new ContentService();
            var textFont = (font == default) ? contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular) : font;

            assignedCharacter = character;
            Parent = parent;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;

            panel = new TagPanel()
            {
                Parent = this,
                WidthSizingMode = SizingMode.AutoSize,
                HeightSizingMode = SizingMode.AutoSize,
                FlowDirection = ControlFlowDirection.SingleLeftToRight,
                OuterControlPadding = new Vector2(5, 3),
                ControlPadding = new Vector2(3, 0),
                AutoSizePadding = new Point(5, 2),
            };
            panel.Click += delegate {
                if (!Highlighted)
                {
                    Highlighted = true;
                    assignedCharacter.Tags.Add(Text);
                }
            };

            deleteButton = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Close],
                Parent = panel,
                //Location = new Point(panel.Width - 32 - 4, -4),
                Size = new Point(21, 23),
                Visible = showDeleteButton,
            };
            deleteButton.MouseEntered += delegate { deleteButton.Texture = _Highlighted ? Textures.Icons[(int)Icons.CloseHovered] : Textures.Icons[(int)Icons.CloseDisabled]; };
            deleteButton.MouseLeft += delegate { deleteButton.Texture = _Highlighted ? Textures.Icons[(int)Icons.Close] : Textures.Icons[(int)Icons.CloseDisabled]; };
            deleteButton.Click += delegate
            {
                Highlighted = false;
                assignedCharacter.Tags.Remove(Text);
                assignedCharacter.Save();

                foreach (string s in assignedCharacter.Tags)
                {
                    Module.Logger.Debug(s);
                }

                if (Module.filterTagsPanel != null)
                {
                    var tempList = new List<string>(Module.Tags);
                    foreach (Character c in Module.Characters)
                    {
                        foreach (string t in c.Tags)
                        {
                            if (tempList.Contains(t)) tempList.Remove(t);
                        }
                    }

                    List<TagEntry> deleteList = new List<TagEntry>();
                    foreach (TagEntry tag in Module.filterTagsPanel)
                    {
                        if (tempList.Contains(tag.Text))
                        {
                            Module.Tags.Remove(tag.Text);
                            deleteList.Add(tag);
                        };
                    }

                    foreach (TagEntry tag in deleteList)
                    {
                        if (tag.Text == Text) this.Dispose();
                        tag.Dispose();
                    }
                }

                if (Discardable) this.Dispose();
            };

            textLabel = new Label()
            {
                Text = txt,
                Parent = panel,
                AutoSizeWidth = true,
                Height = showDeleteButton ? deleteButton.Size.Y : textFont.LineHeight + 4,
                Font = textFont,
                VerticalAlignment = VerticalAlignment.Middle,
            };

            panel.Invalidate();

            _Text = txt;
            showDeleteButton = showButton;
        }

        private string _Text;
        public string Text {
            get { return _Text; }
            set
            {
                _Text = value;
                textLabel.Text = " " + value + " "; }
        }

        private void DeleteOG()
        {
            if (_Highlighted)
            {
                assignedCharacter.Tags.Remove(Text);

                if (Module.filterTagsPanel != null)
                {
                    var tempList = new List<string>(Module.Tags);
                    foreach (Character c in Module.Characters)
                    {
                        foreach (string t in c.Tags)
                        {
                            if (tempList.Contains(t)) tempList.Remove(t);
                        }
                    }

                    List<TagEntry> deleteList = new List<TagEntry>();
                    foreach (TagEntry tag in Module.filterTagsPanel)
                    {
                        if (tempList.Contains(tag.Text))
                        {
                            Module.Tags.Remove(tag.Text);
                            deleteList.Add(tag);
                        };
                    }

                    foreach (TagEntry tag in deleteList)
                    {
                        tag.Dispose();
                    }
                }

                this.Dispose();
            }
        }
    }
    public class CharacterDetailWindow : BasicContainer
    {
        public CharacterDetailWindow()
        {
            this.Click += delegate { lastInput = DateTime.Now; Opacity = 1; };
            MouseMoved += delegate { lastInput = DateTime.Now; Opacity = 1; Module.Logger.Debug("Mouse Moved!"); };
            this.MouseEntered += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.MouseLeft += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.Shown += delegate { lastInput = DateTime.Now; Opacity = 1; };
            this.Hidden += delegate { Opacity = 1; };
        }
        public DateTime lastInput;
        public TextBox tag_TextBox;
        public Image addTag_Button;
        public Label name_Label;
        public Image spec_Image;
        public Image include_Image;

        public Image border_TopRight;
        public Image border_BottomLeft;

        public Label spec_Label;
        public Image separator_Image;
        public FlowPanel customTags_Panel;
        public Checkbox loginCharacter;
        private List<string> Tags = new List<string>();

        public Character assignedCharacter;
        public void setCharacter(Character c)
        {
            assignedCharacter = c;
            name_Label.Text = c.Name;
            //spec_Label.Text = DataManager.getProfessionName(c._Profession);
            //spec_Image.Texture = Textures.Professions[c._Profession];
            spec_Image.Texture = c.getProfessionTexture();

            loginCharacter.Checked = c.loginCharacter;
            include_Image.Texture = c.include ? Textures.Icons[(int)Icons.Visible] : Textures.Icons[(int)Icons.Hide];

            if (!Enumerable.SequenceEqual(Tags, Module.Tags))
            {
                customTags_Panel.ClearChildren();
                Tags = new List<string>(Module.Tags);

                foreach (string tag in Module.Tags)
                {
                    new TagEntry(tag, c, customTags_Panel);
                }
            }

            foreach (TagEntry tag in customTags_Panel)
            {
                tag.Highlighted = c.Tags.Contains(tag.Text);
                tag.assignedCharacter = c;
            }

            var defaultIcon = (assignedCharacter.Icon == null || assignedCharacter.Icon == "");
            border_TopRight.Visible = defaultIcon;
            border_BottomLeft.Visible = defaultIcon;

            include_Image.BasicTooltipText = string.Format(Strings.common.ShowHide_Tooltip, c.Name);
        }
    }
    public class IconLabel : Panel
    {
        public CharacterCrafting _Crafting;
        public int Gap { 
            get { return _Gap;  }
            set
            {
                _Gap = value;
                Label.Location = new Point(Font.LineHeight + 4 + _Gap, 0);
                Invalidate();
            }
        }
        private int _Gap = 8;
        public Image Image;
        public Label Label;
        private Texture2D _Texture;
        public Texture2D Texture
        {
            get {
                return _Texture;
                    }
            set {
                _Texture = value;
                if (Image != null) Image.Texture = _Texture;
            }
        }
        private string _Text;
        public string Text
        {
            get {
                return _Text;
                    }
            set {
                _Text = value;
                if (Label != null)
                {
                    Label.Text = _Text;
                    Invalidate();
                };
            }
        }
        private BitmapFont _Font;
        public BitmapFont Font
        {
            get {
                return _Font;
                    }
            set {
                _Font = value;
                if (Label != null)
                {
                    Label.Font = _Font;
                    Label.Height = Font.LineHeight + 4;
                    Image.Size = new Point(Font.LineHeight + 4, Font.LineHeight + 4);
                    Label.Location = new Point(Font.LineHeight + 4 + _Gap, 0);

                    Invalidate();
                };
            }
        }

        public IconLabel()
        {
            HeightSizingMode = SizingMode.AutoSize;
            WidthSizingMode = SizingMode.AutoSize;
            _Font = new ContentService().GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular);

            Image = new Image()
            {
                Size = new Point(Font.LineHeight + 4, Font.LineHeight + 4),
                Location = new Point(0, 0),
                Parent = this,
            };
            Label = new Label()
            {
                Height = Font.LineHeight + 4,
                VerticalAlignment = VerticalAlignment.Middle, 
                Location = new Point(Image.Size.X + _Gap, 0),
                Parent = this,
                AutoSizeWidth = true,
            };
        }
    }
}