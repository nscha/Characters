using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Controls;
using Kenedia.Modules.Characters.Enums;
using Kenedia.Modules.Characters.Models;
using Kenedia.Modules.Characters.Services;
using Kenedia.Modules.Characters.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Services.TextureManager;
using static Kenedia.Modules.Characters.Utility.WindowsUtil.WindowsUtil;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : Module
    {
        public const uint WMCOMMAND = 0x0111;
        public const uint WMPASTE = 0x0302;

        public static readonly Logger Logger = Logger.GetLogger<Characters>();

        private readonly Ticks _ticks = new();

        private CornerIcon _cornerIcon;
        private bool _dataLoaded;
        private RECT _windowRectangle;
        private Point _clientRes = Point.Zero;
        private RECT _clientRectangle;

        [ImportingConstructor]
        public Characters([Import("ModuleParameters")] ModuleParameters moduleParameters)
            : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        public event EventHandler LanguageChanged;

        public event EventHandler ResolutionChanged;

        public event EventHandler DataLoaded_Event;

        public static VirtualKeyShort[] ModKeyMapping { get; private set; }

        public TagList Tags { get; set; } = new TagList();

        public MainWindow MainWindow { get; private set; }

        public CharacterPotraitCapture PotraitCapture { get; private set; }

        public LoadingSpinner APISpinner { get; private set; }

        public SettingsModel Settings { get; private set; }

        public TextureManager TextureManager { get; private set; }

        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey { get; set; }

        public List<Character_Model> CharacterModels { get; set; } = new List<Character_Model>();

        public Character_Model CurrentCharacterModel { get; set; }

        public CharacterSwapping CharacterSwapping { get; set; }

        public CharacterSorting CharacterSorting { get; set; }

        public string BasePath { get; set; }

        public Data Data { get; private set; }

        public OCR OCR { get; private set; }

        public string CharactersPath { get; set; }

        public string AccountInfoPath { get; set; }

        public string AccountImagesPath { get; set; }

        public string GlobalImagesPath { get; set; }

        public string AccountPath { get; set; }

        public bool RunOCR { get; set; }

        public bool UpdateTags { get; set; }

        public bool SaveCharacters { get; set; }

        public bool FetchingAPI { get; set; }

        public GW2API_Handler GW2APIHandler { get; private set; } = new GW2API_Handler();

        public bool DataLoaded
        {
            get => _dataLoaded;
            set
            {
                _dataLoaded = value;
                if (value)
                {
                    ModuleInstance.OnDataLoaded();
                }
            }
        }

        public RECT WindowRectangle { get => _windowRectangle; set => _windowRectangle = value; }

        public RECT ClientRectangle { get => _clientRectangle; set => _clientRectangle = value; }

        public int TitleBarHeight { get; private set; }

        public int SideBarWidth { get; private set; }

        internal static Characters ModuleInstance { get; set; }

        internal SettingsManager SettingsManager => ModuleParameters.SettingsManager;

        internal ContentsManager ContentsManager => ModuleParameters.ContentsManager;

        internal DirectoriesManager DirectoriesManager => ModuleParameters.DirectoriesManager;

        internal Gw2ApiManager Gw2ApiManager => ModuleParameters.Gw2ApiManager;

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public void OnLanguageChanged(object sender, EventArgs e)
        {
            RebuildUI();
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnResolutionChanged(bool fireEvent = true)
        {
            IntPtr hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            _ = GetWindowRect(hWnd, ref _windowRectangle);
            _ = GetClientRect(hWnd, out _clientRectangle);

            TitleBarHeight = WindowRectangle.Bottom - WindowRectangle.Top - (ClientRectangle.Bottom - ClientRectangle.Top);
            SideBarWidth = WindowRectangle.Right - WindowRectangle.Left - (ClientRectangle.Right - ClientRectangle.Left);

            if (fireEvent)
            {
                ResolutionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void FixCharacterOrder()
        {
            if (CharacterSorting == null)
            {
                CharacterSorting = new CharacterSorting(CharacterModels);
                if (Settings.AutoSortCharacters.Value)
                {
                    CharacterSwapping?.Reset();
                }
                else
                {
                    CharacterSwapping = null;
                }

                CharacterSorting.Finished += (sender, e) =>
                {
                    CharacterSorting = null;
                    MainWindow.SortCharacters();
                };
            }
        }

        public void SwapTo(Character_Model character)
        {
            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;

            if (character.Name != player.Name || !GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                if (CharacterSwapping != null)
                {
                }

                CharacterSwapping = new CharacterSwapping(character);
                CharacterSwapping.Succeeded += (sender, e) =>
                {
                    CharacterSwapping = null;
                    ForceUpdate(null, null);
                    MainWindow.SortCharacters();
                };
                CharacterSwapping.Failed += CharacterSwapping_Failed;
            }
        }

        public void CreateCharacterControls()
        {
            if (MainWindow != null)
            {
                foreach (Character_Model c in CharacterModels)
                {
                    if (MainWindow.CharacterControls.Find(e => e.Character.Name == c.Name) == null)
                    {
                        MainWindow.CharacterControls.Add(new CharacterControl()
                        {
                            Character = c,
                            Parent = MainWindow.ContentPanel,
                            ZIndex = MainWindow.ZIndex + 1,
                        });
                    }
                }

                MainWindow.FilterCharacters();
                MainWindow.UpdateLayout();
            }
        }

        public bool LoadCharacterList()
        {
            try
            {
                if (System.IO.File.Exists(CharactersPath))
                {
                    FileInfo infos = new(CharactersPath);
                    string content = System.IO.File.ReadAllText(CharactersPath);
                    Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content);

                    if (characters != null)
                    {
                        CharacterModels = characters;
                        foreach (Character_Model c in CharacterModels)
                        {
                            foreach (string t in c.Tags)
                            {
                                if (!Tags.Contains(t))
                                {
                                    Tags.Add(t);
                                }
                            }

                            c.Initialize();
                        }

                        DataLoaded = true;
                        return true;
                    }
                }

                DataLoaded = true;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + CharactersPath + "'.");
                System.IO.File.Copy(CharactersPath, CharactersPath.Replace(".json", " [" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "].corruped.json"));
                DataLoaded = true;
                return false;
            }
        }

        public void SaveCharacterList()
        {
            Logger.Debug("Saving Character List.");
            List<JsonCharacter_Model> data = new();

            foreach (Character_Model c in CharacterModels)
            {
                data.Add(new JsonCharacter_Model(c));
            }

            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented, settings);

            // write string to file
            System.IO.File.WriteAllText(CharactersPath, json);
        }

        protected override void Update(GameTime gameTime)
        {
            _ticks.Global += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.APIUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            _ticks.Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.Tags += gameTime.ElapsedGameTime.TotalMilliseconds;
            _ticks.OCR += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_clientRes != GameService.Graphics.Resolution)
            {
                _clientRes = GameService.Graphics.Resolution;
                OnResolutionChanged();
            }

            if (CharacterSorting != null)
            {
                CharacterSorting.Run(gameTime);
            }
            else
            {
                CharacterSwapping?.Run(gameTime);
            }

            if (_ticks.Global > 15000)
            {
                _ticks.Global = 0;
                CurrentCharacterModel = null;

                if (GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
                    CurrentCharacterModel = CharacterModels.Find(e => e.Name == player.Name);

                    if (CurrentCharacterModel != null)
                    {
                        CurrentCharacterModel.Specialization = (SpecializationType)player.Specialization;
                        CurrentCharacterModel.Map = GameService.Gw2Mumble.CurrentMap.Id;
                        CurrentCharacterModel.LastLogin = DateTime.UtcNow;
                        MainWindow?.SortCharacters();
                    }
                }
            }

            if (_ticks.APIUpdate > 300)
            {
                _ticks.APIUpdate = 0;

                GW2APIHandler.CheckAPI();
            }

            if (_ticks.Save > 25 && SaveCharacters)
            {
                _ticks.Save = 0;

                SaveCharacterList();
                SaveCharacters = false;
            }

            if (_ticks.Tags >= 10 && UpdateTags)
            {
                _ticks.Tags = 0;

                UpdateTags = false;
                UpdateTagsCollection();
            }

            if (_ticks.OCR >= 50 && RunOCR)
            {
                _ticks.OCR = 0;

                OCR ??= new OCR();

                if (OCR != null && OCR.Read() != null)
                {
                    RunOCR = false;
                    if (RunOCR)
                    {
                        OCR.ToggleContainer();
                    }
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            Settings = new SettingsModel(settings);
            Settings.ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;

#if DEBUG
            ReloadKey = settings.DefineSetting(
                nameof(ReloadKey),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Alt, Keys.R));

            ReloadKey.Value.Enabled = true;
            ReloadKey.Value.Activated += ReloadKey_Activated;
#endif
        }

        protected override void Initialize()
        {
            Logger.Info($"Starting  {Name} v." + Version.BaseVersion());

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            // string path = DirectoriesManager.GetFullDirectoryPath("characters") + @"\" + API_Account.Name;
            BasePath = DirectoriesManager.GetFullDirectoryPath("characters");

            if (!System.IO.File.Exists(BasePath + @"\gw2.traineddata"))
            {
                using Stream target = System.IO.File.Create(BasePath + @"\gw2.traineddata");
                Stream source = ContentsManager.GetFileStream(@"data\gw2.traineddata");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            if (!System.IO.File.Exists(BasePath + @"\tesseract.dll"))
            {
                using Stream target = System.IO.File.Create(BasePath + @"\tesseract.dll");
                Stream source = ContentsManager.GetFileStream(@"data\tesseract.dll");
                _ = source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            Data = new Data();

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;
        }

        protected override async Task LoadAsync()
        {
            await Task.Delay(0);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            TextureManager = new TextureManager();

            if (Settings.ShowCornerIcon.Value)
            {
                CreateCornerIcons();
            }

            DataLoaded_Event += Characters_DataLoaded_Event;

            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += ForceUpdate;
            player.NameChanged += ForceUpdate;

            Blish_HUD.Gw2Mumble.CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged += ForceUpdate;
            _clientRes = GameService.Graphics.Resolution;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged += ForceUpdate;
            GameService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;

            Tags.CollectionChanged += Tags_CollectionChanged;
            OnResolutionChanged(false);

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Unload()
        {
            MainWindow?.Dispose();
            _cornerIcon?.Dispose();

            TextureManager?.Dispose();
            TextureManager = null;

            if (_cornerIcon != null)
            {
                _cornerIcon.Click -= ToggleModule;
            }

            DataLoaded_Event -= Characters_DataLoaded_Event;
            GameService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;

            Blish_HUD.Gw2Mumble.PlayerCharacter player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged -= ForceUpdate;
            player.NameChanged -= ForceUpdate;

            Blish_HUD.Gw2Mumble.CurrentMap map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged -= ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= ForceUpdate;

            ModuleInstance = null;
        }

        private void OnDataLoaded()
        {
            DataLoaded_Event?.Invoke(this, EventArgs.Empty);
            if (MainWindow == null)
            {
                CreateUI();
            }
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (Control.ActiveControl is not TextBox)
            {
                MainWindow?.ToggleWindow();
            }
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            GW2APIHandler.CheckAPI();
        }

        private void ToggleWindow_Activated(object sender, EventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void ReloadKey_Activated(object sender, EventArgs e)
        {
            RebuildUI();
            MainWindow?.ToggleWindow();
        }

        private void RebuildUI()
        {
            if (MainWindow != null)
            {
                bool shown = MainWindow.Visible;

                MainWindow?.Dispose();
                PotraitCapture?.Dispose();
                OCR?.Dispose();
                CreateUI(true);

                if (shown)
                {
                    MainWindow?.Show();
                }
            }
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DataLoaded)
            {
                UpdateTagsCollection();
            }
        }

        private void UpdateTagsCollection()
        {
            foreach (Character_Model c in CharacterModels)
            {
                ObservableCollection<string> tList = new(c.Tags);

                foreach (string t in tList)
                {
                    if (!Tags.Contains(t))
                    {
                        _ = c.Tags.Remove(t);
                    }
                }
            }
        }

        private void ForceUpdate(object sender, EventArgs e)
        {
            _ticks.Global = 2000000;
            if (CurrentCharacterModel != null)
            {
                CurrentCharacterModel.LastLogin = DateTime.UtcNow;
            }

            CurrentCharacterModel = null;
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
            if (MainWindow == null)
            {
                ScreenNotification.ShowNotification("New API Request sent...");
                GW2APIHandler.CheckAPI();
            }
        }

        private void Characters_DataLoaded_Event(object sender, EventArgs e)
        {
            CreateUI();
        }

        private void ToggleModule(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void CreateCornerIcons()
        {
            _cornerIcon = new CornerIcon()
            {
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156678),
                HoverIcon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156679),
                BasicTooltipText = string.Format(Strings.common.Toggle, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings.ShowCornerIcon.Value,
            };

            APISpinner = new LoadingSpinner()
            {
                Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(_cornerIcon.Width, _cornerIcon.Height),
                BasicTooltipText = "Fetching API data ...",
                Visible = false,
            };

            _cornerIcon.Click += CornerIcon_Click;
            _cornerIcon.Moved += CornerIcon_Moved;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            APISpinner.Location = new Point(_cornerIcon.Left, _cornerIcon.Bottom + 3);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue && _cornerIcon == null)
            {
                CreateCornerIcons();
            }
            else if (_cornerIcon != null && !e.NewValue)
            {
                _cornerIcon.Moved -= CornerIcon_Moved;
                _cornerIcon.Click -= CornerIcon_Click;
                _cornerIcon.Dispose();
                _cornerIcon = null;

                APISpinner.Dispose();
                APISpinner = null;
            }
        }

        private void CharacterSwapping_Failed(object sender, EventArgs e)
        {
            ScreenNotification.ShowNotification("Failed to swap to " + CharacterSwapping.Character.Name + "!");
            if (Settings.AutoSortCharacters.Value)
            {
                ScreenNotification.ShowNotification("Fixing Characters!");
                FixCharacterOrder();
            }

            CharacterSwapping.Failed -= CharacterSwapping_Failed;
        }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e)
        {
            _cornerIcon.BasicTooltipText = string.Format(Strings.common.Toggle, $"{Name}");
            OnLanguageChanged(null, null);
        }

        private void CreateUI(bool force = false)
        {
            if (MainWindow == null || force)
            {
                // var bg = GameService.Content.DatAssetCache.GetTextureFromAssetId(155985).Texture;
                Microsoft.Xna.Framework.Graphics.Texture2D bg = TextureManager.GetBackground(Backgrounds.MainWindow);
                Microsoft.Xna.Framework.Graphics.Texture2D cutBg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

                MainWindow = new MainWindow(
                    bg,
                    new Rectangle(25, 25, cutBg.Width + 10, cutBg.Height),
                    new Rectangle(35, 14, cutBg.Width - 10, cutBg.Height - 10))
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "❤",
                    Subtitle = "❤",
                    SavesPosition = true,
                    Id = $"CharactersWindow",
                    CanResize = true,
                };

                MainWindow.Resized += MainWindow_Resized;
                MainWindow.Size = Settings.CurrentWindowSize;

                CreateCharacterControls();

                PotraitCapture = new CharacterPotraitCapture() { Parent = GameService.Graphics.SpriteScreen, Visible = false, ZIndex = 999 };
                OCR = new OCR();
            }
        }

        private void MainWindow_Resized(object sender, ResizedEventArgs e)
        {
            Settings.WindowSize.Value = MainWindow.Size;
        }
    }
}