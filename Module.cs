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

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Blish_HUD.Modules.Module))]
    public partial class Module : Blish_HUD.Modules.Module
    {
        DateTime dateZero;
        public static DateTime lastLogout;
        private static bool requestAPI = true;
        private static bool filterCharacterPanel = true;
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
        //GUI
        public static StandardWindow MainWidow { get; private set; }

        public static FlowPanel CharacterPanel;
        public static FlowPanel filterPanel;
        public static TextBox filterTextBox;
        public static StandardButton clearButton;
        public static Image infoImage;
        public static Image expandButton;
        public static StandardButton refreshAPI;
        public static CornerIcon cornerButton;
        public static ToggleImage birthdayToggle;
        public static ToggleImage[] filterProfessions;
        public static ToggleImage[] filterCrafting;
        public static ToggleImage[] filterRaces;
        public static ToggleImage[] filterSpecs;
        public static ToggleImage[] filterBaseSpecs;
        public static BasicContainer filterWindow { get; private set; }
        public static Label racesLabel;
        public static Label specializationLabel;

        public static CharacterDetailWindow subWindow { get; private set; }

        //States
        public static bool charactersLoaded;
        private static class Last
        {
            public static Character character { get; set; }
            public static string CharName { get; set; }
            public static int CharCount { get; set; }
            public static double Tick_PanelUpdate;
            public static double Tick_Save;
            public static double Tick_Update;
            public static double Tick_APIUpdate;
        }
        private static class Current
        {
            public static Character character { get; set; }
        }

        public static Account API_Account;
        public static AccountInfo userAccount { get; set; }
        public static List<string> CharacterNames = new List<string>();
        public static List<Character> Characters = new List<Character>();

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
            clearButton.Text = Strings.common.Clear;

            racesLabel.Text = Strings.common.Race;
            specializationLabel.Text = Strings.common.Specialization;

            foreach (ToggleImage toggle in filterProfessions)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getProfessionName(toggle.Id);
                }
            }

            foreach (ToggleImage toggle in filterBaseSpecs)
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

            foreach (ToggleImage toggle in filterSpecs)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getSpecName(toggle.Id);
                }
            }

            foreach (ToggleImage toggle in filterRaces)
            {
                if (toggle != null)
                {
                    toggle.BasicTooltipText = DataManager.getRaceName(toggle.Id);
                }
            }

            birthdayToggle.BasicTooltipText = Strings.common.Birthday;
        }
        private void LoadCharacterList()
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
                    };

                    Characters.Add(character);
                    CharacterNames.Add(character.Name);
                }
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
            cornerButton.Click += delegate { MainWidow.ToggleWindow(); subWindow.Hide(); filterWindow.Hide(); };
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
                    bool raceMatch = false;
                    bool specMatch = false;

                    foreach (ToggleImage toggle in filterProfessions)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == (int)c._Profession) professionMatch = true;
                    }

                    foreach (ToggleImage toggle in filterSpecs)
                    {
                        if (toggle != null && toggle._State == 1 && (toggle.Id == (int)c._Specialization)) specMatch = true;
                    }

                    foreach (ToggleImage toggle in filterBaseSpecs)
                    {
                        if (toggle != null && toggle._State == 1 && ((int)c._Specialization == 0 && toggle.Id == (int)c._Profession)) specMatch = true;
                    }

                    foreach (ToggleImage toggle in filterRaces)
                    {
                        if (toggle != null && toggle._State == 1 && toggle.Id == (int)c.Race) raceMatch = true;
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
                    return craftingMatch && professionMatch && birthdayMatch && raceMatch && specMatch;
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

                CharacterPanel.SortChildren<CharacterControl>((a, b) => b.assignedCharacter.LastModified.CompareTo(a.assignedCharacter.LastModified));
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
            };

            infoImage = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Info],
                Size = new Point(28, 28),
                Location = new Point(Module.MainWidow.Width - 25, -5),
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
            refreshAPI.Click += delegate
            {
                FetchAPI(true);
            };

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
            filterTextBox.TextChanged += delegate { filterCharacterPanel = true; };

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
                List<ToggleImage[]> toggles = new List<ToggleImage[]>();
                toggles.Add(filterCrafting);
                toggles.Add(filterProfessions);
                toggles.Add(filterSpecs);
                toggles.Add(filterRaces);
                toggles.Add(filterBaseSpecs);

                foreach(ToggleImage[]  toggleArray in toggles)
                {
                    foreach (ToggleImage toggle in toggleArray)
                    {
                        if (toggle != null)
                        {
                            toggle._State = 1;
                        }
                    }
                }

                birthdayToggle._State = 0;
                filterTextBox.Text = null;
                filterCharacterPanel = true;
            }
            clearButton.Click += delegate { reset(); };

            filterPanel = new FlowPanel()
            {
                Parent = Module.MainWidow,
                Size = new Point(WINDOW_WIDTH - 56, 60),
                Location = new Point(10, 68 - 16 + 3),
                FlowDirection = ControlFlowDirection.LeftToRight,
            };

            filterProfessions = new ToggleImage[Textures.Professions.Length];
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                filterProfessions[(int)profession] = new ToggleImage()
                {
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(32, 32),
                    Texture = Textures.Professions[(int)profession],
                    Parent = filterPanel,
                    Id = (int)profession,
                };

                filterProfessions[(int)profession]._Textures = new Texture2D[2];
                filterProfessions[(int)profession]._Textures[0] = Textures.ProfessionsDisabled[(int)profession];
                filterProfessions[(int)profession]._Textures[1] = Textures.Professions[(int)profession];

                filterProfessions[(int)profession].Click += delegate { filterProfessions[(int)profession].Toggle(); filterCharacterPanel = true; };
            }

            birthdayToggle = new ToggleImage()
            {
                isActive = false,
                Size = new Point(32, 32),
                Texture = Textures.Icons[(int)Icons.BirthdayGiftGray],
                Parent = filterPanel,
                _State = 0,
                _MaxState = 3,
            };
            birthdayToggle._Textures = new Texture2D[3];
            birthdayToggle._Textures[0] = Textures.Icons[(int)Icons.BirthdayGiftGray];
            birthdayToggle._Textures[1] = Textures.Icons[(int)Icons.BirthdayGift];
            birthdayToggle._Textures[2] = Textures.Icons[(int)Icons.BirthdayGiftOpen];

            birthdayToggle.Click += delegate { birthdayToggle.Toggle(); filterCharacterPanel = true; };

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

                    filterCrafting[(int)crafting].Click += delegate { filterCrafting[(int)crafting].Toggle(); filterCharacterPanel = true; };
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

            filterCrafting[(int)Crafting.Unknown].Click += delegate { filterCrafting[(int)Crafting.Unknown].Toggle(); filterCharacterPanel = true; };

            expandButton = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Expand],
                Size = new Point(26, 64),
                Location = new Point(WINDOW_WIDTH - 56 + 5, 68 - 16 + 5),
                Parent = Module.MainWidow,
            };
            expandButton.MouseEntered += delegate {
                switch (filterWindow.Visible)
                {
                    case true:
                        expandButton.Texture = Textures.Icons[(int)Icons.CollapseHovered];
                        break;

                    case false:
                        expandButton.Texture = Textures.Icons[(int)Icons.ExpandHovered];
                        break;
                }
            };
            expandButton.MouseLeft += delegate { 
                switch (filterWindow.Visible)
                {
                    case true:
                        expandButton.Texture = Textures.Icons[(int)Icons.Collapse];
                        break;

                    case false:
                        expandButton.Texture = Textures.Icons[(int)Icons.Expand];
                        break;
                }
            };
            expandButton.Click += delegate {
                if (filterWindow.Visible)
                {
                    filterWindow.Hide();
                    expandButton.Texture = Textures.Icons[(int)Icons.ExpandHovered];
                }
                else
                {
                    filterWindow.Show();
                    expandButton.Texture = Textures.Icons[(int)Icons.CollapseHovered];
                }
            };

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
        private void CreateFilterWindow()
        {
            ContentService contentService = new ContentService();

            filterWindow = new BasicContainer()
            {
                Size = new Point(300, 225 + 32 + 30),
                Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + 60),
                Parent = GameService.Graphics.SpriteScreen,
                Texture = Textures.Backgrounds[(int)_Backgrounds.ExpandWindow],
            };

            MainWidow.Moved += delegate {
                filterWindow.Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + 60);
            };


            var closeButton = new Image()
            {
                Texture = Textures.Icons[(int)Icons.Close],
                Parent = filterWindow,
                Location = new Point(filterWindow.Width - 30, -2),
                Size = new Point(32, 32),               
            };
            closeButton.MouseEntered += delegate { closeButton.Texture = Textures.Icons[(int)Icons.CloseHovered]; };
            closeButton.MouseLeft += delegate { closeButton.Texture = Textures.Icons[(int)Icons.Close]; };
            closeButton.Click += delegate { 
                filterWindow.Hide();
                expandButton.Texture = Textures.Icons[(int)Icons.Expand];
            };


            racesLabel = new Label()
            {
                Text = "Races",
                Parent = filterWindow,
                Location = new Point(10, 5),
                Visible = true,
                Width = 200,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
                Height = 27,
            };
            new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = filterWindow,
                Location = new Point(0, 30),
                Size = new Point(filterWindow.Width, 4),
            };

            var racePanel = new FlowPanel()
            {
                Parent = filterWindow,
                Size = new Point(filterWindow.Width - 10, 30),
                Location = new Point(5, 25 + 20),
                FlowDirection = ControlFlowDirection.LeftToRight,
            };
            filterRaces = new ToggleImage[Textures.Races.Length];
            foreach (RaceType race in Enum.GetValues(typeof(RaceType)))
            {
                var index = (int)race;

                filterRaces[index] = new ToggleImage()
                {
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(24, 24),
                    Texture = Textures.Races[index],
                    Parent = racePanel,
                    Id = index,
                };

                filterRaces[index]._Textures = new Texture2D[2];
                filterRaces[index]._Textures[0] = Textures.RacesDisabled[index];
                filterRaces[index]._Textures[1] = Textures.Races[index];

                filterRaces[index].Click += delegate { filterRaces[index].Toggle(); filterCharacterPanel = true; };

                new Label()
                {
                    Text = "",
                    Parent = racePanel,
                    Visible = true,
                    Width = 5,
                };
            }

            specializationLabel = new Label()
            {
                Text = "Specializations",
                Parent = filterWindow,
                Location = new Point(10, 85),
                Visible = true,
                Width = 200,
                Height = 27,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size18, ContentService.FontStyle.Regular),
            };
            new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = filterWindow,
                Location = new Point(0, 110),
                Size = new Point(filterWindow.Width, 4),
            };

            var specPanel = new FlowPanel()
            {
                Parent = filterWindow,
                Size = new Point(filterWindow.Width - 10, 32 * 4),
                Location = new Point(5, 110 + 10),
                FlowDirection = ControlFlowDirection.LeftToRight,
            };
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
            filterBaseSpecs = new ToggleImage[Textures.Professions.Length];
            foreach (Professions profession in Enum.GetValues(typeof(Professions)))
            {
                var index = (int)profession;
                filterBaseSpecs[index] = new ToggleImage()
                {
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(32, 32),
                    Texture = Textures.Professions[index],
                    Parent = specPanel,
                    Id = index,
                };

                filterBaseSpecs[index]._Textures = new Texture2D[2];
                filterBaseSpecs[index]._Textures[0] = Textures.ProfessionsDisabled[index];
                filterBaseSpecs[index]._Textures[1] = Textures.Professions[index];

                filterBaseSpecs[index].Click += delegate { filterBaseSpecs[index].Toggle(); filterCharacterPanel = true; };
            }
            filterSpecs = new ToggleImage[Textures.Specializations.Length];
            foreach (Specializations spec in specs)
            {
                var index = (int)spec;

                filterSpecs[index] = new ToggleImage()
                {
                    _State = 1,
                    _MaxState = 2,
                    Size = new Point(32, 32),
                    Texture = Textures.Specializations[index],
                    Parent = specPanel,
                    Id = index,
                };

                filterSpecs[index]._Textures = new Texture2D[2];
                filterSpecs[index]._Textures[0] = Textures.SpecializationsDisabled[index];
                filterSpecs[index]._Textures[1] = Textures.Specializations[index];

                filterSpecs[index].Click += delegate { filterSpecs[index].Toggle(); filterCharacterPanel = true; };
            }

            var btn =new StandardButton()
            {
                Text = "Toggle all",
                Parent = filterWindow,
                Location = new Point(5, specPanel.Location.Y + specPanel.Height + 5),
                Size = new Point(specPanel.Width, 25),
            };
            
            btn.Click += delegate {
                var state = filterBaseSpecs[(int) Professions.Guardian]._State == 1 ? 0 : 1;

                List<ToggleImage[]> toggles = new List<ToggleImage[]>();
                toggles.Add(filterSpecs);
                toggles.Add(filterBaseSpecs);

                foreach (ToggleImage[] toggleArray in toggles)
                {
                    foreach (ToggleImage toggle in toggleArray)
                    {
                        if (toggle != null)
                        {
                            toggle._State = state;
                        }
                    }
                }

                birthdayToggle._State = 0;
                filterCharacterPanel = true;
            };

            filterWindow.Hide();
        }
        private void CreateSubWindow()
        {
            ContentService contentService = new ContentService();
            var offset = (190 - 16 - 5);
            subWindow = new CharacterDetailWindow()
            {
                Size = new Point(350, MainWidow.Height - offset),
                Location = new Point(MainWidow.Location.X + WINDOW_WIDTH - 25, MainWidow.Location.Y + offset),
                Parent = GameService.Graphics.SpriteScreen,
                Texture = Textures.Backgrounds[(int)_Backgrounds.ExpandWindow],
            };

            MainWidow.Moved += delegate {
                subWindow.Location = new Point(MainWidow.Location.X + WINDOW_WIDTH, MainWidow.Location.Y + 60);
            };

            //Profession Icon
            subWindow.spec_Image = new Image()
            {
                Location = new Point(0, 0),
                Texture = Textures.Specializations[(int) Specializations.Berserker],
                Size = new Point(48, 48),
                Parent = subWindow,
            };

            //Character Name
            subWindow.name_Label = new Label()
            {
                Location = new Point(48 + 5, 0),
                Text = Name,
                Parent = subWindow,
                Height = 30,
                Width = subWindow.Width - 165,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size14, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
            };

            //Separator
            new Image()
            {
                Texture = Textures.Icons[(int)Icons.Separator],
                Parent = subWindow,
                Location = new Point(48, 75),
                Size = new Point(subWindow.Width - 165, 4),
            };

            //Spec Name
            subWindow.spec_Label = new Label()
            {
                Location = new Point(48 + 5, 80),
                Text = DataManager.getProfessionName(1),
                Parent = subWindow,
                Height = 16,
                Width = subWindow.Width - 165,
                Font = contentService.GetFont(ContentService.FontFace.Menomonia, ContentService.FontSize.Size12, ContentService.FontStyle.Regular),
                VerticalAlignment = VerticalAlignment.Middle,
            };

            subWindow.Hide();
        }

        protected override void Update(GameTime gameTime)
        {
            Last.Tick_Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_Update += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_APIUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            Last.Tick_PanelUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (charactersLoaded && Last.Tick_Update > 250)
            {
                Last.Tick_Update = -250;
                Update_CurrentCharacter();

                foreach (Character character in Characters)
                {
                    if (character.characterControl.Visible)
                    {
                        character.Update_UI_Time();
                    }
                }

                if (swapCharacter != null && !GameService.GameIntegration.Gw2Instance.IsInGame && DateTime.UtcNow.Subtract(lastLogout).TotalMilliseconds >= Settings.SwapDelay.Value)
                {
                    swapCharacter.Swap();
                    swapCharacter = null;
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

        /// <inheritdoc />
        protected override void Unload()
        {
            // Unload here

            // All static members must be manually unset
        }
    }
}
