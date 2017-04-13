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
        Commit,
        AddSenderToBlackList,
        GrepLogFile,
    }

    public abstract class IronPortSSHCommand
    {
        public string CommandText { get; protected set; }
        public string CommandResult { get; protected set; }
        public virtual bool Succeeded { get { return true; } }

        // Dict containing format strings / literal strings of cli commands for iron port
        protected Dictionary<IronPortSupportedCommand, string> _supportedCommands = new Dictionary<IronPortSupportedCommand, string>
        {
            { IronPortSupportedCommand.GetVersion, "version" },
            { IronPortSupportedCommand.Commit, "commit \"{0}\" Y" },
            { IronPortSupportedCommand.AddSenderToBlackList, "listenerconfig edit \"{0}\" hostaccess edit sendergroup {1} new {2}" },
            { IronPortSupportedCommand.GrepLogFile, "grep \"{0}\" {1}" }
        };

        public abstract string Execute(IronPortShell sshClient);
        public abstract Task<string> ExecuteAsync(IronPortShell sshClient);
    }
}
