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
    public class IronPortSSHController
    {
        // ===================== Constants ========================
        private const string IronPortDefaultHost = "CHANGEHOST";
        private const string IronPortDefaultUserName = "CAHNGEUSER";

        // ==================== Public members ====================
        public string Host { get; private set; }
        public string UserName { get; private set; }

        // ==================== Private members ====================
        private string Key { get; set; }
        private ConnectionInfo _connectionInfo;
        private IronPortSSHClient _sshClient;

        public IronPortSSHController(
            string pass,
            string username = IronPortDefaultUserName,
            string host = IronPortDefaultHost)
        {
            Host = host;
            UserName = username;
            Key = pass;
            _connectionInfo = new PasswordConnectionInfo(Host, UserName, Key);
            _sshClient = new IronPortSSHClient(_connectionInfo);
        }

        public string GetIronPortVersion()
        {
            try
            {
                _sshClient.Connect();
                Debug.WriteLine("SSH client connected to {0} using username {1}", Host, UserName);
                return _sshClient.GetIronPortVersion();
            }
            catch(System.Net.Sockets.SocketException ex)
            {
                Debug.WriteLine("Failed Connecting to IronPort via ssh - {0}", ex.Message);
                throw new IronPortSshConnentionException("Failed Connecting to IronPort via ssh", ex);
            }
            catch(IronPortNotConnectedException)
            {
                Debug.WriteLine("SSH client it not connected");
                throw;
            }
            catch (IronPortSshCommandException ex)
            {
                Debug.WriteLine("Failed getting iron-port version with status: {0} ({1})", ex.ExitStatus, ex.Message);
            }
            catch (IronPortException ex)
            {
                Debug.WriteLine("General Error - {0}", ex.Message);
                throw;
            }
            finally
            {
                _sshClient.Disconnect();
            }

            return null;
        }
    }
}
