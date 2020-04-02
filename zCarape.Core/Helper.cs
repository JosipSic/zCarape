using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace zCarape.Core
{
    public static class Helper
    {
        public static DateTime ConvertToDateTimeFromSqLite(string str)
        {
            string pattern = @"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})";
            string bezMilisekundi = str.Substring(0, 19);
            if (Regex.IsMatch(bezMilisekundi, pattern))
            {
                Match match = Regex.Match(str, pattern);
                int year = Convert.ToInt32(match.Groups[1].Value);
                int month = Convert.ToInt32(match.Groups[2].Value);
                int day = Convert.ToInt32(match.Groups[3].Value);
                int hour = Convert.ToInt32(match.Groups[4].Value);
                int minute = Convert.ToInt32(match.Groups[5].Value);
                int second = Convert.ToInt32(match.Groups[6].Value);
                int millisecond = 0;
                return new DateTime(year, month, day, hour, minute, second, millisecond);
            }
            else
            {
                return DateTime.MinValue;
            }
        }

    }

}
