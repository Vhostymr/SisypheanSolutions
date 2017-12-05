using System;

namespace SisypheanSolutions.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Replaces the first instance of a substring to the end of the first instance of a second substring.
        /// </summary>
        /// <param name="input">The string being manipulated.</param>
        /// <param name="from">The starting substring.</param>
        /// <param name="to">The ending substring.</param>
        /// <param name="replacement">The replacement substring.</param>
        /// <returns>Returns the altered initial string.</returns>
        public static string ReplaceRange(this string input, string from, string to, string replacement = "")
        {
            int offset = to.Length;
            int firstIndex = input.IndexOf(from, StringComparison.Ordinal);
            int secondIndex = input.IndexOf(to, firstIndex, StringComparison.Ordinal);

            if (firstIndex == -1 || secondIndex == -1) return input;

            int finalIndex = (secondIndex - firstIndex) + offset;

            return input.Remove(firstIndex, finalIndex).Insert(firstIndex, replacement);
        }

        /// <summary>
        /// Replaces all instances of a substring for each substring pair.
        /// </summary>
        /// <param name="input">The string being manipulated.</param>
        /// <param name="from">The starting substring.</param>
        /// <param name="to">The ending substring.</param>
        /// <param name="replacement">The replacement substring.</param>
        /// <returns>Returns the altered initial string.</returns>
        public static string ReplaceAll(this string input, string from, string to, string replacement = "")
        {
            while (input.Contains(from))
            {
                input = input.ReplaceRange(from, to, replacement);
            }

            return input;
        }
    }
}