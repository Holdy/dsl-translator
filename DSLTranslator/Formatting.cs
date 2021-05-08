using System;
using System.Text.RegularExpressions;

namespace DSLTranslator
{
    public class Formatting
    {
        private static Regex bracketedPlaceholderRegex = new Regex(@"\(.*?\)", RegexOptions.Compiled);

        public static string SimplifySpacing(string input)
        {
            return Regex.Replace(input, @"\s+", " ", RegexOptions.Multiline);
        }

        public static MatchCollection FindPlaceholders(string input) {
            return bracketedPlaceholderRegex.Matches(input);
        }
    }
}
