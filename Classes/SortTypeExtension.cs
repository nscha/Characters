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
}
