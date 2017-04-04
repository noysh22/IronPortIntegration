using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;

using IronPortIntegration.Exceptions;

namespace IronPortIntegration
{
    class IronPortSSHClient : SshClient
    {
        public IronPortSSHClient(string host, string username, string pass) :
            base (host, username, pass)
        { }

        public IronPortSSHClient(ConnectionInfo connInfo) : base (connInfo)
        { }

        public string GetIronPortVersion()
        {
            if (!IsConnected)
                throw new IronPortNotConnectedException(); 

            var getVersion = new IronPortGetVersionCommand();
            return getVersion.Execute(this);
        }
    }
}
