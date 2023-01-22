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
            LogoutKey = settings.DefineSetting(
                nameof(LogoutKey),
                new Blish_HUD.Input.KeyBinding(Keys.F12),
                () => Strings.common.Logout,
                () => Strings.common.LogoutDescription);

            ShortcutKey = settings.DefineSetting(
                nameof(ShortcutKey),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Shift, Keys.C),
                () => Strings.common.ShortcutToggle_DisplayName,
                () => Strings.common.ShortcutToggle_Description);

            ShowCornerIcon = settings.DefineSetting(
                nameof(ShowCornerIcon),
                true,
                () => Strings.common.ShowCorner_Name,
                () => string.Format(Strings.common.ShowCorner_Tooltip, Characters.ModuleInstance.Name));

            CloseWindowOnSwap = settings.DefineSetting(
                nameof(CloseWindowOnSwap),
                false,
                () => Strings.common.CloseWindowOnSwap_DisplayName,
                () => Strings.common.CloseWindowOnSwap_Description);

            FilterDiacriticsInsensitive = settings.DefineSetting(
                nameof(FilterDiacriticsInsensitive),
                false,
                () => Strings.common.FilterDiacriticsInsensitive_DisplayName,
                () => Strings.common.FilterDiacriticsInsensitive_Description);

            ShowRandomButton = settings.DefineSetting(
                nameof(ShowRandomButton),
                false,
                () => Strings.common.ShowRandomButton_Name,
                () => Strings.common.ShowRandomButton_Description);

            ShowStatusWindow = settings.DefineSetting(
                nameof(ShowStatusWindow),
                true,
                () => Strings.common.ShowStatusWindow_Name,
                () => Strings.common.ShowStatusWindow_Description);

            EnterOnSwap = settings.DefineSetting(
                nameof(EnterOnSwap),
                true,
                () => Strings.common.EnterOnSwap_DisplayName,
                () => Strings.common.EnterOnSwap_Description);

            DoubleClickToEnter = settings.DefineSetting(
                nameof(DoubleClickToEnter),
                false,
                () => Strings.common.DoubleClickToEnter_DisplayName,
                () => Strings.common.DoubleClickToEnter_Description);

            EnterToLogin = settings.DefineSetting(
                nameof(EnterToLogin),
                false,
                () => Strings.common.EnterToLogin_DisplayName,
                () => Strings.common.EnterToLogin_Description);

            SwapDelay = settings.DefineSetting(
                nameof(SwapDelay),
                500,
                () => string.Format(Strings.common.SwapDelay_DisplayName, SwapDelay.Value),
                () => Strings.common.SwapDelay_Description);
            SwapDelay.SetRange(0, 5000);

            KeyDelay = settings.DefineSetting(
                nameof(KeyDelay),
                0,
                () => string.Format(Strings.common.KeyDelay_DisplayName, KeyDelay.Value),
                () => Strings.common.KeyDelay_Description);
            KeyDelay.SetRange(0, 500);

            FilterDelay = settings.DefineSetting(
                nameof(FilterDelay),
                0,
                () => string.Format(Strings.common.FilterDelay_DisplayName, FilterDelay.Value),
                () => Strings.common.FilterDelay_Description);

            FilterDelay.SetRange(0, 500);

            SettingCollection internalSettings = settings.AddSubCollection("Internal", false, false);
            WindowSize = internalSettings.DefineSetting(nameof(CurrentWindowSize), new Point(385, 920));

            DisplayToggles = internalSettings.DefineSetting(nameof(DisplayToggles), new Dictionary<string, ShowCheckPair>());

            Point res = GameService.Graphics.Resolution;
            WindowedMode = internalSettings.DefineSetting(nameof(WindowedMode), false);
            FadeOut = internalSettings.DefineSetting(nameof(FadeOut), true);
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

        public SettingEntry<bool> FadeOut { get; set; }

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
