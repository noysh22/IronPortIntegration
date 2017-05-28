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
    public class IronPortGetVersionCommand : IronPortSshCommand
    {
        public IronPortGetVersionCommand()
        {
            CommandText = _supportedCommands[IronPortSupportedCommand.GetVersion];
        }

        public override string Execute(IronPortShell sshClient)
        {
            using (var sshCommand = sshClient.CreateCommand(CommandText))
            {
                Debug.WriteLine("Executing command {0}...", CommandText);
                var result = sshCommand.Execute();
                Debug.WriteLine("Command {0} done", CommandText);

                if (0 != sshCommand.ExitStatus)
                {
                    throw new IronPortSshCommandException(sshCommand.ExitStatus, sshCommand.Error);
                }

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
