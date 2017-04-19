using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Siemplify.Integrations.IronPort.Common;
using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort
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
            const string messageDetailsQueryFormat = "\"MID {0}\"";

            var grepCommand = new IronPortGrepMailLogCommand(string.Format(messageDetailsQueryFormat, MID));
            return grepCommand.Execute(_sshClient);
        }

        /// <summary>
        /// Build an extended Grep query to get all the relevant SMTP conversation from
        /// IronPort according to their MID
        /// </summary>
        /// <param name="entries">All log entries relevant for the grep query</param>
        /// <returns>All the rows return from the grep query</returns>
        private List<string> GetSmtpConversationsForEntries(List<GrepLogEntry> entries)
        {
            const string extendedMIDQueryFormat = "-e \"MID {0}\" ";

            // Build the query
            var extendedQueryString = new StringBuilder();
            foreach(var entry in entries)
            {
                extendedQueryString.AppendFormat(extendedMIDQueryFormat, entry.MID);
            }

            var extendedGrepCommand = new IronPortGrepMailLogCommand(extendedQueryString.ToString());
            var resultRows = extendedGrepCommand.Execute(_sshClient);

            return resultRows.Split(IronPortCommon.UnixLineBreak).ToList();
        }

        /// <summary>
        /// Get all the recipients of a given mail subject
        /// </summary>
        /// <param name="mailSubject">The mail subject to search</param>
        /// <returns>List of all the recipients</returns>
        /// TODO: Same effeciancy problem as written below, need to be improved 
        public List<string> GetAllRecipientsOfSubject(string mailSubject)
        {
            const string subjectQueryFormat = "\"Subject '{0}'\"";

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

            // Query for all the smtp conversations related to the given log entries list in one grep query
            var smtpConversationsRows = GetSmtpConversationsForEntries(logEntries);

            // Divide the result rows to conversation by their MID
            var smtpConversationByMID = _parser.ExtractSMTPConversations(smtpConversationsRows, logEntries);

            // Extract all the recipients from the conversations
            foreach (var conversation in smtpConversationByMID)
            {
                if (!_parser.IsAbortedConversation(conversation.SmtpConversationStr, conversation.MID))
                {
                    recipients = recipients.Union(_parser.GetRecipientsFromSmtpConversation(conversation.SmtpConversationStr)).ToList();
                }
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
            const string senderQeuryFormat = "\"From: <{0}>\"";

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

            // Query for all the smtp conversations related to the given log entries list in one grep query
            var smtpConversationsRows = GetSmtpConversationsForEntries(logEntries);

            // Divide the result rows to conversation by their MID
            var smtpConversationByMID = _parser.ExtractSMTPConversations(smtpConversationsRows, logEntries);

            // Extract all the recipients from the smtp conversations
            foreach (var conversation in smtpConversationByMID)
            {
                if (!_parser.IsAbortedConversation(conversation.SmtpConversationStr, conversation.MID))
                {
                    recipients = recipients.Union(_parser.GetRecipientsFromSmtpConversation(conversation.SmtpConversationStr)).ToList();
                }
            }

            return recipients;
        }
    }
}
