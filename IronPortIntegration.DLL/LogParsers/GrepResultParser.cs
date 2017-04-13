using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

using IronPortIntegration.Common;

namespace IronPortIntegration
{
    public class GrepResultParser
    {
        private static string DateRegexString = @"^(?:\s*(Sun|Mon|Tue|Wed|Thu|Fri|Sat)\s*)?(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+(0?[1-9]|[1-2][0-9]|3[01])\s+(2[0-3]|[0-1][0-9]):([0-5][0-9])(?::(60|[0-5][0-9]))\s+(19[0-9]{2}|[2-9][0-9]{3}|[0-9]{2})";
        private static Regex DateRegex;

        private static string SeverityRegexString = @"\s(Info|Warning):\s";
        private static Regex SeverityRegex;

        private static string MIDRegexString = @"\s(MID)\s([1-9]+)\s";
        private static Regex MIDRegex;

        private static string ICIDRegexString = @"\s(ICID)\s([1-9]+)\s";
        private static Regex ICIDRegex;

        private static int RegexTimeoutSeconds = 30;

        static GrepResultParser()
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

            ICIDRegex = new Regex(ICIDRegexString,
                RegexOptions.Singleline | RegexOptions.Compiled,
                new TimeSpan(0, 0, RegexTimeoutSeconds));
        }

        public string ParseLogDate(string rawData)
        {
            var matches = DateRegex.Matches(rawData);

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            return matches[0].Value;
        }

        public string ParseLogSeverity(string rawData)
        {
            var matches = SeverityRegex.Matches(rawData);

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            return matches[0].Value.Replace(":", string.Empty).Trim();
        }

        public int ParseMID(string rawData)
        {
            var matches = MIDRegex.Matches(rawData);

            if (0 == matches.Count)
                return -1;

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            var midStr = matches[0].Value.Trim();

            return int.Parse(midStr.Split(' ').Last());
        }

        public int ParseICID(string rawData)
        {
            var matches = ICIDRegex.Matches(rawData);

            if (0 == matches.Count)
                return -1;

            if (1 < matches.Count)
            {
                throw new Exception("Invalid log entry, log cannot containt more than 1 date-time string");
            }

            var midStr = matches[0].Value.Trim();

            return int.Parse(midStr.Split(' ').Last());
        }

        public bool IsAbortedConversation(string smtpConversation, int MID)
        {
            const string abortedMessageIndicatodFormat = @"(Message\sfinished\sMID\s{0}\saborted)$";
            var match = Regex.Match(smtpConversation, string.Format(abortedMessageIndicatodFormat, MID));
            return match.Success;
        }

        public List<string> GetRecipientsFromSmtpConversation(string rawData)
        {
            const string toRegexExpression = @"(To:)\s(<[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}>)";

            var matches = Regex.Matches(rawData, toRegexExpression);

            if (0 == matches.Count)
                return null;

            var recipients = new List<string>();
            foreach(Match match in matches)
            {
                var recipient = match.Value.Split(IronPortCommon.Space).Last()
                    .Replace("<", string.Empty).Replace(">", string.Empty);

                recipients.Add(recipient);
            }

            return recipients;
        }

        public List<GrepLogEntry> ParseRows(List<string> resultRows)
        {
            var logEntries = new List<GrepLogEntry>();

            foreach(string row in resultRows)
            {
                logEntries.Add(new GrepLogEntry(row, this));
            }

            return logEntries;
        }
    }
}
