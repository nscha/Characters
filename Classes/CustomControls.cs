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
        public int _State { get; set; }
        public int _MaxState { get; set; }
        public Texture2D[] _Textures;

        public int Toggle()
        {
            _State++;

            if (_State > (_MaxState -1))
            {
                _State = 0;
            }

            if (_Textures.Length >= _State)
            {
                Texture = _Textures[_State];
            }

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
                    Text = c.Race.ToString(),
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
                    Text = "Level " + c.Level,
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
                    Text = "Map Name" + " " + "(PvE)",
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
                    Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year - 1) + " years)",
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
                    Text = string.Format("{3} Days {0:00}:{1:00}:{2:00}",
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
                _ageLabel.Text = string.Format("{3} Days {0:00}:{1:00}:{2:00}",
                        t.Hours,
                        t.Minutes,
                        t.Seconds,
                        t.Days
                        );

                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = (DateTime.UtcNow - c.Created.UtcDateTime);

                _createdLabel .Text = c.Created.ToString("G") + " (" + ((zeroTime + span).Year -1) + " years)";
                if (c.mapName == "" || c.mapName == null)
                {
                    _mapLabel.Text = "Unkown Map"  + " (Unkown Type)";
                }
                else
                {
                    _mapLabel.Text = c.mapName + " (" + c.gameMode + ")";
                }

                _levelLabel.Text = "Level " + c.Level;
            }
        }
    }
}