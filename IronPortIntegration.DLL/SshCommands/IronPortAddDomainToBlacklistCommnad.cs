using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Renci.SshNet;

using Siemplify.Integrations.IronPort.Common;
using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort
{
    public class IronPortAddDomainToBlacklistCommand : IronPortCommitCommand
    {
        private static string LISTENER = "IncomingMail";
        private static string SENDERGROUP = "BLACKLIST";

        public string HostList;

        public IronPortAddDomainToBlacklistCommand(string hostList)
        {
            HostList = hostList;
            CommitMessage = string.Format("Adding host {0} to blacklist of {1}", HostList, LISTENER);

            CommitCommandText = string.Format(_supportedCommands[IronPortSupportedCommand.Commit], CommitMessage);
            CommandText = string.Format(_supportedCommands[IronPortSupportedCommand.AddDomainToBlackList],
                LISTENER,
                SENDERGROUP,
                HostList);

            CommandResult = null;
        }

        public override string Execute(IronPortShell sshClient)
        {
            return sshClient.RunShellCommand(CommandText);
        }

        public bool IsSucceeded()
        {
            string successString = CommandText + IronPortCommon.UnixLineBreak +
                IronPortShell.SHELL_READY_INDICATOR +
                CommitCommandText + IronPortCommon.UnixLineBreak +
                IronPortShell.SHELL_READY_INDICATOR;

            return CommandResult.Equals(successString);
        }

        public override Task<string> ExecuteAsync(IronPortShell sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
