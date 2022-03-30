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
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using static Blish_HUD.GameService;
using Blish_HUD.Input;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public partial class Module : Blish_HUD.Modules.Module
    {
        internal static Module ModuleInstance;
        public static DateTime dateZero;
        public static DateTime lastLogout;
        private static bool requestAPI = true;
        public static bool filterCharacterPanel = true;
        #region Service Managers
        public SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        public ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        public DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        public Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        public ContentService contentService = new ContentService();
        #endregion
        public static string CharactersPath;
        public static string AccountPath;
        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters) {
            ModuleInstance = this;
        }

        #region Global Variables
        public static _Settings Settings = new _Settings();

        //GUI
        public static StandardWindow MainWidow { get; private set; }

        public static FlowPanel CharacterPanel;
        public static FlowPanel filterPanel;
        public static TextBox filterTextBox;
        public static StandardButton clearButton;
        public static Image infoImage;
        public static StandardButton refreshAPI;
        public static CornerIcon cornerButton;
        public static List<ToggleIcon> filterProfessions = new List<ToggleIcon>();
        public static List<ToggleIcon> filterCrafting = new List<ToggleIcon>();
        public static List<ToggleIcon> filterRaces = new List<ToggleIcon>();
        public static List<ToggleIcon> filterSpecs = new List<ToggleIcon>();
        public static List<ToggleIcon> filterBaseSpecs = new List<ToggleIcon>();
        public static FilterWindow filterWindow { get; private set; }
        public static Label racesLabel;
        public static Label specializationLabel;
        public static Label customTagsLabel;
        public static FlowPanel filterTagsPanel;

        public static CharacterDetailWindow subWindow { get; private set; }

        //States
        public static bool charactersLoaded;
        public static bool saveCharacters;
        public static bool loginCharacter_Swapped;
        public static bool showAllCharacters;

        public static Character loginCharacter;
        private static class Last
        {
            public static Character character { get; set; }
            public static string CharName { get; set; }
            public static int CharCount { get; set; }
            public static double Tick_PanelUpdate;
            public static double Tick_Save;
            public static double Tick_Update;
            public static double Tick_APIUpdate;
            public static double Tick_FadeEffect;
        }
        private static class Current
        {
            public static Character character { get; set; }
        }

        public static Account API_Account;
        public static AccountInfo userAccount { get; set; }
        public static List<string> CharacterNames = new List<string>();
        public static List<Character> Characters = new List<Character>();
        public static List<string> Tags = new List<string>();
        public static List<TagEntry> TagEntries = new List<TagEntry>();
       
        public static readonly Logger Logger = Logger.GetLogger<Module>();

        const int WINDOW_WIDTH = 385;
        const int WINDOW_HEIGHT = 920;
        const int TITLEBAR_HEIGHT = 33;

        public static VirtualKeyShort[] ModKeyMapping;

        public static Character swapCharacter;
        #endregion


        protected override void Initialize()
        {
            Logger.Debug("Initializing ...");

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            LoadTextures();
            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;
            AccountPath = DirectoriesManager.GetFullDirectoryPath("characters") + @"\accounts.json";

            DataManager.ContentsManager = ContentsManager;
            DataManager.Load();

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += OnKeyPressed_ToggleMenu;
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
            Textures.RacesDisabled = new Texture2D[6];
            foreach (RaceType race in Enum.GetValues(typeof(RaceType)))
            {
                Textures.Races[(int)race] = ContentsManager.GetTexture(@"textures\races\" + race + ".png");
                Textures.RacesDisabled[(int)race] = ContentsManager.GetTexture(@"textures\races gray\" + race + ".png");
            }

            Textures.Professions = new Texture2D[Enum.GetValues(typeof(Professions)).Cast<int>().Max() + 1];
            Textures.ProfessionsDisabled = new Texture2D[Enum.GetValues(typeof(Professions)).Cast<int>().Max() + 1];
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                Textures.Professions[(int)profession] = ContentsManager.GetTexture(@"textures\professions\" + (int)profession + ".png");
                Textures.ProfessionsDisabled[(int)profession] = ContentsManager.GetTexture(@"textures\professions gray\" + (int)profession + ".png");
            }

            Textures.Specializations = new Texture2D[Enum.GetValues(typeof(Specializations)).Cast<int>().Max() + 1];
            Textures.SpecializationsDisabled = new Texture2D[Enum.GetValues(typeof(Specializations)).Cast<int>().Max() + 1];
            foreach (Specializations specialization in Enum.GetValues(typeof(Specializations)))
            {
                Textures.Specializations[(int)specialization] = ContentsManager.GetTexture(@"textures\specializations\" + (int)specialization + ".png");
                Textures.SpecializationsDisabled[(int)specialization] = ContentsManager.GetTexture(@"textures\specializations gray\" + (int)specialization + ".png");
            }

            Textures.Crafting = new Texture2D[Enum.GetValues(typeof(Crafting)).Cast<int>().Max() + 1];
            Textures.CraftingDisabled = new Texture2D[Enum.GetValues(typeof(Crafting)).Cast<int>().Max() + 1];
            foreach (Crafting crafting in Enum.GetValues(typeof(Crafting)))
            {
                Textures.Crafting[(int)crafting] = ContentsManager.GetTexture(@"textures\crafting\" + (int)crafting + ".png");
                Textures.CraftingDisabled[(int)crafting] = ContentsManager.GetTexture(@"textures\crafting gray\" + (int)crafting + ".png");
            }
        }
        private void Load_UserLocale()
        {
            foreach (Character c in Characters)
            {
                CharacterTooltip ttp = (CharacterTooltip)c.characterControl.Tooltip;

                ttp._mapLabel.Text = DataManager.getMapName(c.map);
                ttp._raceLabel.Text = DataManager.getRaceName(c.Race.ToString());

                c.switchButton.BasicTooltipText = string.Format(Strings.common.Switch, c.Name);
            }

            filterTextBox.PlaceholderText = Strings.common.SearchFor;
            filterTextBox.BasicTooltipText = Strings.common.SearchGuide;

            clearButton.Text = Strings.common.Clear;

            subWindow.loginCharacter.Text = Strings.common.LoginCharacter;

            filterWindow.CustomTags.Text = Strings.common.CustomTags;
            filterWindow.Utility.Text = Strings.common.Utility;
            filterWindow.Crafting.Text = Strings.common.CraftingProfession;
            filterWindow.Profession.Text = Strings.common.Profession;
            filterWindow.Specialization.Text = Strings.common.Specialization;
            filterWindow.visibleToggle.BasicTooltipText = Strings.common.ToggleVisible;
            filterWindow.birthdayToggle.BasicTooltipText = Strings.common.Birthday;
            filterWindow.toggleSpecsButton.Text = Strings.common.ToggleAll;

            foreach (Character c in Characters)
            {
                c.UpdateLanguage();
            }

            foreach (ToggleIcon toggle in filterProfessions)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getProfessionName(toggle.Id);
                }
            }

            foreach (ToggleIcon toggle in filterBaseSpecs)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getProfessionName(toggle.Id);
                }
            }

            foreach (ToggleIcon toggle in filterCrafting)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getCraftingName(toggle.Id);
                    if (toggle.Id == 0) toggle.BasicTooltipText = Strings.common.NoCraftingProfession;
                }
            }

            foreach (ToggleIcon toggle in filterSpecs)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getSpecName(toggle.Id);
                }
            }

            foreach (ToggleIcon toggle in filterRaces)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getRaceName(toggle.Id);
                }
            }
        }
        private bool LoadCharacterList()
        {
            try
            {
                Logger.Debug("Character File exists: " + System.IO.File.Exists(CharactersPath));
                if (System.IO.File.Exists(CharactersPath))
                {
                    requestAPI = false;
                    List<JsonCharacter> characters = JsonConvert.DeserializeObject<List<JsonCharacter>>(System.IO.File.ReadAllText(CharactersPath));

                    foreach (JsonCharacter c in characters)
                    {
                        Character character = new Character()
                        {
                            contentsManager = ContentsManager,
                            apiManager = Gw2ApiManager,

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
                            Tags = c.Tags != null && c.Tags != "" ? c.Tags.Split('|').ToList() : new List<string>(),
                            loginCharacter = c.loginCharacter,
                            include = c.include,
                        };

                        foreach (string tag in character.Tags)
                        {
                            if (!Tags.Contains(tag)) Tags.Add(tag);
                        }

                        Characters.Add(character);
                        CharacterNames.Add(character.Name);
                        if (c.loginCharacter) loginCharacter = character;
                    }

                    var iC = new Character();
                    foreach (string tag in Tags)
                    {
                        var entry = new TagEntry(tag, new Character(), filterTagsPanel, false, contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular));
                        entry.Click += delegate
                        {
                            filterTextBox.Text += ((filterTextBox.Text.Trim().EndsWith(";") || filterTextBox.Text.Trim() == "") ? " " : "; ") + "-t " + tag;
                        };
                        TagEntries.Add(entry);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + CharactersPath + "'.");
                return false;
            }
        }

        public void SaveCharacters()
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
                        Tags = String.Join("|", c.Tags),
                        loginCharacter = c.loginCharacter,
                        include = c.include,
                    };


                    _data.Add(jsonCharacter);
                }

                string json = JsonConvert.SerializeObject(_data.ToArray());

                //write string to file
                System.IO.File.WriteAllText(CharactersPath, json);
            }
        }

        protected override async Task LoadAsync()
        {
            cornerButton = new CornerIcon()
            {
                IconName = "Characters",
                Icon = Textures.Icons[(int)Icons.People],
                HoverIcon = Textures.Icons[(int)Icons.PeopleWhite],
                Priority = 4
            };
            cornerButton.Click += delegate { MainWidow.ToggleWindow(); };
        }

        private Character getCharacter(string name)
        {
            foreach (Character c in Characters)
            {
                if (c.Name == name) return c;
            }

            return new Character();
        }
        private void UpdateCharacterPanel()
        {
            if (charactersLoaded)
            {
                Characters.Sort((a, b) => b.LastModified.CompareTo(a.LastModified));

                string txt = filterTextBox.Text.ToLower();

                string[] parts = txt.Split(';');
                List<string> mapFilters = new List<string>();
                List<string> craftingFilters = new List<string>();
                List<string> professionFilters = new List<string>();
                List<string> birthdayFilters = new List<string>();
                List<string> raceFilters = new List<string>();
                List<string> nameFilters = new List<string>();
                List<string> tagFilters = new List<string>();

                List<string> filterParts = new List<string>();
                foreach (string part in parts)
                {
                    filterParts.Add(part.Trim().ToLower());
                }

                foreach (string part in filterParts)
                {
                    if (part != "")
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

                            //Custom Tag filter
                            case string s when s.StartsWith("-t "):
                                tagFilters.Add(s.ToLower().Replace("-t ", "").Trim());
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
                                if (DataManager.getCraftingName(crafting.Id).ToLower().Contains(s)) return true;
                            }
                        }
                    }

                    foreach (string s in tagFilters)
                    {
                        foreach (string tag in c.Tags)
                        {
                            if (tag.ToLower().Contains(s)) return true;
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
                    bool raceMatch = false;
                    bool specMatch = false;

                    foreach (ToggleIcon toggle in filterProfessions)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == (int)c._Profession) professionMatch = true;
                    }

                    foreach (ToggleIcon toggle in filterSpecs)
                    {
                        if (toggle != null && toggle._State == 1 && (toggle.Id == (int)c._Specialization)) specMatch = true;
                    }

                    foreach (ToggleIcon toggle in filterBaseSpecs)
                    {
                        if (toggle != null && toggle._State == 1 && ((int)c._Specialization == 0 && toggle.Id == (int)c._Profession)) specMatch = true;
                    }

                    foreach (ToggleIcon toggle in filterRaces)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == (int)c.Race) raceMatch = true;
                    }

                    foreach (ToggleIcon toggle in filterCrafting)
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

                    switch (filterWindow.birthdayToggle._State)
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
                    return craftingMatch && professionMatch && birthdayMatch && raceMatch && specMatch;
                }

                foreach (Character character in Characters)
                {
                    if ((character.include || showAllCharacters) && matchingFilterString(character) && matchingToggles(character))
                    {
                        character.Show();
                    }
                    else
                    {
                        character.Hide();
                    }
                }

                if (CharacterPanel != null)
                {
                    CharacterPanel.SortChildren<CharacterControl>((a, b) => b.assignedCharacter.LastModified.CompareTo(a.assignedCharacter.LastModified));
                }
            }
        }

        private void Update_CurrentCharacter()
        {
            if (GameService.GameIntegration.Gw2Instance.IsInGame && charactersLoaded)
            {
                var player = GameService.Gw2Mumble.PlayerCharacter;
                Last.character = Current.character;

                Current.character = getCharacter(player.Name);

                Current.character.UpdateCharacter();

                if (Last.character != Current.character && userAccount != null)
                {
                    userAccount.LastBlishUpdate = DateTimeOffset.UtcNow;
                    userAccount.Save();
                    filterCharacterPanel = true;
                    Current.character.Save();
                }
            }
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            // Base handler must be called
            base.OnModuleLoaded(e);
            CreateWindow();
            CreateFilterWindow();
            CreateSubWindow();

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += delegate {
                if (Current.character != null)
                {
                    Current.character.UpdateProfession();
                    filterCharacterPanel = true;
                };
            };

            OverlayService.Overlay.UserLocaleChanged += delegate { Load_UserLocale(); };
            Load_UserLocale();

            if (Settings.AutoLogin.Value && (player == null || player.Name == ""))
            {
                Blish_HUD.Controls.Intern.Keyboard.Stroke(VirtualKeyShort.RETURN, false);
            };
        }
        private void CreateWindow()
        {
            var _textureWindowBackground = Textures.Backgrounds[(int)_Backgrounds.MainWindow];

            Module.MainWidow = new StandardWindow(
                _textureWindowBackground,
                new Rectangle(15, 45, WINDOW_WIDTH - 20, WINDOW_HEIGHT),
                new Rectangle(10, 15, WINDOW_WIDTH, WINDOW_HEIGHT)
                )
            {
                Parent = GameService.Graphics.SpriteScreen,
                Title = "Characters",
                Emblem = Textures.Emblems[(int)_Emblems.Characters],
                Subtitle = "❤",
                SavesPosition = true,
                Id = $"CharactersWindow",
            };
            MainWidow.Hidden += delegate
            {
                subWindow.Hide();
                filterWindow.Hide();
            };
            MainWidow.Shown += delegate
            {
                subWindow.Hide();
                filterWindow.Hide();
                if (Settings.FocusFilter.Value) {
                    Control.ActiveControl = filterTextBox;
                    filterTextBox.Focused = true;
                };
            };            

            infoImage = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Info],
                Size = new Point(28, 28),
                Location = new Point(Module.MainWidow.Width - 25, -5),
                Parent = Module.MainWidow,
                Visible = false,
            };

            refreshAPI = new StandardButton()
            {
                Size = new Point(185, 25),
                Location = new Point(175, 0),
                Parent = Module.MainWidow,
                Text = "Refresh API Data",
                Visible = false,
            };
            refreshAPI.Click += delegate
            {
                FetchAPI(true);
            };

            filterTextBox = new TextBox()
            {
                PlaceholderText = "Search for ...",
                Size = new Point(313 - 8 - 23 + 5, 30),
                Font = GameService.Content.DefaultFont16,
                Location = new Point(5, 20),
                Parent = Module.MainWidow,
                BasicTooltipText = "'-c CraftingDiscipline'" + Environment.NewLine + "'-p Profession/Specialization'" + Environment.NewLine + "'-r Race'" + Environment.NewLine + "'-b(irthday)'" + Environment.NewLine + "'-c Chef; -p Warrior' will show all warriors and all chefs"
            };

            Tooltip tt = new Tooltip();
            filterTextBox.TextChanged += delegate { filterCharacterPanel = true; };
            filterTextBox.Click += delegate {
                if (filterWindow.Visible)
                {
                    filterWindow.Hide();
                }
                else
                {
                    filterWindow.Show();
                };
            };

            clearButton = new StandardButton()
            {
                Text = "Clear",
                Location = new Point(290 + 2, 19),
                Size = new Point(73, 32),
                Parent = MainWidow,
                ResizeIcon = true,
            };
            void reset()
            {
                List<List<ToggleIcon>> toggles = new List<List<ToggleIcon>>();
                toggles.Add(filterCrafting);
                toggles.Add(filterProfessions);
                toggles.Add(filterSpecs);
                toggles.Add(filterRaces);
                toggles.Add(filterBaseSpecs);

                foreach(List<ToggleIcon>  toggleArray in toggles)
                {
                    foreach (ToggleIcon toggle in toggleArray)
                    {
                        if (toggle != null)
                        {
                            toggle._State = 1;
                        }
                    }
                }

                filterWindow.birthdayToggle._State = 0;
                filterTextBox.Text = null;
                filterCharacterPanel = true;
            }
            clearButton.Click += delegate { reset(); };

            CharacterPanel = new FlowPanel()
            {
                CanScroll = true,
                ShowBorder = true,
                Parent = MainWidow,
                Width = MainWidow.Width,
                Height = MainWidow.Height - (clearButton.Location.Y + clearButton.Height + 5 + 50),
                //HeightSizingMode = SizingMode.Fill,
                Location = new Point(0, clearButton.Location.Y + clearButton.Height + 5),
                FlowDirection = ControlFlowDirection.SingleTopToBottom,
            };

            //Module.MainWidow.Show();
        }
        private void CreateFilterWindow()
        {
            ContentService contentService = new ContentService();
            Specializations[] specs = {
                //HoT
                Specializations.Dragonhunter,
                Specializations.Berserker,
                Specializations.Scrapper,
                Specializations.Druid,
                Specializations.Daredevil,
                Specializations.Tempest,
                Specializations.Chronomancer,
                Specializations.Reaper,
                Specializations.Herald,

                //PoF
                Specializations.Firebrand,
                Specializations.Spellbreaker,
                Specializations.Holosmith,
                Specializations.Soulbeast,
                Specializations.Deadeye,
                Specializations.Weaver,
                Specializations.Mirage,
                Specializations.Scourge,
                Specializations.Renegade,

                //EoD
                Specializations.Willbender,
                Specializations.Bladesworn,
                Specializations.Mechanist,
                Specializations.Untamed,
                Specializations.Specter,
                Specializations.Catalyst,
                Specializations.Virtuoso,
                Specializations.Harbinger,
                Specializations.Vindicator,
            };

            var windowPadding = new Point(5, 5);

            filterWindow = new FilterWindow()
            {
                Height = 500,
                HeightSizingMode = SizingMode.AutoSize,
                Width = 300,
                Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + 60),
                Parent = GameService.Graphics.SpriteScreen,
                Texture = Textures.Backgrounds[(int)_Backgrounds.ExpandWindow],
                AutoSizePadding = new Point(windowPadding.X, windowPadding.Y),
            };
            filterWindow.Shown += delegate { subWindow.Hide(); };
            MainWidow.Moved += delegate {
                filterWindow.Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + 60);
            };

            var mainPanel = new FlowPanel()
            {
                HeightSizingMode = SizingMode.AutoSize,
                WidthSizingMode = SizingMode.Fill,
                Parent = filterWindow,
                ControlPadding = new Vector2(2, 5),
            };

            var closeButton = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Close],
                Parent = filterWindow,
                Location = new Point(filterWindow.Width - 23, 2),
                Size = new Point(21, 23),
            };
            closeButton.MouseEntered += delegate { closeButton.Texture = Textures.Icons[(int)Icons.CloseHovered]; };
            closeButton.MouseLeft += delegate { closeButton.Texture = Textures.Icons[(int)Icons.Close]; };
            closeButton.Click += delegate {
                filterWindow.Hide();
            };

            //Utility Filters
            #region Utility
            filterWindow.Utility = new HeadedFlowRegion()
            {
                WidthSizingMode = SizingMode.Fill,
                Text = "Utility",
                Width = filterWindow.Width - (windowPadding.X * 2),
                Parent = mainPanel,
            };

            var region = filterWindow.Utility.contentFlowPanel;
            region.OuterControlPadding = new Vector2(0, 0);

            filterWindow.visibleToggle = new ToggleIcon()
            {
                Texture = Textures.Icons[(int)Icons.Hide],
                Parent = region,
                _State = 0,
                _MaxState = 2,
                _Textures = {
                    Textures.Icons[(int)Icons.Hide],
                    Textures.Icons[(int)Icons.Visible],
                },
            };
            filterWindow.visibleToggle.Click += delegate
            {
                showAllCharacters = filterWindow.visibleToggle._State == 1;
            };
            filterWindow.birthdayToggle = new ToggleIcon()
            {
                Parent = region,
                _Textures = {
                    Textures.Icons[(int)Icons.BirthdayGiftGray],
                    Textures.Icons[(int)Icons.BirthdayGift],
                    Textures.Icons[(int)Icons.BirthdayGiftOpen],
                },
                _MaxState = 3,
                _State = 0,
            };

            foreach (RaceType race in Enum.GetValues(typeof(RaceType)))
            {
                var index = (int)race;

                filterRaces.Insert(index, new ToggleIcon()
                {
                    _Textures =
                    {
                        Textures.RacesDisabled[index],
                        Textures.Races[index],
                    },
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(24, 24),
                    Texture = Textures.Races[index],
                    Parent = region,
                    Id = index,
                });

                new Label()
                {
                    Text = "",
                    Parent = region,
                    Visible = true,
                    Width = 5,
                };
            }
            #endregion

            //Crafting Filters
            #region Crafting
            filterWindow.Crafting = new HeadedFlowRegion()
            {
                WidthSizingMode = SizingMode.Fill,
                Text = Strings.common.CraftingProfession,
                Width = filterWindow.Width - (windowPadding.X * 2),
                Parent = mainPanel,
            };

            region = filterWindow.Crafting.contentFlowPanel;

            filterCrafting = new List<ToggleIcon>(new ToggleIcon[Textures.Crafting.Length]);
            foreach (Crafting profession in Enum.GetValues(typeof(Crafting)))
            {
                var index = (int)profession;
                filterCrafting.Insert(index, new ToggleIcon()
                {
                    Size = new Point(28, 28),
                    _Textures = {
                        Textures.CraftingDisabled[index],
                        Textures.Crafting[index],
                    },
                    _State = 1,
                    _MaxState = 2,
                    Parent = region,
                    Id = index,
                });
            }
            #endregion

            //Profession Filters
            #region Profession
            filterWindow.Profession = new HeadedFlowRegion()
            {
                WidthSizingMode = SizingMode.Fill,
                Text = Strings.common.Profession,
                Width = filterWindow.Width - (windowPadding.X * 2),
                Parent = mainPanel,
            };

            region = filterWindow.Profession.contentFlowPanel;

            filterProfessions = new List<ToggleIcon>(new ToggleIcon[Textures.Professions.Length]);
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                var index = (int)profession;
                filterProfessions.Insert(index, new ToggleIcon()
                {
                    _Textures = {
                        Textures.ProfessionsDisabled[index],
                        Textures.Professions[index],
                    },
                    _State = 1,
                    _MaxState = 2,
                    Parent = region,
                    Id = index,
                });
            }
            #endregion

            //Specialization Filters
            #region Specialization
            filterWindow.Specialization = new HeadedFlowRegion()
            {
                WidthSizingMode = SizingMode.Fill,
                Text = Strings.common.Specialization,
                Width = filterWindow.Width - (windowPadding.X * 2),
                Parent = mainPanel,
            };

            region = filterWindow.Specialization.contentFlowPanel;
            filterBaseSpecs = new List<ToggleIcon>(new ToggleIcon[Textures.Professions.Length]);
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                var index = (int)profession;
                filterBaseSpecs.Insert(index, new ToggleIcon()
                {
                    _Textures = {
                        Textures.ProfessionsDisabled[index],
                        Textures.Professions[index],
                    },
                    _State = 1,
                    _MaxState = 2,
                    Parent = region,
                    Id = index,
                });
            }

            filterSpecs = new List<ToggleIcon>(new ToggleIcon[Textures.Specializations.Length]);
            foreach (Specializations spec in specs)
            {
                var index = (int)spec;
                filterSpecs.Insert(index, new ToggleIcon()
                {
                    _Textures = {
                         Textures.SpecializationsDisabled[index],
                          Textures.Specializations[index],
                    },
                    _State = 1,
                    _MaxState = 2,
                    Parent = region,
                    Id = index,
                });
            }

            filterWindow.toggleSpecsButton = new StandardButton()
            {
                Text = Strings.common.ToggleAll,
                Parent = region,
                Size = new Point(region.Width - 10, 25),                
                Padding = new Thickness(0, 3),
            };
            region.Resized += delegate { filterWindow.toggleSpecsButton.Width = region.Width - 10; };
            filterWindow.toggleSpecsButton.Click += delegate {
                var state = filterBaseSpecs[(int)Professions.Guardian]._State == 1 ? 0 : 1;

                List<List<ToggleIcon>> toggles = new List<List<ToggleIcon>>();
                toggles.Add(filterSpecs);
                toggles.Add(filterBaseSpecs);

                foreach (List<ToggleIcon> toggleArray in toggles)
                {
                    foreach (ToggleIcon toggle in toggleArray)
                    {
                        if (toggle != null)
                        {
                            toggle._State = state;
                        }
                    }
                }

                filterWindow.birthdayToggle._State = 0;
                filterCharacterPanel = true;
            };
            #endregion

            //Custom Tag Filters
            #region Custom Tags
            filterWindow.CustomTags = new HeadedFlowRegion()
            {
                WidthSizingMode = SizingMode.Fill,
                Text = Strings.common.CustomTags,
                Width = filterWindow.Width - (windowPadding.X * 2),
                Parent = mainPanel,
            };

            region = filterWindow.CustomTags.contentFlowPanel;
            region.ControlPadding = new Vector2(3, 2);
            filterTagsPanel = region;
            #endregion

            filterWindow.Hide();
        }
        private void CreateSubWindow()
        {
            ContentService contentService = new ContentService();
            var offset = (190 - 16 - 5);
            subWindow = new CharacterDetailWindow()
            {
                //Size = new Point(350, MainWidow.Height - offset),
                Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + offset),
                Parent = GameService.Graphics.SpriteScreen,
                Texture = Textures.Backgrounds[(int)_Backgrounds.ExpandWindow],
                Width =  350,
                HeightSizingMode = SizingMode.AutoSize,
            };
            subWindow.Shown += delegate { filterWindow.Hide(); };

            MainWidow.Moved += delegate {
                subWindow.Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + offset);
            };            

            //Profession Icon
            subWindow.spec_Image = new Image()
            {
                Location = new Point(0, 0),
                Texture = Textures.Professions[(int) Professions.Guardian],
                Size = new Point(58, 58),
                Parent = subWindow,
            };

            //Character Name
            subWindow.name_Label = new Label()
            {
                Location = new Point(60, 0),
                Text = "py" + Name + "yq",
                Parent = subWindow,
                Height = 25,
                Width = subWindow.Width - 60 - 32 - 5,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
            };

            //Include Icon
            subWindow.include_Image = new Image()
            {
                Location = new Point(subWindow.name_Label.Location.X + subWindow.name_Label.Width + 5, 0),
                Texture = Textures.Icons[(int)Icons.Visible],
                Size = new Point(32, 32),
                Parent = subWindow,
            };
            subWindow.include_Image.Click += delegate {
                subWindow.assignedCharacter.include = !subWindow.assignedCharacter.include;
                subWindow.assignedCharacter.Save();

                subWindow.include_Image.Texture = subWindow.assignedCharacter.include ? Textures.Icons[(int)Icons.Visible] : Textures.Icons[(int)Icons.Hide];
            };

            //Separator
            new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = subWindow,
                Location = new Point(55, 27),
                Size = new Point(subWindow.Width - 165, 4),
            };

            //Spec Name
            subWindow.spec_Label = new Label()
            {
                Location = new Point(60, 33),
                Text = DataManager.getProfessionName(1),
                Parent = subWindow,
                Height = 25,
                Width = subWindow.Width - 165,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
                Visible = false,
            };

            //Login Character
            subWindow.loginCharacter = new Checkbox()
            {
                Location = new Point(60, 33),
                Text = Strings.common.LoginCharacter,
                Parent = subWindow,
                Height = 25,
                Width = subWindow.Width - 165,
            };
            subWindow.loginCharacter.Click += delegate { 
                if (subWindow.loginCharacter.Checked)
                {
                    foreach (Character c in Characters)
                    {
                        c.loginCharacter = c == subWindow.assignedCharacter ? subWindow.loginCharacter.Checked : false;
                    }
                }
                else
                {
                    subWindow.assignedCharacter.loginCharacter = subWindow.loginCharacter.Checked;
                }
                subWindow.assignedCharacter.Save();
            };


            subWindow.tag_TextBox = new TextBox()
            {
                Parent = subWindow,
                Location = new Point(5, 60 + 15),
                Size = new Point(subWindow.Width - 5 - 25 - 5, 25),
                PlaceholderText = "PvE, WvW, PvP, Raiding, ERP ..."
            };
            subWindow.tag_TextBox.TextChanged += delegate
            {
                if (subWindow.tag_TextBox.Text != null && subWindow.tag_TextBox.Text.Trim() != "")
                {
                    subWindow.addTag_Button.Texture = Textures.Icons[(int)Icons.Add];
                }
                else
                {
                    subWindow.addTag_Button.Texture = Textures.Icons[(int)Icons.AddDisabled];
                }
            };

            void addTag()
            {
                var txt = subWindow.tag_TextBox != null && subWindow.tag_TextBox.Text.Trim() != "" ? subWindow.tag_TextBox.Text : null;
                if (txt != null && subWindow.assignedCharacter != null && !subWindow.assignedCharacter.Tags.Contains(txt.Trim()))
                {
                    new TagEntry(txt, subWindow.assignedCharacter, subWindow.customTags_Panel);
                    subWindow.assignedCharacter.Tags.Add(txt);
                    subWindow.assignedCharacter.Save();

                    if (!Tags.Contains(txt))
                    {
                        Tags.Add(txt);
                        var entry = new TagEntry(txt, new Character(), filterTagsPanel, false, contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular));
                        entry.Click += delegate
                        {
                            filterTextBox.Text += ((filterTextBox.Text.Trim().EndsWith(";") || filterTextBox.Text.Trim() == "") ? " " : "; ") + "-t " + txt;
                        };
                        TagEntries.Add(entry);
                    }

                    subWindow.tag_TextBox.Text = null;
                    subWindow.customTags_Panel.SortChildren<TagEntry>((a, b) => a.textLabel.Text.CompareTo(b.textLabel.Text));

                    subWindow.Invalidate();
                }
            }

            subWindow.addTag_Button = new Image()
            {
                Texture = Textures.Icons[(int) Icons.AddDisabled],
                Parent = subWindow,
                Location = new Point(subWindow.Width - 25 - 5, 58 + 15),
                Size = new Point(29, 29),
            };
            subWindow.addTag_Button.MouseEntered += delegate { subWindow.addTag_Button.Texture = Textures.Icons[(int)Icons.AddHovered]; };
            subWindow.addTag_Button.MouseLeft += delegate { subWindow.addTag_Button.Texture = Textures.Icons[(int)Icons.AddDisabled]; };
            subWindow.addTag_Button.Click += delegate { addTag(); };
            subWindow.tag_TextBox.EnterPressed += delegate { addTag(); };


            subWindow.customTags_Panel = new FlowPanel()
            {
                Parent = subWindow,
                Location = new Point(5, 95),
                Width = 330,
                ShowBorder = true,
                OuterControlPadding = new Vector2(2, 2),
                ControlPadding = new Vector2(5, 2),
                HeightSizingMode = SizingMode.AutoSize,
            };

            subWindow.Hide();
        }
        private void OnKeyPressed_ToggleMenu(object o, EventArgs e)
        {
            // !filterTextBox.Focused && !subWindow.tag_TextBox.Focused
            if (!(Control.ActiveControl is TextBox))
            {
                MainWidow?.ToggleWindow();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Last.Tick_Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_Update += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_APIUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_PanelUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_FadeEffect += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (charactersLoaded && Last.Tick_Update > 250)
            {
                Last.Tick_Update = -250;
                Update_CurrentCharacter();

                foreach (Character character in Characters)
                {
                    if (character.characterControl != null && character.characterControl.Visible)
                    {
                        character.Update_UI_Time();
                    }
                }
                if (Settings.AutoLogin.Value && !loginCharacter_Swapped && loginCharacter != null && swapCharacter == null)
                {
                    loginCharacter_Swapped = true;
                    loginCharacter.Swap();
                }

                if (swapCharacter != null && !GameService.GameIntegration.Gw2Instance.IsInGame && DateTime.UtcNow.Subtract(lastLogout).TotalMilliseconds >= Settings.SwapDelay.Value)
                {
                    swapCharacter.Swap();
                    swapCharacter = null;
                }
            }

            if (Settings.FadeSubWindows.Value && Last.Tick_FadeEffect > 30)
            {
                Last.Tick_FadeEffect = -30;
                if (filterWindow.Visible && DateTime.Now.Subtract(filterWindow.lastInput).TotalMilliseconds >= 2500)
                {
                    filterWindow.Opacity = filterWindow.Opacity - (float)0.1;
                    if (filterWindow.Opacity <= (float) 0) filterWindow.Hide();
                }
            }

            if (charactersLoaded && Last.Tick_PanelUpdate > Settings._FilterDelay)
            {
                Last.Tick_PanelUpdate = -Settings._FilterDelay;

                if (filterCharacterPanel)
                {
                    filterCharacterPanel = false;
                    UpdateCharacterPanel();
                }
            }

            if (Last.Tick_Save > 250 && saveCharacters)
            {
                Last.Tick_Save = -250;
                SaveCharacters();
            }

            if (Last.Tick_APIUpdate > 30000 && userAccount != null)
            {
                Logger.Debug("Check GW2 API for Updates.");
                Last.Tick_APIUpdate = -30000;
                FetchAPI();
            }
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
            MainWidow?.Dispose();
            subWindow?.Dispose();
            filterWindow?.Dispose();
            cornerButton?.Dispose();

            // Dispose Settings Event
            Settings.ShortcutKey.Value.Activated -= OnKeyPressed_ToggleMenu;
            Gw2ApiManager.SubtokenUpdated -= Gw2ApiManager_SubtokenUpdated;

            CharacterNames = new List<string>();
            Characters = new List<Character>();
            Tags = new List<string>();
            TagEntries = new List<TagEntry>();
            userAccount = null;
            swapCharacter = null;
            API_Account = null;
            Current.character = null;
            Last.character = null;
            
            Textures.Backgrounds = null;
            Textures.Crafting = null;
            Textures.CraftingDisabled = null;
            Textures.Emblems = null;
            Textures.Icons = null;
            Textures.Professions= null;
            Textures.ProfessionsDisabled= null;
            Textures.Races= null;
            Textures.RacesDisabled = null;
            Textures.Specializations= null;
            Textures.SpecializationsDisabled= null;

            ModuleInstance = null;
        }
    }
}
