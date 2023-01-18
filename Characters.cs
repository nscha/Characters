namespace Kenedia.Modules.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Blish_HUD;
    using Blish_HUD.Controls;
    using Blish_HUD.Controls.Extern;
    using Blish_HUD.Modules;
    using Blish_HUD.Modules.Managers;
    using Blish_HUD.Settings;
    using Gw2Sharp.WebApi.V2.Models;
    using Kenedia.Modules.Characters.Classes;
    using Kenedia.Modules.Characters.Classes.Classes.UI_Controls;
    using Kenedia.Modules.Characters.Classes.MainWindow;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Input;
    using Newtonsoft.Json;
    using static Kenedia.Modules.Characters.Classes.WindowsUtil.WindowsUtil;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    [Export(typeof(Module))]
    public class Characters : Module
    {
        public const uint WMCOMMAND = 0x0111;
        public const uint WMPASTE = 0x0302;

        public static readonly Logger Logger = Logger.GetLogger<Characters>();

        private Ticks ticks = new ();

        private CornerIcon cornerIcon;
        private bool dataLoaded;
        private RECT windowRectangle;
        private Point clientRes = Point.Zero;
        private RECT clientRectangle;

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
            get => this.dataLoaded;
            set
            {
                this.dataLoaded = value;
                if (value)
                {
                    ModuleInstance.OnDataLoaded();
                }
            }
        }

        public RECT WindowRectangle { get => this.windowRectangle; set => this.windowRectangle = value; }

        public RECT ClientRectangle { get => this.clientRectangle; set => this.clientRectangle = value; }

        public int TitleBarHeight { get; private set; }

        public int SideBarWidth { get; private set; }

        internal static Characters ModuleInstance { get; set; }

        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;

        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;

        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;

        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;

        [DllImport("user32")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public void OnLanguageChanged(object sender, EventArgs e)
        {
            this.LanguageChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnResolutionChanged(object sender, EventArgs e, bool fireEvent = true)
        {
            var hWnd = GameService.GameIntegration.Gw2Instance.Gw2WindowHandle;
            GetWindowRect(hWnd, ref this.windowRectangle);
            GetClientRect(hWnd, out this.clientRectangle);

            this.TitleBarHeight = this.WindowRectangle.Bottom - this.WindowRectangle.Top - (this.ClientRectangle.Bottom - this.ClientRectangle.Top);
            this.SideBarWidth = this.WindowRectangle.Right - this.WindowRectangle.Left - (this.ClientRectangle.Right - this.ClientRectangle.Left);

            if (fireEvent)
            {
                this.ResolutionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void FixCharacterOrder()
        {
            if (this.CharacterSorting == null)
            {
                this.CharacterSorting = new CharacterSorting(this.CharacterModels);
                if (this.Settings.AutoSortCharacters.Value)
                {
                    this.CharacterSwapping?.Reset();
                }
                else
                {
                    this.CharacterSwapping = null;
                }

                this.CharacterSorting.Finished += (sender, e) =>
                {
                    this.CharacterSorting = null;
                    this.MainWindow.SortCharacters();
                };
            }
        }

        public void SwapTo(Character_Model character)
        {
            var player = GameService.Gw2Mumble.PlayerCharacter;

            if (character.Name != player.Name || !GameService.GameIntegration.Gw2Instance.IsInGame)
            {
                if (this.CharacterSwapping != null)
                {
                }

                this.CharacterSwapping = new CharacterSwapping(character);
                this.CharacterSwapping.Succeeded += (sender, e) =>
                {
                    this.CharacterSwapping = null;
                    this.ForceUpdate(null, null);
                    this.MainWindow.SortCharacters();
                };
                this.CharacterSwapping.Failed += this.CharacterSwapping_Failed;
            }
        }

        public void CreateCharacterControls()
        {
            if (this.MainWindow != null)
            {
                foreach (Character_Model c in this.CharacterModels)
                {
                    if (this.MainWindow.CharacterControls.Find(e => e.Character.Name == c.Name) == null)
                    {
                        this.MainWindow.CharacterControls.Add(new CharacterControl()
                        {
                            Character = c,
                            Parent = this.MainWindow.ContentPanel,
                            ZIndex = this.MainWindow.ZIndex + 1,
                        });
                    }
                }

                this.MainWindow.FilterCharacters();
                this.MainWindow.UpdateLayout();
            }
        }

        public bool LoadCharacterList()
        {
            try
            {
                if (System.IO.File.Exists(this.CharactersPath))
                {
                    var infos = new FileInfo(this.CharactersPath);
                    var content = System.IO.File.ReadAllText(this.CharactersPath);
                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    List<Character_Model> characters = JsonConvert.DeserializeObject<List<Character_Model>>(content);

                    if (characters != null)
                    {
                        this.CharacterModels = characters;
                        foreach (Character_Model c in this.CharacterModels)
                        {
                            foreach (string t in c.Tags)
                            {
                                if (!this.Tags.Contains(t))
                                {
                                    this.Tags.Add(t);
                                }
                            }

                            c.Initialize();
                        }

                        this.DataLoaded = true;
                        return true;
                    }
                }

                this.DataLoaded = true;
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "Failed to load the local characters from file '" + this.CharactersPath + "'.");
                System.IO.File.Copy(this.CharactersPath, this.CharactersPath.Replace(".json", " [" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString() + "].corruped.json"));
                this.DataLoaded = true;
                return false;
            }
        }

        public void SaveCharacterList()
        {
            Logger.Debug("Saving Character List.");
            var data = new List<JsonCharacter_Model>();

            foreach (Character_Model c in this.CharacterModels)
            {
                data.Add(new JsonCharacter_Model(c));
            }

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;

            string json = JsonConvert.SerializeObject(data.ToArray(), Formatting.Indented, settings);

            // write string to file
            System.IO.File.WriteAllText(this.CharactersPath, json);
        }

        protected override void Update(GameTime gameTime)
        {
            this.ticks.Global += gameTime.ElapsedGameTime.TotalMilliseconds;
            this.ticks.APIUpdate += gameTime.ElapsedGameTime.TotalSeconds;
            this.ticks.Save += gameTime.ElapsedGameTime.TotalMilliseconds;
            this.ticks.Tags += gameTime.ElapsedGameTime.TotalMilliseconds;
            this.ticks.OCR += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (this.clientRes != GameService.Graphics.Resolution)
            {
                this.clientRes = GameService.Graphics.Resolution;
                this.OnResolutionChanged(null, null);
            }

            if (this.CharacterSorting != null)
            {
                this.CharacterSorting.Run(gameTime);
            }
            else if (this.CharacterSwapping != null)
            {
                this.CharacterSwapping.Run(gameTime);
            }

            if (this.ticks.Global > 15000)
            {
                this.ticks.Global = 0;
                this.CurrentCharacterModel = null;

                if (GameService.GameIntegration.Gw2Instance.IsInGame)
                {
                    var player = GameService.Gw2Mumble.PlayerCharacter;
                    this.CurrentCharacterModel = this.CharacterModels.Find(e => e.Name == player.Name);

                    if (this.CurrentCharacterModel != null)
                    {
                        this.CurrentCharacterModel.Specialization = (SpecializationType)player.Specialization;
                        this.CurrentCharacterModel.Map = GameService.Gw2Mumble.CurrentMap.Id;
                        this.CurrentCharacterModel.LastLogin = DateTime.UtcNow;
                        this.MainWindow?.SortCharacters();
                    }
                }
            }

            if (this.ticks.APIUpdate > 300)
            {
                this.ticks.APIUpdate = 0;

                this.GW2APIHandler.CheckAPI();
            }

            if (this.ticks.Save > 25 && this.SaveCharacters)
            {
                this.ticks.Save = 0;

                this.SaveCharacterList();
                this.SaveCharacters = false;
            }

            if (this.ticks.Tags >= 10 && this.UpdateTags)
            {
                this.ticks.Tags = 0;

                this.UpdateTags = false;
                this.UpdateTagsCollection();
            }

            if (this.ticks.OCR >= 50 && this.RunOCR)
            {
                this.ticks.OCR = 0;

                if (this.OCR == null)
                {
                    this.OCR = new OCR();
                }

                if (this.OCR != null && this.OCR.Read() != null)
                {
                    this.RunOCR = false;
                    if (this.RunOCR)
                    {
                        this.OCR.ToggleContainer();
                    }
                }
            }
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            this.Settings = new SettingsModel(settings);
            this.Settings.ShowCornerIcon.SettingChanged += this.ShowCornerIcon_SettingChanged;

            this.ReloadKey = settings.DefineSetting(
                nameof(this.ReloadKey),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Alt, Keys.R));

            this.ReloadKey.Value.Enabled = true;
            this.ReloadKey.Value.Activated += this.ReloadKey_Activated;
        }

        protected override void Initialize()
        {
            Logger.Info($"Starting  {this.Name} v." + this.Version.BaseVersion());

            ModKeyMapping = new VirtualKeyShort[5];
            ModKeyMapping[(int)ModifierKeys.Ctrl] = VirtualKeyShort.CONTROL;
            ModKeyMapping[(int)ModifierKeys.Alt] = VirtualKeyShort.MENU;
            ModKeyMapping[(int)ModifierKeys.Shift] = VirtualKeyShort.LSHIFT;

            // string path = DirectoriesManager.GetFullDirectoryPath("characters") + @"\" + API_Account.Name;
            this.BasePath = this.DirectoriesManager.GetFullDirectoryPath("characters");

            if (!System.IO.File.Exists(this.BasePath + @"\gw2.traineddata"))
            {
                using Stream target = System.IO.File.Create(this.BasePath + @"\gw2.traineddata");
                Stream source = this.ContentsManager.GetFileStream(@"data\gw2.traineddata");
                source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            if (!System.IO.File.Exists(this.BasePath + @"\tesseract.dll"))
            {
                using Stream target = System.IO.File.Create(this.BasePath + @"\tesseract.dll");
                Stream source = this.ContentsManager.GetFileStream(@"data\tesseract.dll");
                source.Seek(0, SeekOrigin.Begin);
                source.CopyTo(target);
            }

            this.Data = new Data();

            this.Gw2ApiManager.SubtokenUpdated += this.Gw2ApiManager_SubtokenUpdated;

            this.Settings.ShortcutKey.Value.Enabled = true;
            this.Settings.ShortcutKey.Value.Activated += this.ShortcutWindowToggle;
        }

        protected override async Task LoadAsync()
        {
            await Task.Delay(0);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            this.TextureManager = new TextureManager();

            if (this.Settings.ShowCornerIcon.Value)
            {
                this.CreateCornerIcons();
            }

            this.DataLoaded_Event += this.Characters_DataLoaded_Event;

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged += this.ForceUpdate;
            player.NameChanged += this.ForceUpdate;

            var map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged += this.ForceUpdate;
            this.clientRes = GameService.Graphics.Resolution;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged += this.ForceUpdate;
            OverlayService.Overlay.UserLocale.SettingChanged += this.UserLocale_SettingChanged;

            this.Tags.CollectionChanged += this.Tags_CollectionChanged;
            this.OnResolutionChanged(null, null, false);

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        protected override void Unload()
        {
            this.MainWindow?.Dispose();
            this.cornerIcon?.Dispose();

            this.TextureManager?.Dispose();
            this.TextureManager = null;

            if (this.cornerIcon != null)
            {
                this.cornerIcon.Click -= this.ToggleModule;
            }

            this.DataLoaded_Event -= this.Characters_DataLoaded_Event;
            OverlayService.Overlay.UserLocale.SettingChanged -= this.UserLocale_SettingChanged;

            var player = GameService.Gw2Mumble.PlayerCharacter;
            player.SpecializationChanged -= this.ForceUpdate;
            player.NameChanged -= this.ForceUpdate;

            var map = GameService.Gw2Mumble.CurrentMap;
            map.MapChanged -= this.ForceUpdate;

            GameService.GameIntegration.Gw2Instance.IsInGameChanged -= this.ForceUpdate;

            ModuleInstance = null;
        }

        private void OnDataLoaded()
        {
            this.DataLoaded_Event?.Invoke(this, EventArgs.Empty);
            if (this.MainWindow == null)
            {
                this.CreateUI();
            }
        }

        private void ShortcutWindowToggle(object sender, EventArgs e)
        {
            if (!(Control.ActiveControl is TextBox))
            {
                this.MainWindow?.ToggleWindow();
            }
        }

        private void Gw2ApiManager_SubtokenUpdated(object sender, ValueEventArgs<IEnumerable<TokenPermission>> e)
        {
            this.GW2APIHandler.CheckAPI();
        }

        private void ToggleWindow_Activated(object sender, EventArgs e)
        {
            this.MainWindow?.ToggleWindow();
        }

        private void ReloadKey_Activated(object sender, EventArgs e)
        {
            this.MainWindow?.Dispose();
            this.PotraitCapture?.Dispose();
            this.OCR?.Dispose();
            this.CreateUI(true);
            this.MainWindow?.ToggleWindow();
        }

        private void Tags_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.DataLoaded)
            {
                this.UpdateTagsCollection();
            }
        }

        private void UpdateTagsCollection()
        {
            foreach (Character_Model c in this.CharacterModels)
            {
                var tList = new ObservableCollection<string>(c.Tags);

                foreach (string t in tList)
                {
                    if (!this.Tags.Contains(t))
                    {
                        c.Tags.Remove(t);
                    }
                }
            }
        }

        private void ForceUpdate(object sender, EventArgs e)
        {
            this.ticks.Global = 2000000;
            if (this.CurrentCharacterModel != null)
            {
                this.CurrentCharacterModel.LastLogin = DateTime.UtcNow;
            }

            this.CurrentCharacterModel = null;
        }

        private void CornerIcon_Click(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            this.MainWindow?.ToggleWindow();
            if (this.MainWindow == null)
            {
                ScreenNotification.ShowNotification("New API Request sent...");
                this.GW2APIHandler.CheckAPI();
            }
        }

        private void Characters_DataLoaded_Event(object sender, EventArgs e)
        {
            this.CreateUI();
        }

        private void ToggleModule(object sender, Blish_HUD.Input.MouseEventArgs e)
        {
            if (this.MainWindow != null)
            {
                this.MainWindow.ToggleWindow();
            }
        }

        private void CreateCornerIcons()
        {
            this.cornerIcon = new CornerIcon()
            {
                Icon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156678),
                HoverIcon = GameService.Content.DatAssetCache.GetTextureFromAssetId(156679),
                BasicTooltipText = string.Format(Strings.common.Toggle, $"{this.Name}"),
                Parent = GameService.Graphics.SpriteScreen,
                Visible = this.Settings.ShowCornerIcon.Value,
            };

            this.APISpinner = new LoadingSpinner()
            {
                Location = new Point(this.cornerIcon.Left, this.cornerIcon.Bottom + 3),
                Parent = GameService.Graphics.SpriteScreen,
                Size = new Point(this.cornerIcon.Width, this.cornerIcon.Height),
                BasicTooltipText = "Fetching API data ...",
                Visible = false,
            };

            this.cornerIcon.Click += this.CornerIcon_Click;
            this.cornerIcon.Moved += this.CornerIcon_Moved;
        }

        private void CornerIcon_Moved(object sender, MovedEventArgs e)
        {
            this.APISpinner.Location = new Point(this.cornerIcon.Left, this.cornerIcon.Bottom + 3);
        }

        private void ShowCornerIcon_SettingChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue && this.cornerIcon == null)
            {
                this.CreateCornerIcons();
            }
            else if (this.cornerIcon != null && !e.NewValue)
            {
                this.cornerIcon.Moved -= this.CornerIcon_Moved;
                this.cornerIcon.Click -= this.CornerIcon_Click;
                this.cornerIcon.Dispose();
                this.cornerIcon = null;

                this.APISpinner.Dispose();
                this.APISpinner = null;
            }
        }

        private void CharacterSwapping_Failed(object sender, EventArgs e)
        {
            ScreenNotification.ShowNotification("Failed to swap to " + this.CharacterSwapping.Character.Name + "!");
            if (this.Settings.AutoSortCharacters.Value)
            {
                ScreenNotification.ShowNotification("Fixing Characters!");
                this.FixCharacterOrder();
            }

            this.CharacterSwapping.Failed -= this.CharacterSwapping_Failed;
        }

        private void UserLocale_SettingChanged(object sender, ValueChangedEventArgs<Gw2Sharp.WebApi.Locale> e)
        {
            this.cornerIcon.BasicTooltipText = string.Format(Strings.common.Toggle, $"{this.Name}");
            this.OnLanguageChanged(null, null);
        }

        private void CreateUI(bool force = false)
        {
            if (this.MainWindow == null || force)
            {
                var bg = GameService.Content.DatAssetCache.GetTextureFromAssetId(155985).Texture;
                bg = this.TextureManager.GetBackground(Backgrounds.MainWindow);
                bg = bg.GetRegion(25, 25, bg.Width - 100, bg.Height - 325);

                this.MainWindow = new MainWindow(
                    bg,
                    new Rectangle(25, 25, bg.Width + 10, bg.Height),
                    new Rectangle(35, 14, bg.Width - 10, bg.Height - 10))
                {
                    Parent = GameService.Graphics.SpriteScreen,
                    Title = "❤",
                    Subtitle = "❤",
                    SavesPosition = true,
                    Id = $"CharactersWindow",
                    CanResize = true,
                };

                this.MainWindow.Resized += this.MainWindow_Resized;
                this.MainWindow.Size = this.Settings.CurrentWindowSize;

                this.CreateCharacterControls();

                this.PotraitCapture = new CharacterPotraitCapture() { Parent = GameService.Graphics.SpriteScreen, Visible = false, ZIndex = 999 };
                this.OCR = new OCR();
            }
        }

        private void MainWindow_Resized(object sender, ResizedEventArgs e)
        {
            this.Settings.WindowSize.Value = this.MainWindow.Size;
        }
    }
}