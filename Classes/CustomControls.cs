using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
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

namespace Kenedia.Modules.Characters
{
    public class CharacterControl : Panel
    {
        public Character assignedCharacter;
    }
    public class ToggleImage : Image
    {
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
    public class CharacterControl_DetailsButton : DetailsButton
    {
        public Character assignedCharacter;        
    }
    public class CharacterTooltip : Tooltip
    {
        public Character assignedCharacter;

        public Image _nameTexture;
        public Label _nameLabel;

        public Image _levelTexture;
        public Label _levelLabel;

        public Image _raceTexture;
        public Label _raceLabel;

        public Image _mapTexture;
        public Label _mapLabel;

        public Image _createdTexture;
        public Label _createdLabel;

        public Image _ageTexture;
        public Label _ageLabel;

        public FlowPanel Tags;
        public List<string> _Tags;
        public WindowBase _Parent;


        public void _Create()
        {
            if (assignedCharacter != null && assignedCharacter.characterControl != null)
            {
                Blish_HUD.ContentService contentService = new Blish_HUD.ContentService();
                var c = assignedCharacter;
                var parent = assignedCharacter.characterControl;
                var index = 0;

                _nameTexture = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.Crown],
                    Parent = this,
                    Location = new Point(0, 0 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = false,
                };
                _nameLabel = new Label()
                {
                    Text = c.Name,
                    Parent = this,
                    Location = new Point(0, 0 + (index * 25)),
                    Visible = true,
                    Width = 200,
                    Font = contentService.GetFont(Blish_HUD.ContentService.FontFace.Menomonia, Blish_HUD.ContentService.FontSize.Size20, Blish_HUD.ContentService.FontStyle.Regular),
                    HorizontalAlignment = HorizontalAlignment.Center,
                };
                new Image()
                {
                    Texture = Textures.Icons[(int)Icons.Separator],
                    Parent = this,
                    Location = new Point(0, 25 + (index * 25)),
                    Size = new Point(this.Width - 0, 4),
                };

                _raceTexture = new Image()
                {
                    Texture = Textures.Races[(int)c.Race],
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = true,
                };
                _raceLabel = new Label()
                {
                    Text = DataManager.getRaceName(c.Race.ToString()),
                    Parent = this,
                    Location = new Point(30, 40 + (index * 25)),
                    Visible = true,
                    AutoSizeWidth = true,
                };

                index++;
                _levelTexture = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.Influence],
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = true,
                };
                _levelLabel = new Label()
                {
                    Text = string.Format(Strings.common.Level, c.Level),
                    Parent = this,
                    Location = new Point(30, 40 + (index * 25)),
                    Visible = true,
                    AutoSizeWidth = true,
                };

                index++;
                _mapTexture = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.PvE],
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = true,
                };
                _mapLabel = new Label()
                {
                    Text = DataManager.getMapName(c.map),
                    Parent = this,
                    Location = new Point(30, 40 + (index * 25)),
                    Visible = true,
                    AutoSizeWidth = true,
                };

                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = (DateTime.UtcNow - c.Created.UtcDateTime);
                index++;
                _createdTexture = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.BirthdayGift],
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = true,
                };
                _createdLabel = new Label()
                {
                    Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year - 1) + " "+ Strings.common.Years + ")",
                    Parent = this,
                    Location = new Point(30, 40 + (index * 25)),
                    Visible = true,
                    AutoSizeWidth = true,
                };

                var t = TimeSpan.FromSeconds(c.seconds);
                index++;
                _ageTexture = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.Clock],
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Size = new Point(20, 20),
                    Visible = true,
                };
                _ageLabel = new Label()
                {
                    Text = string.Format("{3} " + Strings.common.Days +" {0:00}:{1:00}:{2:00}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Days
                    ),
                    Parent = this,
                    Location = new Point(30, 40 + (index * 25)),
                    Visible = true,
                    AutoSizeWidth = true,
                };

                index++;
                Tags = new FlowPanel()
                {
                    Parent = this,
                    Location = new Point(0, 40 + (index * 25)),
                    Width = this.Width,
                    OuterControlPadding = new Vector2(2, 2),
                    ControlPadding = new Vector2(5, 2),
                    HeightSizingMode = SizingMode.AutoSize,
                };

                this.Invalidate();
                this._Update();
            }
        }

        public void _Update()
        {
            if (assignedCharacter != null && assignedCharacter.characterControl != null)
            {
                Blish_HUD.ContentService contentService = new Blish_HUD.ContentService();
                var c = assignedCharacter;

                var t = TimeSpan.FromSeconds(c.seconds);
                _ageLabel.Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Days
                        );

                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = (DateTime.UtcNow - c.Created.UtcDateTime);

                _createdLabel .Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year -1) + " " + Strings.common.Years +")";
                _levelLabel.Text = string.Format(Strings.common.Level, c.Level);
                _mapLabel.Text = DataManager.getMapName(c.map);

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
    public class  TagPanel : FlowPanel
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
    public class BasicContainer : Container
    {
        const int PADDING = 2;
        public Texture2D Texture;

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (Texture == null) Texture = Textures.Backgrounds[(int)_Backgrounds.Tooltip];

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
    public class TagEntry : Panel
    {
        public Character assignedCharacter;
        public Label textLabel;
        public Image deleteButton;
        private TagPanel panel;
        private bool showDeleteButton;

        public TagEntry(string txt, Character character, FlowPanel parent, bool showButton = true, MonoGame.Extended.BitmapFonts.BitmapFont font = default)
        {
            ContentService contentService = new ContentService();
            var textFont = (font == default) ? contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular) : font;
            
            assignedCharacter = character;
            Parent = parent;
            WidthSizingMode = SizingMode.AutoSize;
            HeightSizingMode = SizingMode.AutoSize;
            showDeleteButton = showButton;

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

            if (showDeleteButton)
            {
                deleteButton = new Image()
                {
                    Texture = Textures.Icons[(int)Icons.Close],
                    Parent = panel,
                    //Location = new Point(panel.Width - 32 - 4, -4),
                    Size = new Point(21, 23),
                };
                deleteButton.MouseEntered += delegate { deleteButton.Texture = Textures.Icons[(int)Icons.CloseHovered]; };
                deleteButton.MouseLeft += delegate { deleteButton.Texture = Textures.Icons[(int)Icons.Close]; };
                deleteButton.Click += delegate
                {
                    assignedCharacter.Tags.Remove(Text);
                    this.Dispose();
                };
            }

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
        }

        private string _Text;
        public string Text {
            get { return _Text; }
            set
            {
                _Text = value;
                textLabel.Text = " " + value + " ";            }
        }


    }
    public class CharacterDetailWindow : BasicContainer
    {
        public TextBox tag_TextBox;
        public Image addTag_Button;
        public Label name_Label;
        public Image spec_Image;
        public Label spec_Label;
        public Image separator_Image;
        public FlowPanel customTags_Panel;

        public Character assignedCharacter;
        public void setCharacter(Character c)
        {
            assignedCharacter = c;
            name_Label.Text = c.Name;
            spec_Label.Text = DataManager.getProfessionName(c._Profession);
            spec_Image.Texture = Textures.Professions[c._Profession];
            customTags_Panel.ClearChildren();

            foreach(string tag in c.Tags)
            {
                new TagEntry(tag, c, customTags_Panel);
            }
        }
    }
}