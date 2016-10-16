using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Mendo.UWP.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrBlank(this string value)
        {
            return string.IsNullOrEmpty(value) ||
                (!string.IsNullOrEmpty(value) && value.Trim() == string.Empty);
        }

        public static bool EqualsIgnoreCase(this string left, string right)
        {
            return string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool EqualsAny(this string input, params string[] args)
        {
            return args.Aggregate(false, (current, arg) => current | input.Equals(arg));
        }

        public static string FormatWith(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string FormatWithInvariantCulture(this string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static string Then(this string input, string value)
        {
            return string.Concat(input, value);
        }

        public static string UrlEncode(this string value)
        {
            // [DC] This is more correct than HttpUtility; it escapes spaces as %20, not +
            return Uri.EscapeDataString(value);
        }

        public static string UrlDecode(this string value)
        {
            return Uri.UnescapeDataString(value);
        }

        public static Uri AsUri(this string value)
        {
            return new Uri(value);
        }

        public static string ToBase64String(this byte[] input)
        {
            return Convert.ToBase64String(input);
        }

        /// <summary>
        /// Converts a byte array to a string, using its byte order mark to convert it to the right encoding.
        /// http://www.shrinkrays.net/code-snippets/csharp/an-extension-method-for-converting-a-byte-array-to-a-string.aspx
        /// </summary>
        /// <param name="buffer">An array of bytes to convert</param>
        /// <returns>The byte as a string.</returns>
        public static string AsString(this byte[] input)
        {
            return input == null ? "" : Encoding.UTF8.GetString(input, 0, input.Length);

        }

        public static byte[] GetBytes(this string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string PercentEncode(this string s)
        {
            var bytes = s.GetBytes();
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                // [DC]: Support proper encoding of special characters (\n\r\t\b)
                if ((b > 7 && b < 11) || b == 13)
                {
                    sb.Append(string.Format("%0{0:X}", b));
                }
                else
                {
                    sb.Append(string.Format("%{0:X}", b));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts a string to pascal case with the option to remove underscores
        /// </summary>
        /// <param name="text">String to convert</param>
        /// <param name="removeUnderscores">Option to remove underscores</param>
        /// <returns></returns>
        public static string ToPascalCase(this string text, bool removeUnderscores = true)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            text = text.Replace("_", " ");
            string joinString = removeUnderscores ? String.Empty : "_";
            string[] words = text.Split(' ');
            if (words.Length > 1 || words[0].IsUpperCase())
            {
                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i].Length > 0)
                    {
                        string word = words[i];
                        string restOfWord = word.Substring(1);

                        if (restOfWord.IsUpperCase())
                        {    //restOfWord = restOfWord.ToLower(culture);
                            restOfWord = restOfWord.ToLower();
                        }

                        //char firstChar = char.ToUpper(word[0], culture);
                        char firstChar = char.ToUpper(word[0]);

                        words[i] = String.Concat(firstChar, restOfWord);
                    }
                }
                return String.Join(joinString, words);
            }
            //return String.Concat(words[0].Substring(0, 1).ToUpper(culture), words[0].Substring(1));
            return String.Concat(words[0].Substring(0, 1).ToUpper(), words[0].Substring(1));
        }

        /// <summary>
        /// Converts a string to camel case
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">String to convert</param>
        /// <returns>String</returns>
        public static string ToCamelCase(this string lowercaseAndUnderscoredWord)
        {
            return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
        }

        /// <summary>
        /// Convert the first letter of a string to lower case
        /// </summary>
        /// <param name="word">String to convert</param>
        /// <returns>string</returns>
        public static string MakeInitialLowerCase(this string word)
        {
            return string.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
        }

        /// <summary>
        /// Checks to see if a string is all uppper case
        /// </summary>
        /// <param name="inputString">String to check</param>
        /// <returns>bool</returns>
        public static bool IsUpperCase(this string inputString)
        {
            return Regex.IsMatch(inputString, @"^[A-Z]+$");
        }

        public static IDictionary<string, string> ParseQueryString(this string query)
        {
            // [DC]: This method does not URL decode, and cannot handle decoded input
            if (query.StartsWith("?")) query = query.Substring(1);

            if (query.Equals(string.Empty))
            {
                return new Dictionary<string, string>();
            }

            var parts = query.Split(new[] { '&' });

            return parts.Select(
                part => part.Split(new[] { '=' })).ToDictionary(
                    pair => pair[0], pair => pair[1]
                );
        }

        /// <summary>
        /// Remove underscores from a string
        /// </summary>
        /// <param name="input">String to process</param>
        /// <returns>string</returns>
        public static string RemoveUnderscoresAndDashes(this string input)
        {
            return input.Replace("_", "").Replace("-", "");
        }

        private const RegexOptions Options = RegexOptions.IgnoreCase;
    }
}
