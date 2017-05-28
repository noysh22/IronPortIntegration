using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

using Siemplify.Integrations.IronPort.Common;
using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort
{
    public class GrepResultParser
    {
        #region Regexes definitions
        private static string DateRegexString = @"^(?:\s*(Sun|Mon|Tue|Wed|Thu|Fri|Sat)\s*)?(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+(0?[1-9]|[1-2][0-9]|3[01])\s+(2[0-3]|[0-1][0-9]):([0-5][0-9])(?::(60|[0-5][0-9]))\s+(19[0-9]{2}|[2-9][0-9]{3}|[0-9]{2})";
        private static Regex DateRegex;

        private static string SeverityRegexString = @"\s(Info|Warning):\s";
        private static Regex SeverityRegex;

        private static string MIDRegexString = @"\s(MID)\s([0-9]+)\s";
        private static Regex MIDRegex;

        private static string ICIDRegexString = @"\s(ICID)\s([0-9]+)\s";
        private static Regex ICIDRegex;

        private static int RegexTimeoutSeconds = 30;
        #endregion

        #region Static string formats
        private static string SmtpConversationStartFormat = @"(:\sStart\sMID\s{0}\s)";
        private static string SmtpAbortedMessageFormat = @"(Message\sfinished\sMID\s{0}\saborted)$";
        private static string SmtpDroppedMessageFormat = @"(:\sMessage\saborted\sMID\s{0}\sDropped\sby\sfilter\s)";
        private static string SmtpConversationDoneFormat = @"(Message\sfinished\sMID\s{0}\sdone)$";
        #endregion

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
            int result = int.Parse(midStr.Split(' ').Last());
            return result;
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

        /// <summary>
        /// Check if a given SMTP conversation is an aborted conversation or not
        /// </summary>
        /// <param name="smtpConversation">SMTP conversation as a string</param>
        /// <param name="MID">The message ID</param>
        /// <returns>True/False if an aborted SMTP conversation</returns>
        public bool IsAbortedConversation(string smtpConversation, int MID)
        {
            return Regex.IsMatch(smtpConversation, string.Format(SmtpAbortedMessageFormat, MID)) || 
                Regex.IsMatch(smtpConversation, string.Format(SmtpDroppedMessageFormat, MID));
        }

        /// <summary>
        /// Extract all the recipients from a given SMTP conversation
        /// </summary>
        /// <param name="smtpConversation">The SMTP conversation as a string</param>
        /// <returns>List of all the recipients addresses</returns>
        public List<string> GetRecipientsFromSmtpConversation(string smtpConversation)
        {
            const string toRegexExpression = @"(To:)\s(<[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}>)";

            var matches = Regex.Matches(smtpConversation, toRegexExpression);

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

        /// <summary>
        /// Transform grep string result to GrepLogEntry object
        /// </summary>
        /// <param name="resultRows">Grep result rows</param>
        /// <returns>List of GrepLogEntry</returns>
        public List<GrepLogEntry> ParseRows(List<string> resultRows)
        {
            var logEntries = new List<GrepLogEntry>();

            foreach(string row in resultRows)
            {
                logEntries.Add(new GrepLogEntry(row, this));
            }

            return logEntries;
        }

        private string ExtractSMTPConversation(List<string> resultRows, int MID)
        {
            string conversationStart = string.Format(SmtpConversationStartFormat, MID);
            string conversationAborted = string.Format(SmtpAbortedMessageFormat, MID);
            string conversationDone = string.Format(SmtpConversationDoneFormat, MID);

            int conversationStartIndex = resultRows.FindIndex(row => Regex.IsMatch(row, conversationStart));

            if (-1 == conversationStartIndex)
                throw new IronPortParsingException(string.Format("Failed to find conversation start in the results for MID {0}", MID));

            int conversationEndIndex = resultRows.FindIndex(row => Regex.IsMatch(row, conversationDone));

            // If success done conversation was not found, search for aborted smtp conversation
            if (-1 == conversationEndIndex)
            {
                conversationEndIndex = resultRows.FindIndex(row => Regex.IsMatch(row, conversationAborted));
                // smtp conversation must have an end either success or aborted
                if (-1 == conversationEndIndex)
                    throw new IronPortParsingException(string.Format("Failed to find conversation end in the results for MID {0}", MID));
            }

            return string.Join(IronPortCommon.UnixLineBreak.ToString(),
                resultRows.GetRange(conversationStartIndex, (conversationEndIndex - conversationStartIndex) + 1).ToArray());
            
        }

        /// <summary>
        /// Seperate SMTP conversation by MID which were returned from an extended grep query 
        /// </summary>
        /// <param name="resultRows">Grep query result rows</param>
        /// <param name="logEntries">List of GrepLogEntries relevant for the conversations</param>
        /// <returns>List of SmtpConversation objects</returns>
        public List<SmtpConversation> ExtractSMTPConversations(List<string> resultRows, List<GrepLogEntry> logEntries)
        {
            List<SmtpConversation> smtpConversations = new List<SmtpConversation>();

            foreach (var entry in logEntries)
            {
                var smtpConversation = ExtractSMTPConversation(resultRows, entry.MID);
                smtpConversations.Add(new SmtpConversation(entry.MID, smtpConversation));
            }

            return smtpConversations;
        }
    }
}
