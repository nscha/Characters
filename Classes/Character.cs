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
        public string Icon { get; set; }

        public DateTime lastLogin;
        public DateTime LastModified;
        public DateTimeOffset Created;

        public RaceType Race { get; set; }
        public int Profession { get; set; }
        public int apiIndex { get; set; }
        public int Specialization { get; set; }
        public List<CharacterCrafting> Crafting;
        public int Map;
        public int Level { get; set; }
        public string Tags;
        public bool loginCharacter;
        public bool include = true;
    }

    public class Character
    {
        public ContentsManager contentsManager;
        public Gw2ApiManager apiManager;

        public int _mapid;
        public int _lastmapid;
        public int Years;

        public bool logged_In_Once = false;

        public bool loaded = false;
        public List<CharacterCrafting> Crafting;
        public CharacterControl characterControl;

        public int apiIndex;
        public DateTimeOffset Created;
        public DateTime NextBirthday;
        public int Map;
        public bool loginCharacter;
        public bool include = true;

        public bool hadBirthdaySinceLogin()
        {
            for (int i = 1; i < 100; i++)
            {
                DateTime birthDay = Created.AddYears(i).DateTime;
                if ((NextBirthday == null || NextBirthday == Module.dateZero) && birthDay >= DateTime.UtcNow)
                {
                    NextBirthday = birthDay;
                    Years = i;
                }

                if (birthDay <= DateTime.UtcNow)
                {
                    if (birthDay > lastLogin)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                };
            }

            return false;
        }

        public void Create_UI_Elements()
        {
            if (!loaded)
            {
                characterControl = new CharacterControl(this)
                {
                    WidthSizingMode = SizingMode.Standard,
                    Parent = Module.CharacterPanel,
                    Width = Module.CharacterPanel.Width - 25,
                };
            }

            loaded = true;
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
                        this.Map = _mapid;

                        CharacterTooltip tooltp = (CharacterTooltip)characterControl.Tooltip;
                        tooltp._Update();
                        Save();
                    }

                    lastLogin = DateTime.UtcNow.AddSeconds(0);
                    LastModified = DateTime.UtcNow.AddSeconds(1);
                    Race = player.Race;

                    characterControl.UpdateUI();

                    Update_UI_Time();
                    UpdateProfession();
                }
            }
        }

        public Texture2D getProfessionTexture(bool includeCustom = true, bool baseIcons = false)
        {
            if (baseIcons)
            {
                if (_Specialization > 0)
                {
                    return Textures.SpecializationsWhite[_Specialization];
                }
                else if (_Profession <= 9 && _Profession >= 1)
                {
                    return Textures.ProfessionsWhite[_Profession];
                }
            }
            else
            {
                if (includeCustom && Icon != null && Icon != "" && Textures.CustomImages != null)
                {
                    foreach (Texture2D Texture in Textures.CustomImages)
                    {
                        if (Texture != null && Texture.Name == Icon) return Texture;
                    }
                }

                if (_Specialization > 0)
                {
                    return Textures.Specializations[_Specialization];
                }
                else if (_Profession <= 9 && _Profession >= 1)
                {
                    return Textures.Professions[_Profession];
                }
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

                    characterControl.UpdateUI();
                    Save();
                }
            }
        }
        public void UpdateLanguage()
        {
            characterControl.UpdateLanguage();
        }
        public void Update_UI_Time()
        {
            if (this.loaded)
            {
                this.seconds = Math.Round(DateTime.UtcNow.Subtract(this.lastLogin).TotalSeconds);
                characterControl.UpdateUI();
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
            if (GameService.Gw2Mumble.PlayerCharacter.Name != Name || !GameService.GameIntegration.Gw2Instance.IsInGame)
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
        }

        public double seconds { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
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
        public List<string> Tags = new List<string>();

        public void Save()
        {
            Module.saveCharacters = true;
        }
    }
}
