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
    public abstract class IronPortSSHCommitCommand : IronPortSSHCommand
    {
        private string CommitCommandText;
        protected string CommitMessage;

        private string CommitCommandFormat = "{0} \"{1}\" y";

        public IronPortSSHCommitCommand()
        {
            CommitCommandText = _supportedCommands[IronPortSupportedCommand.Commit];
        }

        private void Commit(IronPortShell sshClient)
        {
            var commitCmd = string.Format(CommitCommandFormat, CommitCommandText, CommitMessage);

            sshClient.RunShellCommand(commitCmd);

            //using (var sshCommand = sshClient.CreateCommand(commitCmd))
            //{
            //    Debug.WriteLine(string.Format("Executing command {0}...", commitCmd));
            //    var result = sshCommand.Execute();
            //    Debug.WriteLine(string.Format("Command {0} done", commitCmd));

            //    if (0 != sshCommand.ExitStatus)
            //    {
            //        throw new IronPortSshCommandException(sshCommand.ExitStatus, sshCommand.Error);
            //    }
            //}
        }

        public string ExecuteAndCommit(IronPortShell sshClient)
        {
            var result = Execute(sshClient);

            if (null != result)
                Commit(sshClient);

            return result;
        }

    }
}
