using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPortIntegration.Common;

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

        private List<string> GetAllRecipientsByMID(int MID)
        {
            const string messageDetailsQueryFormat = "MID {0}";
            
            var grepCommand = new IronPortGrepMailLogCommand(string.Format(messageDetailsQueryFormat, MID));
            var resultString = grepCommand.Execute(_sshClient);

            return _parser.GetRecipientsFromSmtpConversation(resultString);
        }

        public List<string> GetAllRecipientsOfSubject(string mailSubject)
        {
            const string subjectQueryFormat = "Subject '{0}'";

            var recipients = new List<string>();

            var grepCommand = new IronPortGrepMailLogCommand(string.Format(subjectQueryFormat, mailSubject));
            var resultRows = grepCommand.Execute(_sshClient).Split(IronPortCommon.UnixLineBreak).ToList();

            // Remove empty strings, ignore return value cause it does not matter now
            resultRows.Remove(string.Empty);

            // Parse all the rows from the grep result to log entries in order to get their message ID
            List<GrepLogEntry> logEntries = _parser.ParseRows(resultRows);

            foreach(var entry in logEntries)
            {
                recipients.AddRange(GetAllRecipientsByMID(entry.MID));
            }

            return recipients;
        }
    }
}
