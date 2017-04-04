using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

namespace IronPortIntegration
{
    /* In order to support more ssh commands add values to this enum
     * and add another key-value suitable to the command in the dict below
     * This is in order to prevent running unwanted contiguration command on IronPort
     */ 
    public enum IronPortSupportedCommand
    {
        GetVersion = 0x1,
    }

    abstract class IronPortSSHCommand
    {
        public string CommandText { get; protected set; }

        protected Dictionary<IronPortSupportedCommand, string> _supportedCommands = new Dictionary<IronPortSupportedCommand, string>
        {
            { IronPortSupportedCommand.GetVersion, "version" },
        };

        public abstract string Execute(SshClient sshClient);

        public abstract Task<string> ExecuteAsync(SshClient sshClient);
    }
}
