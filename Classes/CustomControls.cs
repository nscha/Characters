using Blish_HUD.Controls;
using Gw2Sharp.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace GW2Characters
{
    public class CharacterControl : Panel
    {
        public Module.Character assignedCharacter;
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
        public Module.Character assignedCharacter;        
    }
    public class CharacterTooltip : Tooltip
    {
        public Module.Character assignedCharacter;

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
                }
        }
    }
}