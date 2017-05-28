using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Renci.SshNet;

using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort
{
    public class IronPortController : IDisposable
    {
        // ===================== Constants ========================
        private static string IronPortDefaultHost = ConfigurationManager.AppSettings["DefaultHost"];
        private static string IronPortDefaultUserName = ConfigurationManager.AppSettings["DefaultUserName"];
        private static string IronPortDefaultPass = ConfigurationManager.AppSettings["CurrentPass"];

        // ==================== Public members ====================
        public string Host { get; private set; }
        public string UserName { get; private set; }

        // ==================== Private members ====================
        private string Key { get; set; }
        private ConnectionInfo _connectionInfo;
        private IronPortShell _sshClient;
        private IronPortQueryResolver _queryResolver;

        public IronPortController(
            string host = null,
            string username = null,
            string pass = null)
        {
            Host = null != host ? host : IronPortDefaultHost;
            UserName = null != username ? username : IronPortDefaultUserName;
            Key = null != pass ? pass : IronPortDefaultPass;
            _connectionInfo = new PasswordConnectionInfo(Host, UserName, Key);
            _sshClient = new IronPortShell(_connectionInfo);

            _queryResolver = new IronPortQueryResolver(_sshClient);
        }

        private void EnsureConnection()
        {
            if (!_sshClient.IsConnected)
            {
                _sshClient.Connect();
                Debug.WriteLine("SSH client connected to {0} using username {1}", Host, UserName);
            }
        }

        public string GetIronPortVersion()
        {
            try
            {
                EnsureConnection();   
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

        /// <summary>
        /// Add a domain to the blacklist of the ironport apllience
        /// </summary>
        /// <param name="domain">The domain parameter to add to blacklist</param>
        /// <remarks>
        /// The domain parameter must match the format of cisco ironport domain format
        /// The following formats are allowed: 
        //      IPv6 addresses such as 2001:420:80:1::5
        //      IPv6 subnets such as 2001:db8::/32
        //      IPv4 addresses such as 10.1.1.0
        //      IPv4 subnets such as 10.1.1.0/24 or 10.2.3.1
        //      IPv4 and IPv6 address ranges such as 10.1.1.10-20, 10.1.1-5 or 2001::2-2001::10.
        //      Hostnames such as example.com.
        //      Partial hostnames such as .example.com.
        /// </remarks>
        /// <returns></returns>
        public bool AddDomainToBlacklist(string domain)
        {
            try
            {
                EnsureConnection();
                return _sshClient.AddDomainToBlacklist(domain);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Debug.WriteLine("Failed Connecting to IronPort via ssh - {0}", ex.Message);
                throw new IronPortSshConnentionException("Failed Connecting to IronPort via ssh", ex);
            }
            catch (IronPortNotConnectedException)
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

            return false;
        }

        /// <summary>
        /// Add a list of senders to filter list in iron port which blocks the emails
        /// </summary>
        /// <param name="senders">List of senders to block</param>
        /// <param name="filterName">The name of the filter which is created</param>
        /// <returns></returns>
        public bool AddSendersToBlacklist(List<string> senders, string filterName)
        {
            try
            {
                EnsureConnection();
                return _sshClient.AddSendersToBlackList(senders, filterName);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Debug.WriteLine("Failed Connecting to IronPort via ssh - {0}", ex.Message);
                throw new IronPortSshConnentionException("Failed Connecting to IronPort via ssh", ex);
            }
            catch (IronPortNotConnectedException)
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

            return false;
        }

        /// <summary>
        /// Get all the recipients which recieved a given mail subject
        /// </summary>
        /// <param name="mailSubject">The mail subject to query</param>
        /// <returns>List of all recipients found</returns>
        public List<string> GetAllRecipientsBySubject(string mailSubject)
        {
            EnsureConnection();
            return _queryResolver.GetAllRecipientsOfSubject(mailSubject);
        }

        /// <summary>
        /// Get all the recipients which recieved mails from a given domain
        /// </summary>
        /// <param name="senderMail">The domain address to query</param>
        /// <returns>List of all recipients found</returns>
        public List<string> GetAllRecipientsBySender(string senderMail)
        {
            EnsureConnection();
            return _queryResolver.GetAllRecipientsBySender(senderMail);
        }

        public void Dispose()
        {
            _sshClient.Dispose();
        }
    }
}
