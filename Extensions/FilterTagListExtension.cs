namespace Kenedia.Modules.Characters.Extensions
{
    using System.Collections.Generic;
    using Kenedia.Modules.Characters.Controls;

    public static class FilterTagListExtension
    {
        public static List<FilterTag> CreateFilterTagList(this List<string> strings)
        {
            var list = new List<FilterTag>();
            foreach (string s in strings)
            {
                list.Add(new FilterTag()
                {
                    Tag = s,
                });
            }

            return list;
        }
    }
}
