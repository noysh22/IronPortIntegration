using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPortIntegration.Common;
using IronPortIntegration.Exceptions;

namespace IronPortIntegration
{
    /// <summary>
    /// Responsible for quering the iron port appliance and resolve data queries
    /// For example, find all the emails which were connected to a certain subject
    /// Or find all the recepients which recieved a mail from a certain sender
    /// </summary>
    public class IronPortQueryResolver
    {
        private IronPortShell _sshClient;
        private GrepResultParser _parser;

        public IronPortQueryResolver(IronPortShell sshClient, GrepResultParser parser = null)
        {
            _sshClient = sshClient;
            _parser = null != parser ? parser : new GrepResultParser();
        }

        private string GetSmtpConversationByMID(int MID)
        {
            const string messageDetailsQueryFormat = "MID {0}";

            var grepCommand = new IronPortGrepMailLogCommand(string.Format(messageDetailsQueryFormat, MID));
            return grepCommand.Execute(_sshClient);
        }

        /// <summary>
        /// Get all the recipients of a given mail subject
        /// </summary>
        /// <param name="mailSubject">The mail subject to search</param>
        /// <returns>List of all the recipients</returns>
        /// TODO: Same effeciancy problem as written below, need to be improved 
        public List<string> GetAllRecipientsOfSubject(string mailSubject)
        {
            const string subjectQueryFormat = "Subject '{0}'";

            var recipients = new List<string>();

            var grepCommand = new IronPortGrepMailLogCommand(string.Format(subjectQueryFormat, mailSubject));
            var resultRows = grepCommand.Execute(_sshClient).Split(IronPortCommon.UnixLineBreak).ToList();

            // Return null if command failed
            if (!grepCommand.Succeeded)
            {
                return null;
            }

            // Remove empty strings, ignore return value cause it does not matter now
            resultRows.Remove(string.Empty);

            // Parse all the rows from the grep result to log entries in order to get their message ID
            List<GrepLogEntry> logEntries = _parser.ParseRows(resultRows);

            // Get all the recipients from each smtp conversation with a given mid
            foreach(var entry in logEntries)
            {
                var resultString = GetSmtpConversationByMID(entry.MID);
                recipients = recipients.Union(_parser.GetRecipientsFromSmtpConversation(resultString)).ToList();
            }

            return recipients;
        }

        /// <summary>
        /// Return all the recipients who got mails from a given sender email
        /// </summary>
        /// <param name="senderMail">The sender email address</param>
        /// <returns>List of all the recipients</returns>
        /// TODO: This is not very efficient method can be improved by getting all
        /// TODO: the SMTP conversation in 1 query and then filter the conversations
        public List<string> GetAllRecipientsBySender(string senderMail)
        {
            const string senderQeuryFormat = "From: <{0}>";

            // TODO: Maybe validate if the parameter is valid email using regex?
            if (string.IsNullOrEmpty(senderMail))
                throw new IronPortException(string.Format("Invalid argument passed to {0} cannot get null or empty string",
                    System.Reflection.MethodBase.GetCurrentMethod()));

            List<string> recipients = new List<string>();

            var grepCommand = new IronPortGrepMailLogCommand(string.Format(senderQeuryFormat, senderMail));
            var resultRows = grepCommand.Execute(_sshClient).Split(IronPortCommon.UnixLineBreak).ToList();

            // Return null if command failed
            if (!grepCommand.Succeeded)
            {
                return null;
            }

            // Remove empty strings, ignore return value cause it does not matter now
            resultRows.Remove(string.Empty);

            // Parse all the rows from the grep result to log entries in order to get their message ID
            List<GrepLogEntry> logEntries = _parser.ParseRows(resultRows);

            foreach (var entry in logEntries)
            {
                var stmpConversation = GetSmtpConversationByMID(entry.MID);
                if(!_parser.IsAbortedConversation(stmpConversation, entry.MID))
                {
                    recipients = recipients.Union(_parser.GetRecipientsFromSmtpConversation(stmpConversation)).ToList();
                }
            }

            return recipients;
        }
    }
}
