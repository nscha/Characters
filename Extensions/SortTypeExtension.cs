using static Kenedia.Modules.Characters.Services.SettingsModel;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class SortTypeExtension
    {
        public static ESortType GetSortType(this string s)
        {
            return s switch
            {
                "Sort By Name" => ESortType.SortByName,
                "Sort By Tag" => ESortType.SortByTag,
                "Sort By Profession" => ESortType.SortByProfession,
                "Sort By Last Login" => ESortType.SortByLastLogin,
                "Sort By Map" => ESortType.SortByMap,
                "Custom" => ESortType.Custom,
                _ => ESortType.Custom,
            };
        }

        public static string GetSortType(this ESortType st)
        {
            return st switch
            {
                ESortType.SortByName => "Sort By Name",
                ESortType.SortByTag => "Sort By Tag",
                ESortType.SortByProfession => "Sort By Profession",
                ESortType.SortByLastLogin => "Sort By Login",
                ESortType.SortByMap => "Sort By Map",
                ESortType.Custom => "Custom",
                _ => string.Empty,
            };
        }

        public static ESortOrder GetSortOrder(this string s)
        {
            return s switch
            {
                "Sort Ascending" => ESortOrder.Ascending,
                "Sort Descending" => ESortOrder.Descending,
                _ => ESortOrder.Ascending,
            };
        }

        public static string GetSortOrder(this ESortOrder so)
        {
            return so switch
            {
                ESortOrder.Ascending => "Ascending",
                ESortOrder.Descending => "Descending",
                _ => "Ascending",
            };
        }

        public static FilterBehavior GetFilterBehavior(this string s)
        {
            return s switch
            {
                "Include Filters" => FilterBehavior.Include,
                "Exclude Filters" => FilterBehavior.Exclude,
                _ => FilterBehavior.Include,
            };
        }

        public static string GetFilterBehavior(this FilterBehavior fb)
        {
            return fb switch
            {
                FilterBehavior.Include => "Include Filters",
                FilterBehavior.Exclude => "Exclude Filters",
                _ => "IncludeFilters",
            };
        }

        public static MatchingBehavior GetMatchingBehavior(this string s)
        {
            return s switch
            {
                "Match Any Filter" => MatchingBehavior.MatchAny,
                "Match All Filter" => MatchingBehavior.MatchAll,
                _ => MatchingBehavior.MatchAny,
            };
        }

        public static string GetMatchingBehavior(this MatchingBehavior fb)
        {
            return fb switch
            {
                MatchingBehavior.MatchAny => "Match Any Filter",
                MatchingBehavior.MatchAll => "Match All Filter",
                _ => "Match Any Filter",
            };
        }
    }
}
