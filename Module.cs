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

namespace GW2Characters
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public partial class Module : Blish_HUD.Modules.Module
    {
        DateTime dateZero;
        private static bool requestAPI = true;
        #region Service Managers
        public SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion
        public static string CharactersPath;
        public static string AccountPath;
        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) { }

        #region Global Variables
        private static bool charactersLoaded;
        private static Account API_Account;
        public static FlowPanel CharacterPanel;
        public static FlowPanel filterPanel;
        public static TextBox filterTextBox;
        public static StandardButton clearButton;
        public static Image infoImage;
        public static StandardButton refreshAPI;
        private static List<string> CharacterNames = new List<string>();
        private static List<Character> Characters = new List<Character>();
        private static ToggleImage[] filterProfessions;
        private static ToggleImage[] filterCrafting;
        private static ToggleImage birthdayToggle;
        private static CornerIcon cornerButton;

        private static readonly Logger Logger = Logger.GetLogger<Module>();
        public static StandardWindow MainWidow { get; private set; }

        public static Tooltip tooltip;
        public static StandardWindow ConsoleWindow { get; private set; }
        public static FlowPanel ConsolePanel;
        private static class Active
        {
            public static Character character { get; set; }
        }
        private static class Last
        {
            public static Character character { get; set; }
            public static string CharName { get; set; }
            public static int CharCount { get; set; }
            public static double Tick_Save;
            public static double Tick_Update;
            public static double Tick_APIUpdate;
        }
        private static class Current
        {
            public static Character character { get; set; }
            public static double Tick_Save;
            public static double Tick_Update;
            public static double Tick_APIUpdate;
        }

        static internal SettingEntry<Blish_HUD.Input.KeyBinding> LogoutKey;
        static internal SettingEntry<bool> EnterOnSwap;

        const int WINDOW_WIDTH = 385;
        const int WINDOW_HEIGHT = 920;
        const int TITLEBAR_HEIGHT = 33;

        static VirtualKeyShort[] ModKeyMapping;

        public static Character swapCharacter;
        #endregion
        public static AccountInfo userAccount { get; set; }
        public class AccountInfo
        {
            public string Name;
            public DateTimeOffset LastModified;
            public DateTimeOffset LastBlishUpdate;
            public void Save()
            {
                if (API_Account != null)
                {                    
                    List<AccountInfo> _data = new List<AccountInfo>();
                    _data.Add(userAccount);

                    string json = JsonConvert.SerializeObject(_data.ToArray());

                    //write string to file
                    System.IO.File.WriteAllText(AccountPath, json);
                }
            }
            public bool CharacterUpdateNeeded()
            {
                double lastModified = DateTimeOffset.UtcNow.Subtract(LastModified).TotalSeconds;
                double lastUpdate = DateTimeOffset.UtcNow.Subtract(LastBlishUpdate).TotalSeconds;

                return lastModified > 800 ||(lastUpdate) > lastModified;
            }
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
        public class CharacterCrafting
        {
            public int Id { get; set; }
            public int Rating { get; set; }
            public bool Active { get; set; }
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
            public DateTime lastLogout;
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
                    Parent = CharacterPanel,
                    Height = 60,
                    Width = CharacterPanel.Width - 20 - 5,
                    ShowBorder = true,
                    assignedCharacter = this,
                    Tooltip = new CharacterTooltip()
                    {
                        Parent = characterControl,
                        assignedCharacter = this,
                    },
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
                        Specialization = (Specializations)player.Specialization;
                        _Specialization = player.Specialization;

                        Profession = player.Profession;
                        _Profession = (int)player.Profession;
                        Race = player.Race;
                        birthdayImage.Visible = false;

                        Update_UI_Time();
                        UpdateProfession();
                        Current.character = this;
                    }
                }
            }

            public Texture2D getProfessionTexture()
            {
                if (this._Specialization > 0)
                {
                    return Textures.Specializations[this._Specialization];
                }
                else if (this._Profession <= 9 && this._Profession >= 1)
                {
                    return Textures.Professions[this._Profession];
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

                    Specialization = (Specializations)player.Specialization;
                    _Specialization = player.Specialization;

                    Profession = player.Profession;
                    _Profession = (int)player.Profession;

                    this.classImage.Texture = this.getProfessionTexture();
                    if (changed) Save();
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
                        this.timeLabel.Text = string.Format("{3} " + Strings.common.Days +" {0:00}:{1:00}:{2:00}",
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
                        for (int i = 0; i < Characters.Count; i++)
                        {
                            Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.LEFT, false);
                        }

                        foreach (Character c in Characters)
                        {
                            if (c.Name != Name)
                            {
                                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RIGHT, false);
                            }
                            else
                            {
                                if (EnterOnSwap.Value) Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                                break;
                            }
                        }
                    }
                    else if (DateTime.UtcNow.Subtract(lastLogout).TotalSeconds > 5)
                    {
                        var mods = LogoutKey.Value.ModifierKeys;
                        var primary = (VirtualKeyShort)LogoutKey.Value.PrimaryKey;

                        foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                        {
                            if (mod != ModifierKeys.None && mods.HasFlag(mod))
                            {
                                Blish_HUD.Controls.Intern.Keyboard.Press(ModKeyMapping[(int)mod], false);
                            }
                        }

                        Blish_HUD.Controls.Intern.Keyboard.Stroke(primary, false);

                        foreach (ModifierKeys mod in Enum.GetValues(typeof(ModifierKeys)))
                        {
                            if (mod != ModifierKeys.None && mods.HasFlag(mod))
                            {
                                Blish_HUD.Controls.Intern.Keyboard.Release(ModKeyMapping[(int)mod], false);
                            }
                        }

                        Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
                        lastLogout = DateTime.UtcNow;
                        swapCharacter = this;
                    }
                }
                else
                {
                    ScreenNotification.ShowNotification(Strings.common.Error_Competivive, ScreenNotification.NotificationType.Error);
                }
            }
            public double seconds { get; set; }
            public string Name { get; set; }
            public int Level{ get; set; }
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
                if (API_Account != null)
                {

                    List<JsonCharacter> _data = new List<JsonCharacter>();
                    foreach (Character c in Characters)
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
                    System.IO.File.WriteAllText(CharactersPath, json);
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            LogoutKey = settings.DefineSetting(nameof(LogoutKey),
                                                     new Blish_HUD.Input.KeyBinding(Keys.F12),
                                                     () => Strings.common.Logout,
                                                     () => Strings.common.LogoutDescription);
            EnterOnSwap = settings.DefineSetting("ShowInTaskbar",
                                                              false,
                                                              () => Strings.common.LoginAfterSelect,
                                                              () => Strings.common.LoginAfterSelect);
        }
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        protected override void Initialize()
        {
            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;
            Array.AsReadOnly(ModKeyMapping);

            Logger.Debug("Initializing ...");
            LoadTextures();
            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;
            AccountPath = DirectoriesManager.GetFullDirectoryPath("gw2characters") + @"\accounts.json";


            DataManager.ContentsManager = ContentsManager;
            DataManager.Load();

            Logger.Debug("RACE CHECK");
            Logger.Debug(DataManager.getRaceName(1));
            Logger.Debug(DataManager.getRaceName("Human"));
            Logger.Debug("Map Check: " + DataManager.getMapName("Interior Harathi"));
            Logger.Debug("Map Check: " + DataManager.getMapName(1464));
            Logger.Debug("Map Check: " + DataManager.getMapName(1465));

        }

        private void LoadTextures()
        {
            Logger.Debug("Loading Textures....");          

            Textures.Backgrounds = new Texture2D[Enum.GetValues(typeof(_Backgrounds)).Cast<int>().Max() + 1];
            foreach (_Backgrounds background in Enum.GetValues(typeof(_Backgrounds)))
            {
                Textures.Backgrounds[(int)background] = ContentsManager.GetTexture(@"textures\backgrounds\" + (int)background + ".png");
            }

            Textures.Emblems= new Texture2D[Enum.GetValues(typeof(_Emblems)).Cast<int>().Max() + 1];
            foreach (_Backgrounds emblem in Enum.GetValues(typeof(_Emblems)))
            {
                Textures.Emblems[(int)emblem] = ContentsManager.GetTexture(@"textures\emblems\" + (int)emblem + ".png");
            }
            
            Textures.Icons = new Texture2D[Enum.GetValues(typeof(Icons)).Cast<int>().Max() + 1];
            foreach (Icons icon in Enum.GetValues(typeof(Icons)))
            {
                Textures.Icons[(int)icon] = ContentsManager.GetTexture(@"textures\icons\" + (int)icon + ".png");
            }
           
            Textures.Races = new Texture2D[6];
            foreach (RaceType race in Enum.GetValues(typeof(RaceType)))
            {
                Textures.Races[(int)race] = ContentsManager.GetTexture(@"textures\races\" + race + ".png");
            }

            Textures.Professions = new Texture2D[Enum.GetValues(typeof(Professions)).Cast<int>().Max() + 1];
            Textures.ProfessionsDisabled = new Texture2D[Enum.GetValues(typeof(Professions)).Cast<int>().Max() + 1];
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                Textures.Professions[(int)profession] = ContentsManager.GetTexture(@"textures\professions\" + (int)profession + ".png");
                Textures.ProfessionsDisabled[(int)profession] = ContentsManager.GetTexture(@"textures\professions gray\" + (int)profession + ".png");
            }

            Textures.Specializations = new Texture2D[Enum.GetValues(typeof(Specializations)).Cast<int>().Max() + 1];
            foreach (Specializations specialization in Enum.GetValues(typeof(Specializations)))
            {
                Textures.Specializations[(int)specialization] = ContentsManager.GetTexture(@"textures\specializations\" + (int)specialization + ".png");
            }

            Textures.Crafting = new Texture2D[Enum.GetValues(typeof(Crafting)).Cast<int>().Max() + 1];
            Textures.CraftingDisabled = new Texture2D[Enum.GetValues(typeof(Crafting)).Cast<int>().Max() + 1];
            foreach (Crafting crafting in Enum.GetValues(typeof(Crafting)))
            {
                Textures.Crafting[(int)crafting] = ContentsManager.GetTexture(@"textures\crafting\" + (int)crafting + ".png");
                Textures.CraftingDisabled[(int)crafting] = ContentsManager.GetTexture(@"textures\crafting gray\" + (int)crafting + ".png");
            }
        }

        public async void FetchAPI(bool force = false)
        {
            if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }) && API_Account != null && userAccount != null)
            {
                //ScreenNotification.ShowNotification("Updating Account ....", ScreenNotification.NotificationType.Warning);
                var account = await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();

                userAccount.LastModified = account.LastModified;
                userAccount.Save();

                ///character.LastModified older than account.LastModified --> character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j)
                ///character.LastModified younger than account.LastModified --> character.LastModified = character.LastModified

                if (userAccount.CharacterUpdateNeeded() || force)
                {
                    userAccount.LastBlishUpdate = userAccount.LastBlishUpdate > account.LastModified ? userAccount.LastBlishUpdate : account.LastModified;
                    userAccount.Save();

                    var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                    Logger.Debug("Updating Characters ....");
                    //ScreenNotification.ShowNotification("Updating Characters ....", ScreenNotification.NotificationType.Warning);
                    Character last = null;
                    int j = 0;

                    foreach (Gw2Sharp.WebApi.V2.Models.Character c in characters)
                    {
                        Character character = getCharacter(c.Name);

                        character.Name = character.Name ?? c.Name;
                        character.Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race);
                        character._Profession = (int)Enum.Parse(typeof(Professions), c.Profession.ToString());
                        character.Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession.ToString());
                        character._Specialization = character._Specialization > -1 ? character._Specialization : -1;
                        character.Level = c.Level;
                        character.Created = c.Created;
                        character.contentsManager = ContentsManager;
                        character.apiManager = Gw2ApiManager;

                        character.Crafting = new List<CharacterCrafting>();

                        foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                        {
                            character.Crafting.Add(new CharacterCrafting()
                            {
                                Id = (int)disc.Discipline.Value,
                                Rating = disc.Rating,
                                Active = disc.Active,
                            });
                        }
                        character.apiIndex = j;

                        if (character.LastModified == dateZero || character.LastModified < account.LastModified.UtcDateTime)
                        {
                            character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j);
                        }

                        if (character.lastLogin == dateZero)
                        {
                            character.lastLogin = c.LastModified.UtcDateTime;
                        }

                        last = character;
                        j++;
                    }

                    if (last != null) last.Save();

                    UpdateCharacters();
                    //ScreenNotification.ShowNotification("Characters Updated!", ScreenNotification.NotificationType.Warning);
                    Logger.Debug("Characters Updated!");
                }

                double lastModified = DateTimeOffset.UtcNow.Subtract(userAccount.LastModified).TotalSeconds;
                double lastUpdate = DateTimeOffset.UtcNow.Subtract(userAccount.LastBlishUpdate).TotalSeconds;
                infoImage.BasicTooltipText = "Last Modified: " + Math.Round(lastModified) + Environment.NewLine + "Last Blish Login: " + Math.Round(lastUpdate);
            }
            else
            {
                ScreenNotification.ShowNotification(Strings.common.Error_Competivive, ScreenNotification.NotificationType.Error);
                Logger.Error("This API Token has not the required permissions!");
            }
        }
        private async void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            Logger.Debug("API Subtoken Updated!");

            if (Gw2ApiManager.HasPermissions(new[] { TokenPermission.Account, TokenPermission.Characters }))
            {
                var account = await Gw2ApiManager.Gw2ApiClient.V2.Account.GetAsync();


                Logger.Debug("Account Age: " + account.Age.TotalSeconds + " seconds");
                Logger.Debug("LastModified: " + account.LastModified);

                API_Account = account;
                string path = DirectoriesManager.GetFullDirectoryPath("gw2characters") + @"\" + API_Account.Name;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                CharactersPath = path + @"\characters.json";
                AccountPath = path + @"\account.json";

                if (userAccount == null)
                {
                    userAccount = new AccountInfo()
                    {
                        Name = account.Name,
                        LastModified = account.LastModified,
                    };
                }

                if (System.IO.File.Exists(AccountPath))
                {
                    requestAPI = false;
                    List<AccountInfo> accountInfos = JsonConvert.DeserializeObject<List<AccountInfo>>(System.IO.File.ReadAllText(AccountPath));

                    foreach (AccountInfo acc in accountInfos)
                    {
                        if (acc.Name == account.Name)
                        {
                            userAccount.LastBlishUpdate = acc.LastBlishUpdate;
                            break;
                        }
                    }
                }

                LoadCharacterList();

                if (userAccount.CharacterUpdateNeeded())
                {
                    userAccount.LastBlishUpdate = account.LastModified;
                    userAccount.Save();

                    Logger.Debug("Updating Characters ....");
                    //ScreenNotification.ShowNotification("Updating Characters!", ScreenNotification.NotificationType.Warning);
                    Logger.Debug("The last API modification is more recent than our last local data track.");
                    var characters = await Gw2ApiManager.Gw2ApiClient.V2.Characters.AllAsync();
                    Character last = null;
                    int j = 0;

                    foreach (Gw2Sharp.WebApi.V2.Models.Character c in characters)
                    {
                        Character character = getCharacter(c.Name);
                        character.Name = character.Name ?? c.Name;
                        character.Race = (RaceType)Enum.Parse(typeof(RaceType), c.Race);
                        character._Profession = (int)Enum.Parse(typeof(Professions), c.Profession.ToString());
                        character.Profession = (ProfessionType)Enum.Parse(typeof(ProfessionType), c.Profession.ToString());
                        character._Specialization = character._Specialization > -1 ? character._Specialization : -1;
                        character.Level = c.Level;
                        character.Created = c.Created;
                        character.apiIndex = j;

                        if (character.LastModified == dateZero || character.LastModified < account.LastModified.UtcDateTime)
                        {
                            character.LastModified = account.LastModified.UtcDateTime.AddSeconds(-j);
                        }

                        if (character.lastLogin == dateZero)
                        {
                            character.lastLogin = c.LastModified.UtcDateTime;
                        }

                        character.contentsManager = ContentsManager;
                        character.apiManager = Gw2ApiManager;

                        character.Crafting = new List<CharacterCrafting>();

                        foreach (CharacterCraftingDiscipline disc in c.Crafting.ToList())
                        {
                            character.Crafting.Add(new CharacterCrafting()
                            {
                                Id = (int)disc.Discipline.Value,
                                Rating = disc.Rating,
                                Active = disc.Active,
                            });
                        }


                        if (!CharacterNames.Contains(c.Name))
                        {
                            CharacterNames.Add(c.Name);
                            Characters.Add(character);
                        }

                        last = character;
                        j++;
                    }

                    if (last != null) last.Save();
                    //ScreenNotification.ShowNotification("Characters Updated!", ScreenNotification.NotificationType.Warning);
                }

                var player = GameService.Gw2Mumble.PlayerCharacter;
                foreach (Character character in Characters)
                {
                    Logger.Debug("Adding UI Elements for " + character.Name);
                    character.Create_UI_Elements();
                    if (player != null && player.Name == character.Name) Current.character = character;
                }

                charactersLoaded = true;
                UpdateCharacters();

                double lastModified = DateTimeOffset.UtcNow.Subtract(userAccount.LastModified).TotalSeconds;
                double lastUpdate = DateTimeOffset.UtcNow.Subtract(userAccount.LastBlishUpdate).TotalSeconds;
                infoImage.BasicTooltipText = "Last Modified: " + Math.Round(lastModified) + Environment.NewLine + "Last Blish Login: " + Math.Round(lastUpdate);
            }
            else
            {
                ScreenNotification.ShowNotification(Strings.common.Error_InvalidPermissions, ScreenNotification.NotificationType.Error);
                Logger.Error("This API Token has not the required permissions!");
                // You don't actually have permission
            }
        }
        private Character getCharacter(string name)
        {
            foreach (Character c in Characters)
            {
                if (c.Name == name) return c;
            }

            return new Character();
        }

        private void CreateWindow()
        {
            var _textureWindowBackground = Textures.Backgrounds[(int)_Backgrounds.MainWindow];

            Module.MainWidow = new StandardWindow(
                _textureWindowBackground,
                new Microsoft.Xna.Framework.Rectangle(15, 45, WINDOW_WIDTH - 20, WINDOW_HEIGHT),
                new Microsoft.Xna.Framework.Rectangle(10, 15, WINDOW_WIDTH, WINDOW_HEIGHT)
                )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Characters",
                Emblem = Textures.Emblems[(int)_Emblems.Characters],
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"CharactersWindow",
                //CanResize = true,
                //BackgroundColor = Microsoft.Xna.Framework.Color.AliceBlue,
            };

            infoImage = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Info],
                Size = new Point(32, 32),
                Location = new Point(Module.MainWidow.Width - 30, -10),
                Parent = Module.MainWidow,
            };

            refreshAPI = new StandardButton()
            {
                Size = new Point(185, 25),
                Location = new Point(175, 0),
                Parent = Module.MainWidow,
                Text = "Refresh API Data",
                Visible = false,
            };
            refreshAPI.Click += RefreshAPI_Click;

            filterTextBox = new TextBox()
            {
                PlaceholderText = "Search for ...",
                Size = new Point(313 - 8 - 23, 30),
                Font = GameService.Content.DefaultFont16,
                Location = new Point(10, 20),
                Parent = Module.MainWidow,
                BasicTooltipText = "'-c CraftingDiscipline'" + Environment.NewLine + "'-p Profession/Specialization'" + Environment.NewLine + "'-r Race'" + Environment.NewLine + "'-b(irthday)'" + Environment.NewLine + "'-c Chef; -p Warrior' will show all warriors and all chefs"
            };

            Tooltip tt = new Tooltip();
            filterTextBox.TextChanged += delegate { UpdateCharacters(); };

            clearButton = new StandardButton()
            {
                Text = "Clear",
                Location = new Point(290 + 2, 19),
                Size = new Point(73, 32),
                Parent = MainWidow,
                ResizeIcon = true,
            };
            void reset() {
                foreach (ToggleImage toggle in filterProfessions)
                {
                    if (toggle != null && toggle._State != 1)
                    {
                        toggle._State = 1;
                    }
                }

                foreach (ToggleImage toggle in filterCrafting)
                {
                    if (toggle != null)
                    {
                        toggle._State = 1;
                    }
                }

                birthdayToggle._State = 0;
                filterTextBox.Text = null;
                UpdateCharacters();
            }
            clearButton.Click += delegate { reset(); };

            filterPanel = new FlowPanel()
            {
                Parent = Module.MainWidow,
                Size = new Point(WINDOW_WIDTH - 56, 60),
                Location = new Point(22, 68 - 16 + 3),
                FlowDirection = ControlFlowDirection.LeftToRight,
            };

            filterProfessions = new ToggleImage[Textures.Professions.Length];
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                filterProfessions[(int)profession] = new ToggleImage() {
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(32, 32),
                    Texture = Textures.Professions[(int)profession],
                    Parent = filterPanel,
                    Id = (int) profession,
                };

                filterProfessions[(int)profession]._Textures = new Texture2D[2];
                filterProfessions[(int)profession]._Textures[0] = Textures.ProfessionsDisabled[(int)profession];
                filterProfessions[(int)profession]._Textures[1] = Textures.Professions[(int)profession];

                filterProfessions[(int)profession].Click += delegate { filterProfessions[(int)profession].Toggle(); UpdateCharacters(); };
            }
            
            birthdayToggle = new ToggleImage()
            {
                isActive = false,
                Size = new Point(32, 32),
                Texture = Textures.Icons[(int) Icons.BirthdayGiftGray],
                Parent = filterPanel,
                _State = 0,
                _MaxState = 3,
            };
            birthdayToggle._Textures = new Texture2D[3];
            birthdayToggle._Textures[0] = Textures.Icons[(int)Icons.BirthdayGiftGray];
            birthdayToggle._Textures[1] = Textures.Icons[(int)Icons.BirthdayGift];
            birthdayToggle._Textures[2] = Textures.Icons[(int)Icons.BirthdayGiftOpen];

            birthdayToggle.Click += delegate { birthdayToggle.Toggle(); UpdateCharacters(); };

            new Label()
            {
                Text = "",
                Size = new Point(16, 32),
                Parent = filterPanel,
                Visible = false,
            };

            filterCrafting = new ToggleImage[Textures.Crafting.Length];
            foreach (Crafting crafting in Enum.GetValues(typeof(Crafting)))
            {
                if (crafting != Crafting.Unknown)
                {
                    filterCrafting[(int)crafting] = new ToggleImage()
                    {
                        _State = 1,
                        _MaxState = 2,
                        Size = new Point(32, 32),
                        Texture = Textures.Crafting[(int)crafting],
                        Parent = filterPanel,
                        Id = (int)crafting,
                    };

                    filterCrafting[(int)crafting]._Textures = new Texture2D[2];
                    filterCrafting[(int)crafting]._Textures[0] = Textures.CraftingDisabled[(int)crafting];
                    filterCrafting[(int)crafting]._Textures[1] = Textures.Crafting[(int)crafting];

                    filterCrafting[(int)crafting].Click += delegate { filterCrafting[(int)crafting].Toggle(); UpdateCharacters(); };
                }
            }

            filterCrafting[(int)Crafting.Unknown] = new ToggleImage()
            {
                _State = 1,
                _MaxState = 2,
                Size = new Point(32, 32),
                Texture = Textures.Crafting[(int)Crafting.Unknown],
                Parent = filterPanel,
                Id = (int)Crafting.Unknown,
            };

            filterCrafting[(int)Crafting.Unknown]._Textures = new Texture2D[2];
            filterCrafting[(int)Crafting.Unknown]._Textures[0] = Textures.CraftingDisabled[(int)Crafting.Unknown];
            filterCrafting[(int)Crafting.Unknown]._Textures[1] = Textures.Crafting[(int)Crafting.Unknown];

            filterCrafting[(int)Crafting.Unknown].Click += delegate { filterCrafting[(int)Crafting.Unknown].Toggle(); UpdateCharacters(); };

 
            CharacterPanel = new FlowPanel()
            {
                CanScroll = true,
                ShowBorder = true,
                Parent = Module.MainWidow,
                Size = new Point(MainWidow.Width, MainWidow.Height - 161),
                Location = new Point(0, 140 - 16 - 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,             
            };

            Module.MainWidow.Show();            
        }


        private void RefreshAPI_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            FetchAPI(true);
        }

        protected override async Task LoadAsync()
        {
            Logger.Debug("Load Async ...");
            cornerButton = new CornerIcon()
            {
                IconName = "Characters",
                Icon = Textures.Icons[(int)Icons.People],
                HoverIcon = Textures.Icons[(int)Icons.PeopleWhite],
                Priority = 4
            };
            cornerButton.Click += delegate { MainWidow.ToggleWindow(); };
        }

        public bool updatePanel = false;
        private void UpdateCharacters()
        {
            if (charactersLoaded)
            {
                CharacterPanel.SortChildren<CharacterControl>((a, b) => b.assignedCharacter.LastModified.CompareTo(a.assignedCharacter.LastModified));
                Characters.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

                string txt = filterTextBox.Text.ToLower();

                string[] parts = txt.Split(';');
                List<string> mapFilters = new List<string>();
                List<string> craftingFilters = new List<string>();
                List<string> professionFilters = new List<string>();
                List<string> birthdayFilters = new List<string>();
                List<string> raceFilters = new List<string>();
                List<string> nameFilters = new List<string>();
                List<string> filterParts = new List<string>();
                foreach (string part in parts)
                {
                    filterParts.Add(part.Trim().ToLower());
                }

                foreach (string part in filterParts)
                {
                    switch (part)
                    {
                        //Crafting filter
                        case string s when s.StartsWith("-c "):
                            craftingFilters.Add(s.ToLower().Replace("-c ", "").Trim());
                            break;

                        //Profession filter
                        case string s when s.StartsWith("-p "):
                            professionFilters.Add(s.ToLower().Replace("-p ", "").Trim());
                            break;

                        //Map filter
                        case string s when s.StartsWith("-m "):
                            mapFilters.Add(s.ToLower().Replace("-m ", "").Trim());
                            break;

                        //Race filter
                        case string s when s.StartsWith("-r "):
                            raceFilters.Add(s.ToLower().Replace("-r ", "").Trim());
                            break;

                        //Race filter
                        case string s when s.StartsWith("-b"):
                            birthdayFilters.Add(s.ToLower().Replace("-b ", "").Trim());
                            break;

                        //Name filter
                        default:
                            nameFilters.Add(part.ToLower().Trim());
                            break;
                    }
                }

                bool matchingFilterString(Character c)
                {
                    if (txt == "") return true;

                    foreach (string s in craftingFilters)
                    {
                        foreach (CharacterCrafting crafting in c.Crafting)
                        {
                            if (crafting.Active)
                            {
                                if (Enum.GetName(typeof(Crafting), crafting.Id).ToLower().Contains(s)) return true;
                            }
                        }
                    }

                    foreach (string s in professionFilters)
                    {
                        var profName = DataManager.getProfessionName(c._Profession);
                        var specName = DataManager.getSpecName(c._Specialization);

                        if (profName.ToLower().Contains(s)) return true;
                        if (c._Specialization > 0 && specName.ToLower().Contains(s)) return true;
                    }

                    foreach (string s in birthdayFilters)
                    {
                        if (c.birthdayImage.Visible) return true;
                    }

                    foreach (string s in raceFilters)
                    {
                        var race = DataManager.getRaceName(c.Race.ToString());
                        if (race.ToString().ToLower().Contains(s)) return true;
                    }

                    foreach (string s in mapFilters)
                    {
                        var map = DataManager.getMapName(c.map);
                        if (map != null && map.ToLower().Contains(s)) return true;
                    }

                    foreach (string s in nameFilters)
                    {
                        if (c.Name.ToLower().Contains(s)) return true;
                    }

                    return false;
                }
                bool matchingToggles(Character c)
                {
                    bool professionMatch = false;
                    bool craftingMatch = false;
                    bool birthdayMatch = false;

                    foreach (ToggleImage toggle in filterProfessions)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == (int)c._Profession) professionMatch = true;
                    }

                    foreach (ToggleImage toggle in filterCrafting)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == 0 && c.Crafting.Count == 0) { craftingMatch = true; break; };

                        if (toggle != null && toggle._State == 1)
                        {
                            foreach (CharacterCrafting crafting in c.Crafting)
                            {
                                if (crafting.Active && toggle.Id == crafting.Id) craftingMatch = true;
                            }
                        }
                    }

                    switch (birthdayToggle._State)
                    {
                        case 0:
                            birthdayMatch = true;
                            break;
                        case 1:
                            birthdayMatch = c.birthdayImage.Visible == true;
                            break;
                        case 2:
                            birthdayMatch = c.birthdayImage.Visible == false;
                            break;
                    }
                    return craftingMatch && professionMatch && birthdayMatch;
                }

                foreach (Character character in Characters)
                {
                    if (matchingFilterString(character) && matchingToggles(character))
                    {
                        character.Show();
                    }
                    else
                    {
                        character.Hide();
                    }
                }

                CharacterPanel.Invalidate();
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {

            // Base handler must be called
            base.OnModuleLoaded(e);
            CreateWindow();

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += Player_SpecializationChanged;
            GameService.GameIntegration.Gw2Instance.IsInGameChanged += Gw2Instance_IsInGameChanged;
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMap_MapChanged;
            OverlayService.Overlay.UserLocaleChanged += delegate { Load_UserLocale(); };
            Load_UserLocale();
        }

        private void Load_UserLocale()
        {
            foreach (Character c in Characters) { 
                CharacterTooltip ttp = (CharacterTooltip) c.characterControl.Tooltip;

                ttp._mapLabel.Text = DataManager.getMapName(c.map);
                ttp._raceLabel.Text = DataManager.getRaceName(c.Race.ToString());

                c.switchButton.BasicTooltipText = string.Format(Strings.common.Switch, c.Name);
            }

            filterTextBox.PlaceholderText = Strings.common.SearchFor;
            clearButton.Text = Strings.common.Clear;

            foreach(ToggleImage toggle in filterProfessions)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getProfessionName(toggle.Id);
                }
            }

            foreach (ToggleImage toggle in filterCrafting)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getCraftingName(Enum.GetName(typeof(Crafting), toggle.Id));
                    if (toggle.Id == 0) toggle.BasicTooltipText = Strings.common.NoCraftingProfession;
                }
            }

            birthdayToggle.BasicTooltipText = Strings.common.Birthday;
        }

        private async void CurrentMap_MapChanged(object sender, ValueEventArgs<int> e)
        {

        }

        private void LoadCharacterList()
        {
            if (System.IO.File.Exists(CharactersPath))
            {
                requestAPI = false;
                List<JsonCharacter> characters = JsonConvert.DeserializeObject<List<JsonCharacter>>(System.IO.File.ReadAllText(CharactersPath));

                foreach (JsonCharacter c in characters)
                {
                    Character character = new Character()
                    {
                        contentsManager = ContentsManager,

                        Race = c.Race,
                        Name = c.Name,
                        lastLogin = c.lastLogin,
                        _Profession = c.Profession,
                        _Specialization = c.Specialization,
                        Crafting = c.Crafting,
                        apiIndex = c.apiIndex,
                        Created = c.Created,
                        LastModified = c.LastModified,
                        map = c.map,
                        Level = c.Level,
                    };

                    Characters.Add(character);
                    CharacterNames.Add(character.Name);
                }
            }
        }

        private void Player_SpecializationChanged(object sender, ValueEventArgs<int> e)
        {
            if (Current.character != null)
            {
                Current.character.UpdateProfession();
                UpdateCharacters();
            };
        }

        protected override void Update(GameTime gameTime)
        {
            Last.Tick_Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_Update += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_APIUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (charactersLoaded && Last.Tick_Update > 250)
            {
                Last.Tick_Update = -250;

                if (Current.character != null && GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    Current.character.UpdateCharacter();
                }

                foreach (Character character in Characters)
                {
                    if (character.characterControl.Visible)
                    {
                        character.Update_UI_Time();
                    }
                }

                if (swapCharacter != null && !GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    swapCharacter.Swap();
                    swapCharacter = null;
                }
            }

            if (Last.Tick_Save > 15000 && userAccount != null)
            {
                Last.Tick_Save = -15000;

            }

            if (Last.Tick_APIUpdate > 30000 && userAccount != null)
            {
                Logger.Debug("Check GW2 API for Updates.");
                Last.Tick_APIUpdate = -30000;
                FetchAPI();
            }
        }

        private void Gw2Instance_IsInGameChanged(object sender, ValueEventArgs<bool> e)
        {
            if (e.Value)
            {
                if (userAccount != null)
                {
                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    Character c = getCharacter(player.Name);
                    c.UpdateCharacter();
                    c.apiIndex = 0;

                    int i = 1;
                    foreach (Character character in Characters)
                    {
                        if (character != c)
                        {
                            character.apiIndex = i;
                            i++;
                        }
                    }
                    UpdateCharacters();
                    c.Save();

                    userAccount.LastBlishUpdate = DateTimeOffset.UtcNow;
                    userAccount.Save();
                }
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }

    }
}
