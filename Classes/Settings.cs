using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Gw2Sharp.WebApi.V2.Models;
using Kenedia.Modules.Characters.Classes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Kenedia.Modules.Characters.Classes
{
    public static class SortTypeExtension
    {
        public static SortType GetSortType(this string s)
        {
            switch (s)
            {
                case "Sort By Name":
                    return SortType.SortByName;

                case "Sort By Tag":
                    return SortType.SortByTag;

                case "Sort By Profession":
                    return SortType.SortByProfession;

                case "Sort By Last Login":
                    return SortType.SortByLastLogin;

                case "Sort By Map":
                    return SortType.SortByMap;

                case "Custom":
                    return SortType.Custom;
            }

            return SortType.Custom;
        }

        public static string GetSortType(this SortType st)
        {
            switch (st)
            {
                case SortType.SortByName:
                    return "Sort By Name";

                case SortType.SortByTag:
                    return "Sort By Tag";

                case SortType.SortByProfession:
                    return "Sort By Profession";

                case SortType.SortByLastLogin:
                    return "Sort By Login";

                case SortType.SortByMap:
                    return "Sort By Map";

                case SortType.Custom:
                    return "Custom";
            }
            return "";
        }

        public static SortOrder GetSortOrder(this string s)
        {
            switch (s)
            {
                case "Sort Ascending":
                    return SortOrder.Ascending;

                case "Sort Descending":
                    return SortOrder.Descending;
            }

            return SortOrder.Ascending;
        }

        public static string GetSortOrder(this SortOrder so)
        {
            switch (so)
            {
                case SortOrder.Ascending:
                    return "Ascending";

                case SortOrder.Descending:
                    return "Descending";
            }

            return "Ascending";
        }

        public static FilterBehavior GetFilterBehavior(this string s)
        {
            switch (s)
            {
                case "Include Filters":
                    return FilterBehavior.Include;

                case "Exclude Filters":
                    return FilterBehavior.Exclude;
            }

            return FilterBehavior.Include;
        }
        public static string GetFilterBehavior(this FilterBehavior fb)
        {
            switch (fb)
            {
                case FilterBehavior.Include:
                    return "Include Filters";

                case FilterBehavior.Exclude:
                    return "Exclude Filters";
            }

            return "IncludeFilters";
        }

        public static MatchingBehavior GetMatchingBehavior(this string s)
        {
            switch (s)
            {
                case "Match Any Filter":
                    return MatchingBehavior.MatchAny;

                case "Match All Filter":
                    return MatchingBehavior.MatchAll;
            }

            return MatchingBehavior.MatchAny;
        }
        public static string GetMatchingBehavior(this MatchingBehavior fb)
        {
            switch (fb)
            {
                case MatchingBehavior.MatchAny:
                    return "Match Any Filter";

                case MatchingBehavior.MatchAll:
                    return "Match All Filter";
            }

            return "Match Any Filter";
        }
    }

    public enum SortOrder
    {
        Ascending,
        Descending,
    }
    public enum SortType
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
        Exclude
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

    public class SettingsModel
    {
        public bool ShowCornerIcon;
        public SettingEntry<bool> _ShowCornerIcon;
        public Point WindowSize => _WindowSize.Value;
        public SettingEntry<Point> _WindowSize;
        public SettingEntry<bool> _AutoSortCharacters;
        public SettingEntry<bool> _UseOCR;
        public SettingEntry<bool> _WindowedMode;
        public Rectangle OCRRegion
        {
            get
            {
                var key = GameService.Graphics.Resolution.ToString();
                var regions = _OCRRegions.Value;

                return regions.ContainsKey(key) ? regions[key] : new Rectangle(0, 0, 200, 50);
            }
        }
        public SettingEntry<Rectangle> _OCRRegion;
        public SettingEntry<Dictionary<string, Rectangle>> _OCRRegions;
        public SettingEntry<Rectangle> _OCRCustomOffset;
        public SettingEntry<int> _OCRNoPixelColumns;

        public SettingEntry<PanelSizes> PanelSize;
        public SettingEntry<CharacterPanelLayout> PanelLayout;
        public SettingEntry<bool> Show_DetailedTooltip;
        public SettingEntry<bool> Show_Name;
        public SettingEntry<bool> Show_Level;
        public SettingEntry<bool> Show_Race;
        public SettingEntry<bool> Show_Profession;
        public SettingEntry<bool> Show_LastLogin;
        public SettingEntry<bool> Show_Map;
        public SettingEntry<bool> Show_Crafting;
        public SettingEntry<bool> Show_OnlyMaxCrafting;
        public SettingEntry<bool> Show_Tags;

        public SettingEntry<MatchingBehavior> FilterMatching;
        public SettingEntry<FilterBehavior> FilterDirection;
        public SettingEntry<bool> Check_Name;
        public SettingEntry<bool> Check_Level;
        public SettingEntry<bool> Check_Race;
        public SettingEntry<bool> Check_Profession;
        public SettingEntry<bool> Check_Map;
        public SettingEntry<bool> Check_Crafting;
        public SettingEntry<bool> Check_OnlyMaxCrafting;
        public SettingEntry<bool> Check_Tags;

        public SettingEntry<SortType> Sort_Type;
        public SettingEntry<SortOrder> Sort_Order;

        public SettingEntry<Blish_HUD.Input.KeyBinding> LogoutKey;
        public SettingEntry<Blish_HUD.Input.KeyBinding> ShortcutKey;
        //public SettingEntry<Blish_HUD.Input.KeyBinding> SwapModifier;

        public SettingEntry<bool> EnterOnSwap;
        public SettingEntry<bool> DoubleClickToEnter;
        public SettingEntry<bool> EnterToLogin;

        public SettingEntry<int> SwapDelay;
        public SettingEntry<int> FilterDelay;

        public SettingsModel(SettingCollection settings)
        {
            LogoutKey = settings.DefineSetting(nameof(LogoutKey),
                                                     new Blish_HUD.Input.KeyBinding(Keys.F12),
                                                     () => Strings.common.Logout,
                                                     () => Strings.common.LogoutDescription);

            ShortcutKey = settings.DefineSetting(nameof(ShortcutKey),
                                                     new Blish_HUD.Input.KeyBinding(ModifierKeys.Shift, Keys.C),
                                                     () => Strings.common.ShortcutToggle_DisplayName,
                                                     () => Strings.common.ShortcutToggle_Description);

            //SwapModifier = settings.DefineSetting(nameof(SwapModifier),
            //                                         new Blish_HUD.Input.KeyBinding(Keys.None),
            //                                         () => Strings.common.SwapModifier_DisplayName,
            //                                         () => Strings.common.SwapModifier_Description);

            _ShowCornerIcon = settings.DefineSetting(nameof(_ShowCornerIcon),
                                                          true,
                                                          () => Strings.common.ShowCorner_Name,
                                                          () => string.Format(Strings.common.ShowCorner_Tooltip, Characters.ModuleInstance.Name));

            EnterOnSwap = settings.DefineSetting(nameof(EnterOnSwap),
                                                              true,
                                                              () => Strings.common.EnterOnSwap_DisplayName,
                                                              () => Strings.common.EnterOnSwap_Description);

            DoubleClickToEnter = settings.DefineSetting(nameof(DoubleClickToEnter),
                                                              false,
                                                              () => Strings.common.DoubleClickToEnter_DisplayName,
                                                              () => Strings.common.DoubleClickToEnter_Description);

            EnterToLogin = settings.DefineSetting(nameof(EnterToLogin),
                                                              false,
                                                              () => Strings.common.EnterToLogin_DisplayName,
                                                              () => Strings.common.EnterToLogin_Description);

            SwapDelay = settings.DefineSetting(nameof(SwapDelay),
                                                              500,
                                                              () => string.Format(Strings.common.SwapDelay_DisplayName, SwapDelay.Value),
                                                              () => Strings.common.SwapDelay_Description);
            SwapDelay.SetRange(0, 5000);

            FilterDelay = settings.DefineSetting(nameof(FilterDelay),
                                                              0,
                                                              () => string.Format(Strings.common.FilterDelay_DisplayName, FilterDelay.Value),
                                                              () => Strings.common.FilterDelay_Description);

            FilterDelay.SetRange(0, 500);

            var internalSettings = settings.AddSubCollection("Internal", false, false);
            _WindowSize = internalSettings.DefineSetting(nameof(WindowSize), new Point(385, 920));

            var res = GameService.Graphics.Resolution;
            _WindowedMode = internalSettings.DefineSetting(nameof(_WindowedMode), false);
            _UseOCR = internalSettings.DefineSetting(nameof(_UseOCR), false);
            _AutoSortCharacters = internalSettings.DefineSetting(nameof(_AutoSortCharacters), false);
            _OCRRegion = internalSettings.DefineSetting(nameof(OCRRegion), new Rectangle((res.X - 200) / 2, (res.Y - 250), 200, 200));
            _OCRRegions = internalSettings.DefineSetting(nameof(_OCRRegions), new Dictionary<string, Rectangle>());
            _OCRCustomOffset = internalSettings.DefineSetting(nameof(_OCRCustomOffset), new Rectangle(3, 3, 5, 5));
            _OCRNoPixelColumns = internalSettings.DefineSetting(nameof(_OCRNoPixelColumns), 20);

            PanelSize = internalSettings.DefineSetting(nameof(PanelSize), PanelSizes.Normal);
            PanelLayout = internalSettings.DefineSetting(nameof(PanelLayout), CharacterPanelLayout.IconAndText);

            Show_Name = internalSettings.DefineSetting(nameof(Show_Name), true);
            Show_Level = internalSettings.DefineSetting(nameof(Show_Level), true);
            Show_Race = internalSettings.DefineSetting(nameof(Show_Race), false);
            Show_Profession = internalSettings.DefineSetting(nameof(Show_Profession), false);
            Show_LastLogin = internalSettings.DefineSetting(nameof(Show_LastLogin), true);
            Show_Map = internalSettings.DefineSetting(nameof(Show_Map), true);
            Show_Crafting = internalSettings.DefineSetting(nameof(Show_Crafting), false);
            Show_OnlyMaxCrafting = internalSettings.DefineSetting(nameof(Show_OnlyMaxCrafting), true);
            Show_DetailedTooltip = internalSettings.DefineSetting(nameof(Show_DetailedTooltip), true);
            Show_Tags = internalSettings.DefineSetting(nameof(Show_Tags), true);

            FilterMatching = internalSettings.DefineSetting(nameof(FilterMatching), MatchingBehavior.MatchAny);
            FilterDirection = internalSettings.DefineSetting(nameof(FilterDirection), FilterBehavior.Include);
            Check_Name = internalSettings.DefineSetting(nameof(Check_Name), true);
            Check_Level = internalSettings.DefineSetting(nameof(Check_Level), true);
            Check_Race = internalSettings.DefineSetting(nameof(Check_Race), true);
            Check_Profession = internalSettings.DefineSetting(nameof(Check_Profession), true);
            Check_Map = internalSettings.DefineSetting(nameof(Check_Map), true);
            Check_Crafting = internalSettings.DefineSetting(nameof(Check_Crafting), true);
            Check_OnlyMaxCrafting = internalSettings.DefineSetting(nameof(Check_OnlyMaxCrafting), true);
            Check_Tags = internalSettings.DefineSetting(nameof(Check_Tags), true);

            Sort_Type = internalSettings.DefineSetting(nameof(Sort_Type), SortType.SortByLastLogin);
            Sort_Order = internalSettings.DefineSetting(nameof(Sort_Order), SortOrder.Ascending);
        }
    }
}
