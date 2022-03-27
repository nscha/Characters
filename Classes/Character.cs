using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kenedia.Modules.Characters
{
    public class CharacterCrafting
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public bool Active { get; set; }
    }

    public class JsonCharacter
    {
        public string Name { get; set; }
        public DateTime lastLogin;
        public DateTime LastModified;
        public DateTimeOffset Created;
        public RaceType Race { get; set; }
        public int Profession { get; set; }
        public int apiIndex { get; set; }
        public int Specialization { get; set; }
        public List<CharacterCrafting> Crafting;
        public int map;
        public int Level { get; set; }
    }

    public class Character
    {
        public ContentsManager contentsManager;
        public Gw2ApiManager apiManager;

        public int _mapid;
        public int _lastmapid;

        public bool logged_In_Once = false;

        public bool loaded = false;
        public List<CharacterCrafting> Crafting;
        public CharacterControl characterControl;
        public Tooltip tooltip;
        public Image classImage;
        public Label nameLabel;
        public Label timeLabel;
        public Image switchButton;
        public Image birthdayImage;
        public List<Image> craftingImages;
        public int apiIndex;
        public DateTimeOffset Created;
        public int map;

        private void MainPanel_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Panel s = (Panel)sender;
            s.BackgroundTexture = null;
        }
        private void MainPanel_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Panel s = (Panel)sender;
            s.BackgroundTexture = Textures.Icons[(int)Icons.RectangleHighlight];
        }
        public void Create_UI_Elements()
        {
            ContentService contentService = new ContentService();

            characterControl = new CharacterControl()
            {
                Parent = Module.CharacterPanel,
                Height = 60,
                Width = Module.CharacterPanel.Width - 20 - 5,
                ShowBorder = true,
                assignedCharacter = this,
                Tooltip = new CharacterTooltip()
                {
                    Parent = characterControl,
                    assignedCharacter = this,
                },
            };
            characterControl.Click += delegate {
                switch (Module.subWindow.Visible)
                {
                    case true:
                        if (!switchButton.MouseOver && Module.subWindow.assignedCharacter == this)
                        {
                            Module.subWindow.Hide();
                        }
                        break;

                    case false:
                        if (!switchButton.MouseOver)
                        {
                            Module.subWindow.Show();

                            if (Module.subWindow.assignedCharacter != this)
                            {
                                Module.subWindow.setCharacter(this);
                            }
                        }
                        break;
                }
            };

            characterControl.MouseEntered += MainPanel_MouseEntered;
            characterControl.MouseLeft += MainPanel_MouseLeft;
            CharacterTooltip tooltp = (CharacterTooltip)characterControl.Tooltip;
            tooltp.Shown += delegate { tooltp._Update(); };

            //Profession Icon
            classImage = new Image()
            {
                Location = new Point(0, 0),
                Texture = getProfessionTexture(),
                Size = new Point(48, 48),
                Parent = characterControl,
                Tooltip = tooltp,
            };

            //Character Name
            nameLabel = new Label()
            {
                Location = new Point(48 + 5, 0),
                Text = Name,
                Parent = characterControl,
                Height = characterControl.Height / 2,
                Width = characterControl.Width - 165,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
                Tooltip = tooltp,
            };

            //Separator
            new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = characterControl,
                Location = new Point(48, (characterControl.Height / 2) - 6),
                Size = new Point(characterControl.Width - 165, 4),
                Tooltip = tooltp,
            };

            //Time since Login
            timeLabel = new Label()
            {
                Location = new Point(48 + 5, characterControl.Height / 2 - 2),
                Text = "00:00:00",
                Parent = characterControl,
                Height = 16,
                Width = characterControl.Width - 165,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
                Tooltip = tooltp,
            };

            //Birthday Image
            birthdayImage = new Image()
            {
                Texture = Textures.Icons[(int)Icons.BirthdayGift],
                Parent = characterControl,
                Location = new Point(characterControl.Width - 150, (characterControl.Height / 2) - 2),
                Size = new Point(20, 20),
                Visible = false,
            };

            //Crafting Professions
            if (Crafting.Count > 0)
            {
                var craftingPanel = new FlowPanel()
                {
                    Location = new Point(characterControl.Width - 45 - 48 - 10, 0),
                    Parent = characterControl,
                    Height = characterControl.Height,
                    Width = 55,
                    FlowDirection = ControlFlowDirection.LeftToRight,
                };
                string ttp = "";

                craftingImages = new List<Image>();
                foreach (CharacterCrafting crafting in Crafting)
                {
                    if (crafting.Active)
                    {
                        craftingImages.Add(new Image()
                        {
                            Texture = Textures.Crafting[crafting.Id],
                            Size = new Point(24, 24),
                            Parent = craftingPanel,
                            Enabled = false,
                        });
                        ttp = ttp + Enum.GetName(typeof(Crafting), crafting.Id) + " (" + crafting.Rating + ")" + Environment.NewLine;
                    }
                }

                ttp = ttp.TrimEnd();

                foreach (Image image in craftingImages)
                {
                    image.BasicTooltipText = ttp;
                }
                craftingPanel.BasicTooltipText = ttp;
            }

            switchButton = new Image()
            {
                Location = new Point(characterControl.Width - 45, 10),
                Texture = Textures.Icons[(int)Icons.Logout],
                Size = new Point(32, 32),
                Parent = characterControl,
                BasicTooltipText = string.Format(Strings.common.Switch, this.Name),
            };
            switchButton.Click += SwitchButton_Click;
            switchButton.MouseEntered += SwitchButton_MouseEntered;
            switchButton.MouseLeft += SwitchButton_MouseLeft;

            tooltp._Create();
            this.loaded = true;


        }

        private void SwitchButton_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            Swap();
        }

        private void SwitchButton_MouseLeft(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            switchButton.Texture = Textures.Icons[(int)Icons.Logout];
        }
        private void SwitchButton_MouseEntered(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            switchButton.Texture = Textures.Icons[(int)Icons.LogoutWhite];
        }
        public async void UpdateCharacter()
        {
            if (loaded && apiManager != null)
            {
                var player = GameService.Gw2Mumble.PlayerCharacter;

                if (Name == player.Name)
                {
                    _mapid = GameService.Gw2Mumble.CurrentMap.Id;
                    if (_mapid > 0 && _mapid != _lastmapid)
                    {
                        _lastmapid = _mapid;
                        this.map = _mapid;

                        CharacterTooltip tooltp = (CharacterTooltip)characterControl.Tooltip;
                        tooltp._Update();
                        Save();
                    }

                    lastLogin = DateTime.UtcNow.AddSeconds(0);
                    LastModified = DateTime.UtcNow.AddSeconds(1);
                    Race = player.Race;
                    birthdayImage.Visible = false;

                    Update_UI_Time();
                    UpdateProfession();
                }
            }
        }

        public Texture2D getProfessionTexture()
        {
            if (_Specialization > 0)
            {
                return Textures.Specializations[_Specialization];
            }
            else if (_Profession <= 9 && _Profession >= 1)
            {
                return Textures.Professions[_Profession];
            }

            return Textures.Icons[(int)Icons.Bug];
        }
        public Texture2D getRaceTexture()
        {
            if (this.Race.ToString() != "")
            {
                return Textures.Races[(int)this.Race];
            }

            return Textures.Icons[(int)Icons.Bug];
        }
        public void UpdateProfession()
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (Name == player.Name)
            {
                bool changed = (_Specialization != player.Specialization || Profession != player.Profession);

                if (changed)
                {
                    if (DataManager._Specializations.Length > player.Specialization && DataManager._Specializations[player.Specialization] != null)
                    {
                        Specialization = (Specializations)player.Specialization;
                        _Specialization = player.Specialization;
                    }
                    else
                    {
                        Specialization = 0;
                        _Specialization = 0;
                    }

                    Profession = player.Profession;
                    _Profession = (int)player.Profession;

                    classImage.Texture = getProfessionTexture();
                    Save();
                }
            }
        }
        public void UpdateTooltips()
        {

        }
        public void Update_UI_Time()
        {
            if (this.loaded)
            {
                this.seconds = Math.Round(DateTime.UtcNow.Subtract(this.lastLogin).TotalSeconds);
                this.UpdateTooltips();

                var t = TimeSpan.FromSeconds(this.seconds);

                if (this.timeLabel != null)
                {
                    this.timeLabel.Text = string.Format("{3} " + Strings.common.Days + " {0:00}:{1:00}:{2:00}",
                    t.Hours,
                    t.Minutes,
                    t.Seconds,
                    t.Days
                    );
                }

                if (!birthdayImage.Visible)
                {
                    for (int i = 1; i < 100; i++)
                    {
                        DateTime birthDay = Created.AddYears(i).DateTime;

                        if (birthDay <= DateTime.UtcNow)
                        {
                            if (birthDay > lastLogin)
                            {
                                birthdayImage.Visible = true;
                                birthdayImage.BasicTooltipText = Name + " had Birthday! They are now " + i + " years old.";
                            }
                        }
                        else
                        {
                            break;
                        };
                    }
                }

                if (characterControl.Tooltip.Visible)
                {
                    CharacterTooltip tooltp = (CharacterTooltip)characterControl.Tooltip;
                    tooltp._Update();
                };
            }
        }
        public void Show()
        {
            if (this.loaded)
            {
                visible = true;
                this.characterControl.Show();
            }
        }
        public void Hide()
        {
            if (this.loaded)
            {
                visible = false;
                this.characterControl.Hide();
            }
        }
        public bool visible = true;
        public void Swap()
        {
            if (!GameService.Gw2Mumble.CurrentMap.Type.IsCompetitive())
            {
                ScreenNotification.ShowNotification(string.Format(Strings.common.Switch, Name), ScreenNotification.NotificationType.Warning);

                if (!GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    for (int i = 0; i < Module.Characters.Count; i++)
                    {
                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                    }

                    foreach (Character c in Module.Characters)
                    {
                        if (c.Name != Name)
                        {
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                        }
                        else
                        {
                            if (Module.Settings.EnterOnSwap.Value) Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                            break;
                        }
                    }
                }
                else if (DateTime.UtcNow.Subtract(Module.lastLogout).TotalSeconds > 1)
                {
                    var mods = Module.Settings.LogoutKey.Value.ModifierKeys;
                    var primary = (VirtualKeyShort)Module.Settings.LogoutKey.Value.PrimaryKey;

                    foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                    {
                        if (mod != ModifierKeys.None && mods.HasFlag(mod))
                        {
                            Blish_HUD.Controls.Intern.Keyboard.Press(Module.ModKeyMapping[(int)mod], false);
                        }
                    }

                    Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, false);

                    foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                    {
                        if (mod != ModifierKeys.None && mods.HasFlag(mod))
                        {
                            Blish_HUD.Controls.Intern.Keyboard.Release(Module.ModKeyMapping[(int)mod], false);
                        }
                    }

                    Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                    Module.lastLogout = DateTime.UtcNow;
                    Module.swapCharacter = this;
                }
            }
            else
            {
                ScreenNotification.ShowNotification(Strings.common.Error_Competivive, ScreenNotification.NotificationType.Error);
            }
        }
        public double seconds { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public RaceType Race { get; set; }

        public ProfessionType Profession { get; set; }
        public int _Profession { get; set; }

        public Specializations Specialization { get; set; }
        public int _Specialization { get; set; }

        public int spec { get; set; }
        public int mapid { get; set; }
        public DateTime lastLogin;
        public DateTime LastModified;
        public Label checkbox;
        public Label timeSince;
        public Image icon;

        public void Save()
        {
            if (Module.API_Account != null)
            {

                List<JsonCharacter> _data = new List<JsonCharacter>();
                foreach (Character c in Module.Characters)
                {
                    JsonCharacter jsonCharacter = new JsonCharacter()
                    {
                        Name = c.Name,
                        Race = c.Race,

                        Specialization = c._Specialization,
                        Profession = c._Profession,
                        Crafting = c.Crafting,
                        lastLogin = c.lastLogin,
                        apiIndex = c.apiIndex,
                        Created = c.Created,
                        LastModified = c.LastModified,
                        map = c.map,
                        Level = c.Level,
                    };


                    _data.Add(jsonCharacter);
                }

                string json = JsonConvert.SerializeObject(_data.ToArray());

                //write string to file
                System.IO.File.WriteAllText(Module.CharactersPath, json);
            }
        }
    }
}
