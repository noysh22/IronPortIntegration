using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Renci.SshNet;

using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort
{
    public class IronPortGrepMailLogCommand : IronPortSshCommand
    {
        private static string MailLogFolder = "mail_logs"; 

        public IronPortGrepMailLogCommand(string query)
        {
            CommandText = string.Format(_supportedCommands[IronPortSupportedCommand.GrepLogFile], query, MailLogFolder);
            CommandResult = null;
        }

        public override bool Succeeded
        {
            get
            {
                const string failedGrepResultIndicator = "No results were found. Use another regular expression(s) to search.";
                if (null == CommandResult)
                    return false;

                return !CommandResult.Contains(failedGrepResultIndicator);
            }
        }

        public override string Execute(IronPortShell sshClient)
        {
            using (var sshCommand = sshClient.CreateCommand(CommandText))
            {
                Debug.WriteLine("Executing command {0}...", CommandText);
                var result = sshCommand.Execute();
                Debug.WriteLine("Command {0} done", CommandText);

                if (0 != sshCommand.ExitStatus || !string.IsNullOrEmpty(sshCommand.Error))
                {
                    throw new IronPortSshCommandException(sshCommand.ExitStatus, sshCommand.Error);
                }

                // Save the command result
                CommandResult = result;

                return result;
            }
        }

        public override Task<string> ExecuteAsync(IronPortShell sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
