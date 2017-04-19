using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Renci.SshNet;

using Siemplify.Integrations.IronPort.Common;

namespace Siemplify.Integrations.IronPort
{
    public abstract class IronPortCommitCommand : IronPortSSHCommand
    {
        protected string CommitCommandText;
        protected string CommitMessage;

        private string Commit(IronPortShell sshClient)
        {
            return sshClient.RunShellCommand(CommitCommandText);
        }

        public string ExecuteAndCommit(IronPortShell sshClient)
        {
            var result = Execute(sshClient);
            var commitResult = string.Empty;

            if (null != result)
                commitResult = Commit(sshClient);

            CommandResult = string.Concat(result.Trim(), commitResult.Trim());

            return CommandResult;
        }

    }
}
