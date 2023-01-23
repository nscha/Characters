using Blish_HUD;
using Blish_HUD.Settings;
using Kenedia.Modules.Characters.Models;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Services
{
    public class SettingsModel
    {
        public SettingsModel(SettingCollection settings)
        {
            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);
            LogoutKey = internalSettings.DefineSetting(nameof(LogoutKey), new Blish_HUD.Input.KeyBinding(Keys.F12));
            ShortcutKey = internalSettings.DefineSetting(nameof(ShortcutKey), new Blish_HUD.Input.KeyBinding(ModifierKeys.Shift, Keys.C));
            ShowCornerIcon = internalSettings.DefineSetting(nameof(ShowCornerIcon), true);
            CloseWindowOnSwap = internalSettings.DefineSetting(nameof(CloseWindowOnSwap), false);
            FilterDiacriticsInsensitive = internalSettings.DefineSetting(nameof(FilterDiacriticsInsensitive), false);
            ShowRandomButton = internalSettings.DefineSetting(nameof(ShowRandomButton), false);
            ShowStatusWindow = internalSettings.DefineSetting(nameof(ShowStatusWindow), true);
            EnterOnSwap = internalSettings.DefineSetting(nameof(EnterOnSwap), true);
            DoubleClickToEnter = internalSettings.DefineSetting(nameof(DoubleClickToEnter), false);
            EnterToLogin = internalSettings.DefineSetting(nameof(EnterToLogin), false);
            SwapDelay = internalSettings.DefineSetting(nameof(SwapDelay), 250);
            KeyDelay = internalSettings.DefineSetting(nameof(KeyDelay), 0);
            FilterDelay = internalSettings.DefineSetting(nameof(FilterDelay), 0);
            WindowSize = internalSettings.DefineSetting(nameof(CurrentWindowSize), new Point(385, 920));            
            DisplayToggles = internalSettings.DefineSetting(nameof(DisplayToggles), new Dictionary<string, ShowCheckPair>());

            Point res = GameService.Graphics.Resolution;
            WindowedMode = internalSettings.DefineSetting(nameof(WindowedMode), false);
            PinSideMenus = internalSettings.DefineSetting(nameof(PinSideMenus), true);
            UseOCR = internalSettings.DefineSetting(nameof(UseOCR), false);
            AutoSortCharacters = internalSettings.DefineSetting(nameof(AutoSortCharacters), false);
            OCRRegion = internalSettings.DefineSetting(nameof(ActiveOCRRegion), new Rectangle(50, 550, 530, 50));
            OCRRegions = internalSettings.DefineSetting(nameof(OCRRegions), new Dictionary<string, Rectangle>());
            OCRCustomOffset = internalSettings.DefineSetting(nameof(OCRCustomOffset), new RectangleOffset(3, 3, 5, 5));
            OCRNoPixelColumns = internalSettings.DefineSetting(nameof(OCRNoPixelColumns), 20);

            PanelSize = internalSettings.DefineSetting(nameof(PanelSize), PanelSizes.Normal);
            PanelLayout = internalSettings.DefineSetting(nameof(PanelLayout), CharacterPanelLayout.IconAndText);

            ShowName = internalSettings.DefineSetting(nameof(ShowName), true);
            ShowLevel = internalSettings.DefineSetting(nameof(ShowLevel), true);
            ShowRace = internalSettings.DefineSetting(nameof(ShowRace), false);
            ShowProfession = internalSettings.DefineSetting(nameof(ShowProfession), false);
            ShowLastLogin = internalSettings.DefineSetting(nameof(ShowLastLogin), true);
            ShowMap = internalSettings.DefineSetting(nameof(ShowMap), true);
            ShowCrafting = internalSettings.DefineSetting(nameof(ShowCrafting), false);
            ShowOnlyMaxCrafting = internalSettings.DefineSetting(nameof(ShowOnlyMaxCrafting), true);
            ShowDetailedTooltip = internalSettings.DefineSetting(nameof(ShowDetailedTooltip), true);
            ShowTags = internalSettings.DefineSetting(nameof(ShowTags), true);

            ResultMatchingBehavior = internalSettings.DefineSetting(nameof(ResultMatchingBehavior), MatchingBehavior.MatchAny);
            ResultFilterBehavior = internalSettings.DefineSetting(nameof(ResultFilterBehavior), FilterBehavior.Include);
            CheckName = internalSettings.DefineSetting(nameof(CheckName), true);
            CheckLevel = internalSettings.DefineSetting(nameof(CheckLevel), true);
            CheckRace = internalSettings.DefineSetting(nameof(CheckRace), true);
            CheckProfession = internalSettings.DefineSetting(nameof(CheckProfession), true);
            CheckMap = internalSettings.DefineSetting(nameof(CheckMap), true);
            CheckCrafting = internalSettings.DefineSetting(nameof(CheckCrafting), true);
            CheckOnlyMaxCrafting = internalSettings.DefineSetting(nameof(CheckOnlyMaxCrafting), true);
            CheckTags = internalSettings.DefineSetting(nameof(CheckTags), true);

            SortType = internalSettings.DefineSetting(nameof(SortType), ESortType.SortByLastLogin);
            SortOrder = internalSettings.DefineSetting(nameof(SortOrder), ESortOrder.Ascending);
        }

        public enum ESortOrder
        {
            Ascending,
            Descending,
        }

        public enum ESortType
        {
            SortByName,
            SortByTag,
            SortByProfession,
            SortByLastLogin,
            SortByMap,
            Custom,
        }

        public enum FilterBehavior
        {
            Include,
            Exclude,
        }

        public enum MatchingBehavior
        {
            MatchAny,
            MatchAll,
        }

        public enum CharacterPanelLayout
        {
            OnlyIcons,
            OnlyText,
            IconAndText,
        }

        public enum PanelSizes
        {
            Small,
            Normal,
            Large,
            Custom,
        }

        public SettingEntry<Dictionary<string, ShowCheckPair>> DisplayToggles { get; set; }

        public SettingEntry<bool> PinSideMenus { get; set; }

        public SettingEntry<bool> CloseWindowOnSwap{ get; set; }

        public SettingEntry<bool> FilterDiacriticsInsensitive { get; set; }

        public SettingEntry<bool> ShowRandomButton{ get; set; }

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public Point CurrentWindowSize => WindowSize.Value;

        public SettingEntry<Point> WindowSize { get; set; }

        public SettingEntry<bool> ShowStatusWindow { get; set; }

        public SettingEntry<bool> AutoSortCharacters { get; set; }

        public SettingEntry<bool> UseOCR { get; set; }

        public SettingEntry<bool> WindowedMode { get; set; }

        public Rectangle ActiveOCRRegion
        {
            get
            {
                string key = GameService.Graphics.Resolution.ToString();
                Dictionary<string, Rectangle> regions = OCRRegions.Value;

                return regions.ContainsKey(key) ? regions[key] : new Rectangle(0, 0, 200, 50);
            }
        }

        public SettingEntry<Rectangle> OCRRegion { get; set; }

        public SettingEntry<Dictionary<string, Rectangle>> OCRRegions { get; set; }

        public SettingEntry<RectangleOffset> OCRCustomOffset { get; set; }

        public SettingEntry<int> OCRNoPixelColumns { get; set; }

        public SettingEntry<PanelSizes> PanelSize { get; set; }

        public SettingEntry<CharacterPanelLayout> PanelLayout { get; set; }

        public SettingEntry<bool> ShowDetailedTooltip { get; set; }

        public SettingEntry<bool> ShowName { get; set; }

        public SettingEntry<bool> ShowLevel { get; set; }

        public SettingEntry<bool> ShowRace { get; set; }

        public SettingEntry<bool> ShowProfession { get; set; }

        public SettingEntry<bool> ShowLastLogin { get; set; }

        public SettingEntry<bool> ShowMap { get; set; }

        public SettingEntry<bool> ShowCrafting { get; set; }

        public SettingEntry<bool> ShowOnlyMaxCrafting { get; set; }

        public SettingEntry<bool> ShowTags { get; set; }

        public SettingEntry<MatchingBehavior> ResultMatchingBehavior { get; set; }

        public SettingEntry<FilterBehavior> ResultFilterBehavior { get; set; }

        public SettingEntry<bool> CheckName { get; set; }

        public SettingEntry<bool> CheckLevel { get; set; }

        public SettingEntry<bool> CheckRace { get; set; }

        public SettingEntry<bool> CheckProfession { get; set; }

        public SettingEntry<bool> CheckMap { get; set; }

        public SettingEntry<bool> CheckCrafting { get; set; }

        public SettingEntry<bool> CheckOnlyMaxCrafting { get; set; }

        public SettingEntry<bool> CheckTags { get; set; }

        public SettingEntry<ESortType> SortType { get; set; }

        public SettingEntry<ESortOrder> SortOrder { get; set; }

        public SettingEntry<Blish_HUD.Input.KeyBinding> LogoutKey { get; set; }

        public SettingEntry<Blish_HUD.Input.KeyBinding> ShortcutKey { get; set; }

        public SettingEntry<bool> EnterOnSwap { get; set; }

        public SettingEntry<bool> DoubleClickToEnter { get; set; }

        public SettingEntry<bool> EnterToLogin { get; set; }

        public SettingEntry<int> SwapDelay { get; set; }

        public SettingEntry<int> KeyDelay { get; set; }

        public SettingEntry<int> FilterDelay { get; set; }
    }
}
