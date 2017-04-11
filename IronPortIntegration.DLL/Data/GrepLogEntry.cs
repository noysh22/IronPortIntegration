using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace IronPortIntegration
{
    public class GrepLogEntry
    {
        private static string DateRegexString = @"^(?:\s*(Sun|Mon|Tue|Wed|Thu|Fri|Sat)\s*)?(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+(0?[1-9]|[1-2][0-9]|3[01])\s+(2[0-3]|[0-1][0-9]):([0-5][0-9])(?::(60|[0-5][0-9]))\s+(19[0-9]{2}|[2-9][0-9]{3}|[0-9]{2})";
        private static Regex DateRegex;

        private static string SeverityRegexString = @"\s(Info|Warning):\s";
        private static Regex SeverityRegex;

        private static string MIDRegexString = @"\s(MID)\s([1-9]+)\s";
        private static Regex MIDRegex;

        private static int RegexTimeoutSeconds = 30;

        public int MID { get; private set; }
        public string DateTimeAsString { get; private set; }
        public string Severity { get; private set; }
        public string RawData { get; private set; }

        static GrepLogEntry()
        {
            DateRegex = new Regex(DateRegexString,
                RegexOptions.Singleline | RegexOptions.Compiled,
                new TimeSpan(0, 0, RegexTimeoutSeconds));

            SeverityRegex = new Regex(SeverityRegexString,
                RegexOptions.Singleline | RegexOptions.Compiled,
                new TimeSpan(0, 0, RegexTimeoutSeconds));

            MIDRegex = new Regex(MIDRegexString,
                RegexOptions.Singleline | RegexOptions.Compiled,
                new TimeSpan(0, 0, RegexTimeoutSeconds));
        }

        public GrepLogEntry(string rawData)
        {
            RawData = rawData;
            DateTimeAsString = ParseLogDate(RawData);
            Severity = ParseLogSeverity(RawData);
            MID = ParseMID(rawData);
        }

        private static string ParseLogDate(string rawData)
        {
            var matches = DateRegex.Matches(rawData);

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            return matches[0].Value;
        }

        private static string ParseLogSeverity(string rawData)
        {
            var matches = SeverityRegex.Matches(rawData);

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            return matches[0].Value.Replace(":", string.Empty).Trim();
        }

        private static int ParseMID(string rawData)
        {
            var matches = MIDRegex.Matches(rawData);

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            var midStr = matches[0].Value.Trim();

            return int.Parse(midStr.Split(' ').Last());
        }

        public override string ToString()
        {
            return string.Format("DateTime: {0}\nSeverity: {1}\nMID: {2}\nRaw: {3}",
                DateTimeAsString,
                Severity,
                MID,
                RawData);
        }
    }
}
