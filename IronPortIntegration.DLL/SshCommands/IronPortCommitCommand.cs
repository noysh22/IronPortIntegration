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
    public abstract class IronPortCommitCommand : IronPortSSHCommand
    {
        private string CommitCommandText;
        protected string CommitMessage;

        private string CommitCommandFormat = "{0} \"{1}\" y";

        public IronPortCommitCommand()
        {
            CommitCommandText = _supportedCommands[IronPortSupportedCommand.Commit];
            CommandResult = null;
        }

        private void Commit(IronPortShell sshClient)
        {
            var commitCmd = string.Format(CommitCommandFormat, CommitCommandText, CommitMessage);

            sshClient.RunShellCommand(commitCmd);
        }

        public string ExecuteAndCommit(IronPortShell sshClient)
        {
            var result = Execute(sshClient);

            if (null != result)
                Commit(sshClient);

            CommandResult = result;

            return result;
        }

    }
}
