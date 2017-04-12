using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Renci.SshNet;

using IronPortIntegration.Exceptions;

namespace IronPortIntegration
{
    public class IronPortGrepMailLogCommand : IronPortSSHCommand
    {
        private static string MailLogFolder = "mail_logs"; 

        public IronPortGrepMailLogCommand(string query)
        {
            CommandText = string.Format(_supportedCommands[IronPortSupportedCommand.GrepLogFile], query, MailLogFolder);
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

                return result;
            }
        }

        public override Task<string> ExecuteAsync(IronPortShell sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
