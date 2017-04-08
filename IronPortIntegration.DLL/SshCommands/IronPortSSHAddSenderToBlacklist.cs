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
    class IronPortSSHAddSenderToBlacklist : IronPortSSHCommitCommand
    {
        private static string LISTENER = "IncomingMail";
        private static string SENDERGROUP = "BLACKLIST";

        public string HostList;

        public IronPortSSHAddSenderToBlacklist(string hostList)
        {
            CommandText = _supportedCommands[IronPortSupportedCommand.AddSenderToBlackList];
            HostList = hostList;
            CommitMessage = string.Format("Adding host {0} to blacklist of {1}", HostList, LISTENER);
        }

        public override string Execute(SshClient sshClient)
        {
            var formattedCommand = string.Format(CommandText, LISTENER, SENDERGROUP, HostList);

            using (var sshCommand = sshClient.CreateCommand(formattedCommand))
            {
                Debug.WriteLine(string.Format("Executing command {0}...", formattedCommand));
                var result = sshCommand.Execute();
                Debug.WriteLine(string.Format("Command {0} done", formattedCommand));

                if (0 != sshCommand.ExitStatus)
                {
                    throw new IronPortSshCommandException(sshCommand.ExitStatus, sshCommand.Error);
                }

                return result;
            }
        }

        public override Task<string> ExecuteAsync(SshClient sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
