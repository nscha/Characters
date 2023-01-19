namespace Kenedia.Modules.Characters.Services
{
    using System.Collections.Generic;
    using Blish_HUD;
    using Blish_HUD.Settings;
    using Microsoft.Xna.Framework.Input;
    using Point = Microsoft.Xna.Framework.Point;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;

    public class SettingsModel
    {
        public SettingsModel(SettingCollection settings)
        {
            this.LogoutKey = settings.DefineSetting(
                nameof(this.LogoutKey),
                new Blish_HUD.Input.KeyBinding(Keys.F12),
                () => Strings.common.Logout,
                () => Strings.common.LogoutDescription);

            this.ShortcutKey = settings.DefineSetting(
                nameof(this.ShortcutKey),
                new Blish_HUD.Input.KeyBinding(ModifierKeys.Shift, Keys.C),
                () => Strings.common.ShortcutToggle_DisplayName,
                () => Strings.common.ShortcutToggle_Description);

            this.ShowCornerIcon = settings.DefineSetting(
                nameof(this.ShowCornerIcon),
                true,
                () => Strings.common.ShowCorner_Name,
                () => string.Format(Strings.common.ShowCorner_Tooltip, Characters.ModuleInstance.Name));

            this.EnterOnSwap = settings.DefineSetting(
                nameof(this.EnterOnSwap),
                true,
                () => Strings.common.EnterOnSwap_DisplayName,
                () => Strings.common.EnterOnSwap_Description);

            this.DoubleClickToEnter = settings.DefineSetting(
                nameof(this.DoubleClickToEnter),
                false,
                () => Strings.common.DoubleClickToEnter_DisplayName,
                () => Strings.common.DoubleClickToEnter_Description);

            this.EnterToLogin = settings.DefineSetting(
                nameof(this.EnterToLogin),
                false,
                () => Strings.common.EnterToLogin_DisplayName,
                () => Strings.common.EnterToLogin_Description);

            this.SwapDelay = settings.DefineSetting(
                nameof(this.SwapDelay),
                500,
                () => string.Format(Strings.common.SwapDelay_DisplayName, this.SwapDelay.Value),
                () => Strings.common.SwapDelay_Description);
            this.SwapDelay.SetRange(0, 5000);

            this.FilterDelay = settings.DefineSetting(
                nameof(this.FilterDelay),
                0,
                () => string.Format(Strings.common.FilterDelay_DisplayName, this.FilterDelay.Value),
                () => Strings.common.FilterDelay_Description);

            this.FilterDelay.SetRange(0, 500);

            var internalSettings = settings.AddSubCollection("Internal", false, false);
            this.WindowSize = internalSettings.DefineSetting(nameof(this.CurrentWindowSize), new Point(385, 920));

            var res = GameService.Graphics.Resolution;
            this.WindowedMode = internalSettings.DefineSetting(nameof(this.WindowedMode), false);
            this.UseOCR = internalSettings.DefineSetting(nameof(this.UseOCR), false);
            this.AutoSortCharacters = internalSettings.DefineSetting(nameof(this.AutoSortCharacters), false);
            this.OCRRegion = internalSettings.DefineSetting(nameof(this.ActiveOCRRegion), new Rectangle((res.X - 200) / 2, res.Y - 250, 200, 200));
            this.OCRRegions = internalSettings.DefineSetting(nameof(this.OCRRegions), new Dictionary<string, Rectangle>());
            this.OCRCustomOffset = internalSettings.DefineSetting(nameof(this.OCRCustomOffset), new Rectangle(3, 3, 5, 5));
            this.OCRNoPixelColumns = internalSettings.DefineSetting(nameof(this.OCRNoPixelColumns), 20);

            this.PanelSize = internalSettings.DefineSetting(nameof(this.PanelSize), PanelSizes.Normal);
            this.PanelLayout = internalSettings.DefineSetting(nameof(this.PanelLayout), CharacterPanelLayout.IconAndText);

            this.ShowName = internalSettings.DefineSetting(nameof(this.ShowName), true);
            this.ShowLevel = internalSettings.DefineSetting(nameof(this.ShowLevel), true);
            this.ShowRace = internalSettings.DefineSetting(nameof(this.ShowRace), false);
            this.ShowProfession = internalSettings.DefineSetting(nameof(this.ShowProfession), false);
            this.ShowLastLogin = internalSettings.DefineSetting(nameof(this.ShowLastLogin), true);
            this.ShowMap = internalSettings.DefineSetting(nameof(this.ShowMap), true);
            this.ShowCrafting = internalSettings.DefineSetting(nameof(this.ShowCrafting), false);
            this.ShowOnlyMaxCrafting = internalSettings.DefineSetting(nameof(this.ShowOnlyMaxCrafting), true);
            this.ShowDetailedTooltip = internalSettings.DefineSetting(nameof(this.ShowDetailedTooltip), true);
            this.ShowTags = internalSettings.DefineSetting(nameof(this.ShowTags), true);

            this.FilterMatching = internalSettings.DefineSetting(nameof(this.FilterMatching), MatchingBehavior.MatchAny);
            this.FilterDirection = internalSettings.DefineSetting(nameof(this.FilterDirection), FilterBehavior.Include);
            this.CheckName = internalSettings.DefineSetting(nameof(this.CheckName), true);
            this.CheckLevel = internalSettings.DefineSetting(nameof(this.CheckLevel), true);
            this.CheckRace = internalSettings.DefineSetting(nameof(this.CheckRace), true);
            this.CheckProfession = internalSettings.DefineSetting(nameof(this.CheckProfession), true);
            this.CheckMap = internalSettings.DefineSetting(nameof(this.CheckMap), true);
            this.CheckCrafting = internalSettings.DefineSetting(nameof(this.CheckCrafting), true);
            this.CheckOnlyMaxCrafting = internalSettings.DefineSetting(nameof(this.CheckOnlyMaxCrafting), true);
            this.CheckTags = internalSettings.DefineSetting(nameof(this.CheckTags), true);

            this.SortType = internalSettings.DefineSetting(nameof(this.SortType), ESortType.SortByLastLogin);
            this.SortOrder = internalSettings.DefineSetting(nameof(this.SortOrder), ESortOrder.Ascending);
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

        public SettingEntry<bool> ShowCornerIcon { get; set; }

        public Point CurrentWindowSize => this.WindowSize.Value;

        public SettingEntry<Point> WindowSize { get; set; }

        public SettingEntry<bool> AutoSortCharacters { get; set; }

        public SettingEntry<bool> UseOCR { get; set; }

        public SettingEntry<bool> WindowedMode { get; set; }

        public Rectangle ActiveOCRRegion
        {
            get
            {
                var key = GameService.Graphics.Resolution.ToString();
                var regions = this.OCRRegions.Value;

                return regions.ContainsKey(key) ? regions[key] : new Rectangle(0, 0, 200, 50);
            }
        }

        public SettingEntry<Rectangle> OCRRegion { get; set; }

        public SettingEntry<Dictionary<string, Rectangle>> OCRRegions { get; set; }

        public SettingEntry<Rectangle> OCRCustomOffset { get; set; }

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

        public SettingEntry<MatchingBehavior> FilterMatching { get; set; }

        public SettingEntry<FilterBehavior> FilterDirection { get; set; }

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

        public SettingEntry<int> FilterDelay { get; set; }
    }
}
