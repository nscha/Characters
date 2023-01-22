using System.Text;
using System.Text.RegularExpressions;

namespace Kenedia.Modules.Characters.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex s_diacritics = new(@"\p{M}");

        public static string RemoveDiacritics(this string s)
        {
            string result = s.Normalize(NormalizationForm.FormD);
            return s_diacritics.Replace(result, string.Empty);
        }
    }
}
