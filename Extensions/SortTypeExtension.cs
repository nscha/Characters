using static Kenedia.Modules.Characters.Services.SettingsModel;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class SortTypeExtension
    {
        public static ESortType GetSortType(this string s)
        {
            switch (s)
            {
                case "Sort By Name":
                    return ESortType.SortByName;

                case "Sort By Tag":
                    return ESortType.SortByTag;

                case "Sort By Profession":
                    return ESortType.SortByProfession;

                case "Sort By Last Login":
                    return ESortType.SortByLastLogin;

                case "Sort By Map":
                    return ESortType.SortByMap;

                case "Custom":
                    return ESortType.Custom;
            }

            return ESortType.Custom;
        }

        public static string GetSortType(this ESortType st)
        {
            switch (st)
            {
                case ESortType.SortByName:
                    return "Sort By Name";

                case ESortType.SortByTag:
                    return "Sort By Tag";

                case ESortType.SortByProfession:
                    return "Sort By Profession";

                case ESortType.SortByLastLogin:
                    return "Sort By Login";

                case ESortType.SortByMap:
                    return "Sort By Map";

                case ESortType.Custom:
                    return "Custom";
            }

            return string.Empty;
        }

        public static ESortOrder GetSortOrder(this string s)
        {
            switch (s)
            {
                case "Sort Ascending":
                    return ESortOrder.Ascending;

                case "Sort Descending":
                    return ESortOrder.Descending;
            }

            return ESortOrder.Ascending;
        }

        public static string GetSortOrder(this ESortOrder so)
        {
            switch (so)
            {
                case ESortOrder.Ascending:
                    return "Ascending";

                case ESortOrder.Descending:
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
}
