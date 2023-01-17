using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Kenedia.Modules.Characters.Classes.Classes.UI_Controls;
using Kenedia.Modules.Characters.Classes.MainWindow;
using Kenedia.Modules.Characters.Classes.UI_Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Patagames.Ocr;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters
{
    [Export(typeof(Module))]
    public class Characters : Module
    {
        internal static Characters ModuleInstance;
        public static readonly Logger Logger = Logger.GetLogger<Characters>();

        #region Service Managers

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        #endregion

        [ImportingConstructor]
        public Characters([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            ModuleInstance = this;
        }

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        public const uint WM_COMMAND = 0x0111;
        public const uint WM_PASTE = 0x0302;
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public SettingEntry<bool> ShowCornerIcon;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ReloadKey;
        public SettingsModel Settings;

        public string CultureString;
        public TextureManager TextureManager;
        public Ticks Ticks = new Ticks();

        public TagList Tags = new TagList();
        public MainWindow MainWindow;
        public CharacterPotraitCapture PotraitCapture;
        private CornerIcon CornerIcon;
        public LoadingSpinner APISpinner;

        public List<Character_Model> Character_Models = new List<Character_Model>();
        public Character_Model CurrentCharacter_Model;
        public CharacterSwapping characterSwapping;
        public CharacterSorting characterSorting;

        public string BasePath;
        public Data Data;
        public OCR OCR;
        public GW2API_Handler GW2API_Handler = new GW2API_Handler();

        public static VirtualKeyShort[] ModKeyMapping;

        public string CharactersPath;
        public string AccountInfoPath;
        public string AccountImagesPath;
        public string GlobalImagesPath;
        public string AccountPath;

        public bool _runOCR;
        public bool _updateTags;
        public bool SaveCharacters;
        private bool _DataLoaded;
        public bool FetchingAPI;
        public bool DataLoaded
        {
            get => _DataLoaded;
            set
            {
                _DataLoaded = value;
                if (value) ModuleInstance.OnDataLoaded();
            }
        }

        public event EventHandler DataLoaded_Event;
        void OnDataLoaded()
        {
            this.DataLoaded_Event?.Invoke(this, EventArgs.Empty);
            if (MainWindow == null) CreateUI();
        }

        public event EventHandler LanguageChanged;
        public void OnLanguageChanged(object sender, EventArgs e)
        {
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        Point _ClientRes = Point.Zero;
        public RECT WindowRectangle;
        public RECT ClientRectangle;
        public int TitleBarHeight;
        public int SideBarWidth;
        public event EventHandler ResolutionChanged;
        public void OnResolutionChanged(object sender, EventArgs e, bool fireEvent = true)
        {
            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            GetWindowRect(hWnd, ref WindowRectangle);
            GetClientRect(hWnd, out ClientRectangle);

            TitleBarHeight = WindowRectangle.Bottom - WindowRectangle.Top - (ClientRectangle.Bottom - ClientRectangle.Top);
            SideBarWidth = WindowRectangle.Right - WindowRectangle.Left - (ClientRectangle.Right - ClientRectangle.Left);

            if (fireEvent) this.ResolutionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void DefineSettings(SettingCollection settings)
        {

            Settings = new SettingsModel(settings);
            Settings._ShowCornerIcon.SettingChanged += ShowCornerIcon_SettingChanged;

            //ReloadKey = settings.DefineSetting(nameof(ReloadKey),
            //                                              new Blish_HUD.Input.KeyBinding(ModifierKeys.Alt, Keys.R));

            //ReloadKey.Value.Enabled = true;
            //ReloadKey.Value.Activated += ReloadKey_Activated;
        }

        protected override void Initialize()
        {
            Logger.Info($"Starting  {Name} v." + Version.BaseVersion());

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            //string path = DirectoriesManager.GetFullDirectoryPath("characters") + @"\" + API_Account.Name;
            BasePath = DirectoriesManager.GetFullDirectoryPath("characters");

            if (!System.IO.File.Exists((BasePath + @"\gw2.traineddata")))
                using (Stream target = System.IO.File.Create(BasePath + @"\gw2.traineddata"))
                {
                    Stream source = ContentsManager.GetFileStream(@"data\gw2.traineddata");
                    source.Seek(0, SeekOrigin.Begin);
                    source.CopyTo(target);
                }

            if (!System.IO.File.Exists((BasePath + @"\tesseract.dll")))
                using (Stream target = System.IO.File.Create(BasePath + @"\tesseract.dll"))
                {
                    Stream source = ContentsManager.GetFileStream(@"data\tesseract.dll");
                    source.Seek(0, SeekOrigin.Begin);
                    source.CopyTo(target);
                }

            Data = new Data();

            Gw2ApiManager.SubtokenUpdated += Gw2ApiManager_SubtokenUpdated;

            Settings.ShortcutKey.Value.Enabled = true;
            Settings.ShortcutKey.Value.Activated += ShortcutWindowToggle;
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (!(Control.ActiveControl is TextBox))
            {
                MainWindow?.ToggleWindow();
            }
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            GW2API_Handler.CheckAPI();
        }

        private void ToggleWindow_Activated(object sender, EventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void ReloadKey_Activated(object sender, EventArgs e)
        {
            MainWindow?.Dispose();
            PotraitCapture?.Dispose();
            OCR?.Dispose();
            CreateUI();
            MainWindow?.ToggleWindow();
        }

        protected override async Task LoadAsync()
        {

        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            TextureManager = new TextureManager();

            if(Settings._ShowCornerIcon.Value) CreateCornerIcons();
            DataLoaded_Event += Characters_DataLoaded_Event;

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += ForceUpdate;
            player.NameChanged += ForceUpdate;

            var map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged += ForceUpdate;
            _ClientRes = GameService.Graphics.Resolution;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged += ForceUpdate;
            OverlayService.Overlay.UserLocale.SettingChanged += UserLocale_SettingChanged;

            Tags.CollectionChanged += Tags_CollectionChanged;
            OnResolutionChanged(null, null, false);

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (DataLoaded) UpdateTags();
        }

        private void UpdateTags()
        {
            foreach (Character_Model c in Character_Models)
            {
                var tList = new ObservableCollection<string>(c.Tags);

                foreach (string t in tList)
                {
                    if (!Tags.Contains(t)) c.Tags.Remove(t);
                }
            }
        }

        private void ForceUpdate(object sender, EventArgs e)
        {
            Ticks.Global = 2000000;
            if (CurrentCharacter_Model != null) CurrentCharacter_Model.LastLogin = DateTime.UtcNow;
            CurrentCharacter_Model = null;
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            MainWindow?.ToggleWindow();
        }

        private void Characters_DataLoaded_Event(object sender, EventArgs e)
        {
            CreateUI();
        }

        private void ToggleModule(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (MainWindow != null) MainWindow.ToggleWindow();
        }

        private void CreateCornerIcons()
        {
            CornerIcon = new CornerIcon()
            {
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156678),
                HoverIcon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156679),
                BasicTooltipText = string.Format(Strings.common.Toggle, $"{Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = Settings.ShowCornerIcon,
            };

            APISpinner = new LoadingSpinner()
            {
                Location = new Point(CornerIcon.Left, CornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(CornerIcon.Width, CornerIcon.Height),
                BasicTooltipText = "Fetching API data ...",
                Visible = false,
            };

            CornerIcon.Click += CornerIcon_Click;
            CornerIcon.Moved += CornerIcon_Moved;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            APISpinner.Location = new Point(CornerIcon.Left, CornerIcon.Bottom + 3);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if(e.NewValue && CornerIcon == null)
            {
                Debug.WriteLine("SETTING CHANGED");
                CreateCornerIcons();
            }
            else if(CornerIcon != null && !e.NewValue)
            {
                CornerIcon.Moved -= CornerIcon_Moved;
                CornerIcon.Click -= CornerIcon_Click;
                CornerIcon.Dispose();
                CornerIcon = null;

                APISpinner.Dispose();
                APISpinner = null;
            }
        }

        public void FixCharacterOrder()
        {
            if (characterSorting == null)
            {
                characterSorting = new CharacterSorting(Character_Models);
                if(Settings._AutoSortCharacters.Value)
                {
                    characterSwapping?.Reset();
                }
                else
                {
                    characterSwapping = null;
                }
                characterSorting.Finished += delegate { characterSorting = null; MainWindow.SortCharacters(); };
            }
        }

        protected override void Update(GameTime gameTime)
        {
            Ticks.Global += gameTime.ElapsedGameTime.TotalMilliseconds;
            Ticks.API_Update += gameTime.ElapsedGameTime.TotalSeconds;
            Ticks.Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            Ticks.Tags += gameTime.ElapsedGameTime.TotalMilliseconds;
            Ticks.OCR += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_ClientRes != GameService.Graphics.Resolution)
            {
                _ClientRes = GameService.Graphics.Resolution;
                OnResolutionChanged(null, null);
            }

            if (characterSorting != null) characterSorting.Run(gameTime);
            else if (characterSwapping != null) characterSwapping.Run(gameTime);

            if (Ticks.Global > 15000)
            {
                Ticks.Global = 0;
                CurrentCharacter_Model = null;

                if (GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    CurrentCharacter_Model = Character_Models.Find(e => e.Name == player.Name);

                    if (CurrentCharacter_Model != null)
                    {
                        CurrentCharacter_Model.Specialization = (SpecializationType)player.Specialization;
                        CurrentCharacter_Model.Map = GameService.Gw2Mumble.CurrentMap.Id;
                        CurrentCharacter_Model.LastLogin = DateTime.UtcNow;
                        MainWindow?.SortCharacters();
                    }
                }
            }

            if (Ticks.API_Update > 300)
            {
                Ticks.API_Update = 0;

                GW2API_Handler.CheckAPI();
            }

            if (Ticks.Save > 25 && SaveCharacters)
            {
                Ticks.Save = 0;

                SaveCharacterList();
                SaveCharacters = false;
            }

            if (Ticks.Tags >= 10 && _updateTags)
            {
                Ticks.Tags = 0;

                _updateTags = false;
                UpdateTags();
            }

            if ((Ticks.OCR >= 50 && (_runOCR)))
            //if ((Ticks.OCR >= 1000 && (_runOCR)))
            {
                Ticks.OCR = 0;

                if(OCR == null)
                {
                    OCR = new OCR();
                }

                if (OCR != null && OCR.Read() != null)
                {
                    _runOCR = false;
                    if (_runOCR) OCR.ToggleContainer();
                }
            }
        }

        public void SwapTo(Character_Model character)
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (character.Name != player.Name || !GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                if (characterSwapping != null)
                {

                }

                characterSwapping = new CharacterSwapping(character);
                characterSwapping.Succeeded += delegate { characterSwapping = null; ForceUpdate(null, null); MainWindow.SortCharacters(); };
                characterSwapping.Failed += CharacterSwapping_Failed;
            }
        }

        private void CharacterSwapping_Failed(object sender, EventArgs e)
        {
            ScreenNotification.ShowNotification("Failed to swap to " + characterSwapping.Character.Name + "!");
            if (Settings._AutoSortCharacters.Value)
            {
                ScreenNotification.ShowNotification("Fixing Characters!");
                FixCharacterOrder();
            }
            characterSwapping.Failed -= CharacterSwapping_Failed;
        }

        protected override void Unload()
        {
            MainWindow?.Dispose();
            CornerIcon?.Dispose();

            TextureManager?.Dispose();
            TextureManager = null;

            if (CornerIcon != null) CornerIcon.Click -= ToggleModule;

            DataLoaded_Event -= Characters_DataLoaded_Event;
            OverlayService.Overlay.UserLocale.SettingChanged -= UserLocale_SettingChanged;

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged -= ForceUpdate;
            player.NameChanged -= ForceUpdate;

            var map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged -= ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= ForceUpdate;
             
            ModuleInstance = null;
        }

        private async void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e)
        {
            CornerIcon.BasicTooltipText = string.Format(Strings.common.Toggle, $"{Name}");

            OnLanguageChanged(null, null);
        }

        public void CreateCharacterControls()
        {
            if (MainWindow != null)
            {
                foreach (Character_Model c in Character_Models)
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
        private void CreateUI()
        {
            if (MainWindow == null)
            {
                var bg = GameService.Content.DatAssetCache.GetTextureFromAssetId(155985).Texture;
                bg = TextureManager.getBackground(_Backgrounds.MainWindow);
                bg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

                MainWindow = new MainWindow(
                    //GameService.Content.DatAssetCache.GetTextureFromAssetId(155985).Texture,
                    TextureManager.getBackground(_Backgrounds.MainWindow),
                    new Rectangle(25, 25, bg.Width + 10, bg.Height),
                    new Rectangle(35, 14, bg.Width - 10, bg.Height - 10)
                    )
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    //Title = $"{Name}",
                    Title = "❤",
                    //Emblem = GameService.Content.DatAssetCache.GetTextureFromAssetId(156015),
                    //Emblem = TextureManager.getEmblem(_Emblems.Characters),
                    Subtitle = "❤",
                    SavesPosition = true,
                    Id = $"CharactersWindow",
                    CanResize = true,
                };

                MainWindow.Resized += MainWindow_Resized;
                MainWindow.Size = Settings.WindowSize;

                CreateCharacterControls();

                //MainWindow.ToggleWindow();

                PotraitCapture = new CharacterPotraitCapture() { Parent = GameService.Graphics.SpriteScreen, Visible = false, ZIndex = 999 };
                OCR = new OCR();
            }
        }

        private void MainWindow_Resized(object sender, ResizedEventArgs e)
        {
            Settings._WindowSize.Value = MainWindow.Size;
        }

        public bool LoadCharacterList()
        {
            try
            {
                if (System.IO.File.Exists(CharactersPath))
                {
                    var infos = new FileInfo(CharactersPath);
                    var content = System.IO.File.ReadAllText(CharactersPath);
                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content);

                    if (characters != null)
                    {
                        Character_Models = characters;
                        foreach (Character_Model c in Character_Models)
                        {
                            foreach (string t in c.Tags)
                            {
                                if (!Tags.Contains(t)) Tags.Add(t);
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
            var data = new List<JsonCharacter_Model>();

            foreach (Character_Model c in Character_Models)
            {
                data.Add(new JsonCharacter_Model(c));
            }

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;

            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented, settings);

            //write string to file
            System.IO.File.WriteAllText(CharactersPath, json);
        }
    }
}