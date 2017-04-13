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
    public class IronPortAddSenderToBlacklistCommand : IronPortCommitCommand
    {
        private static string LISTENER = "IncomingMail";
        private static string SENDERGROUP = "BLACKLIST";

        public string HostList;

        public IronPortAddSenderToBlacklistCommand(string hostList)
        {
            CommandText = _supportedCommands[IronPortSupportedCommand.AddSenderToBlackList];
            HostList = hostList;
            CommitMessage = string.Format("Adding host {0} to blacklist of {1}", HostList, LISTENER);
        }

        public override string Execute(IronPortShell sshClient)
        {
            var formattedCommand = string.Format(CommandText, LISTENER, SENDERGROUP, HostList);

            return sshClient.RunShellCommand(formattedCommand);
        }

        public override Task<string> ExecuteAsync(IronPortShell sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
