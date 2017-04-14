﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Configuration;
using Renci.SshNet;

using IronPortIntegration.Exceptions;

namespace IronPortIntegration
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

        public string AddSenderToBlacklist(string sender)
        {
            try
            {
                EnsureConnection();
                Debug.WriteLine("SSH client connected to {0} using username {1}", Host, UserName);
                return _sshClient.AddSenderToBlacklist(sender);
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

            return null;
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
        /// Get all the recipients which recieved mails from a given sender
        /// </summary>
        /// <param name="senderMail">The sender address to query</param>
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