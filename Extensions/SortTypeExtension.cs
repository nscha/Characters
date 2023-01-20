using static Kenedia.Modules.Characters.Services.SettingsModel;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class SortTypeExtension
    {
        public static ESortType GetSortType(this string s)
        {
            if(s == string.Format(Strings.common.SortBy, Strings.common.Name))
            {
                return ESortType.SortByName;
            }
            else if (s == string.Format(Strings.common.SortBy, Strings.common.Tag))
            {
                return ESortType.SortByTag;
            }
            else if (s == string.Format(Strings.common.SortBy, Strings.common.Profession))
            {
                return ESortType.SortByProfession;
            }
            else if (s == string.Format(Strings.common.SortBy, Strings.common.LastLogin))
            {
                return ESortType.SortByLastLogin;
            }
            else if (s == string.Format(Strings.common.SortBy, Strings.common.Map))
            {
                return ESortType.SortByMap;
            }

            return ESortType.Custom;
        }

        public static string GetSortType(this ESortType st)
        {
            return st switch
            {
                ESortType.SortByName => string.Format(Strings.common.SortBy, Strings.common.Name),
                ESortType.SortByTag => string.Format(Strings.common.SortBy, Strings.common.Tag),
                ESortType.SortByProfession => string.Format(Strings.common.SortBy, Strings.common.Profession),
                ESortType.SortByLastLogin => string.Format(Strings.common.SortBy, Strings.common.LastLogin),
                ESortType.SortByMap => string.Format(Strings.common.SortBy, Strings.common.Map),
                ESortType.Custom => Strings.common.Custom,
                _ => Strings.common.Custom,
            };
        }

        public static ESortOrder GetSortOrder(this string s)
        {
            return s == Strings.common.Descending ? ESortOrder.Descending : ESortOrder.Ascending;
        }

        public static string GetSortOrder(this ESortOrder so)
        {
            return so switch
            {
                ESortOrder.Ascending => Strings.common.Ascending,
                ESortOrder.Descending => Strings.common.Descending,
                _ => Strings.common.Ascending,
            };
        }

        public static FilterBehavior GetFilterBehavior(this string s)
        {
            return s == Strings.common.ExcludeMatches ? FilterBehavior.Exclude : FilterBehavior.Include;
        }

        public static string GetFilterBehavior(this FilterBehavior fb)
        {
            return fb switch
            {
                FilterBehavior.Include => Strings.common.IncludeMatches,
                FilterBehavior.Exclude => Strings.common.ExcludeMatches,
                _ => Strings.common.IncludeMatches,
            };
        }

        public static MatchingBehavior GetMatchingBehavior(this string s)
        {
            return s == Strings.common.MatchAllFilter ? MatchingBehavior.MatchAll : MatchingBehavior.MatchAny;
        }

        public static string GetMatchingBehavior(this MatchingBehavior fb)
        {
            return fb switch
            {
                MatchingBehavior.MatchAny => Strings.common.MatchAnyFilter,
                MatchingBehavior.MatchAll => Strings.common.MatchAllFilter,
                _ => Strings.common.MatchAnyFilter,
            };
        }

        public static string GetPanelSize(this PanelSizes s)
        {
            return s switch
            {
                PanelSizes.Small => Strings.common.Small,
                PanelSizes.Normal => Strings.common.Normal,
                PanelSizes.Large => Strings.common.Large,
                _ => Strings.common.Normal,
            };
        }

        public static PanelSizes GetPanelSize(this string s)
        {
            if(s == Strings.common.Small)
            {
                return PanelSizes.Small;
            }
            else if(s == Strings.common.Large)
            {
                return PanelSizes.Large;
            }

            return PanelSizes.Normal;
        }

        public static string GetPanelLayout(this CharacterPanelLayout layout)
        {
            return layout switch
            {
                CharacterPanelLayout.OnlyIcons => Strings.common.OnlyIcons,
                CharacterPanelLayout.OnlyText => Strings.common.OnlyText,
                CharacterPanelLayout.IconAndText => Strings.common.TextAndIcon,
                _ => Strings.common.TextAndIcon,
            };
        }

        public static CharacterPanelLayout GetPanelLayout(this string layout)
        {
            if(layout == Strings.common.OnlyIcons)
            {
                return CharacterPanelLayout.OnlyIcons;
            }
            else if(layout == Strings.common.OnlyText)
            {
                return CharacterPanelLayout.OnlyText;
            }

            return CharacterPanelLayout.IconAndText;
        }
    }
}
