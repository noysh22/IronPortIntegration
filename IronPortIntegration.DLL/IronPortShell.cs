using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Renci.SshNet;
using Renci.SshNet.Common;

using Siemplify.Integrations.IronPort.Exceptions;
using Siemplify.Integrations.IronPort.SshCommands;

namespace Siemplify.Integrations.IronPort
{
    public class IronPortShell : SshClient
    {

        private const string DEFAULT_SHELL = "ironportshell";
        private const uint COLUMNS = 80;
        private const uint ROWS = 24;
        private const uint WIDTH = 800;
        private const uint HEIGHT = 600;
        private const int BUFFER_SIZE = 4 * 1024;

        public const string SHELL_READY_INDICATOR = "relay.dworld.co.uk>";

        private static readonly Tuple<string, string> ReplacableShellString = 
            new Tuple<string, string>("\r\r\n", string.Empty);

        private static readonly Tuple<string, string> ReplacableBreakLine =
            new Tuple<string, string>("\r\n\r\n", "\n");

        private const string UNIX_LINEBREAK = "\n";

        private static readonly TimeSpan CommandTimeout = new TimeSpan(0, 1, 0);
        private ShellStream _shell;
        private object _syncObj;
        private bool _isShellReady;

        public IronPortShell(string host, string username, string pass)
            : base(host, username, pass)
        {
            ErrorOccurred += IronPortShell_ErrorOccurred;
            _syncObj = new object();
            _isShellReady = false;
        }

        public IronPortShell(ConnectionInfo conn)
            : base(conn)
        {
            ErrorOccurred += IronPortShell_ErrorOccurred;
            _syncObj = new object();
            _isShellReady = false;
 
        }

        public void StartShell(string terminalName = DEFAULT_SHELL, uint columns = COLUMNS, uint rows = ROWS, uint width = WIDTH, uint height = HEIGHT, int bufferSize = BUFFER_SIZE)
        {
            if(!IsConnected)
                Connect();

            _shell = CreateShellStream(terminalName, columns, rows, width, height, bufferSize);

            // Wait for the shell to be ready by waiting for the console indicator to appear on the stream
            _shell.Expect(SHELL_READY_INDICATOR, CommandTimeout);
            _isShellReady = true;
        }

        public void RawShellWrite(string input)
        {
            lock (_syncObj)
            {
                _shell.Write(input + UNIX_LINEBREAK);
            }
        }

        public string WaitForShellOutput(TimeSpan WaitTimeout, string shellReadyIndicator = SHELL_READY_INDICATOR)
        {
            return _shell.Expect(shellReadyIndicator, WaitTimeout);
        }

        public string RunShellCommand(string commandText, string shellReadyIndicator = SHELL_READY_INDICATOR)
        {
            if (!_isShellReady)
                return null;

            lock (_syncObj)
            {
                //_shell.Write(commandText + UNIX_LINEBREAK);

                RawShellWrite(commandText);

                var result = WaitForShellOutput(CommandTimeout, shellReadyIndicator);
                    //_shell.Expect(shellReadyIndicator, CommandTimeout);

                // Replace the string \r\r\n which returns from the shell with empty string
                result = result.Replace(ReplacableShellString.Item1, ReplacableShellString.Item2);
                // Replace the breakline which returns from the shell with single \n
                result = result.Replace(ReplacableBreakLine.Item1, ReplacableBreakLine.Item2);

                Debug.WriteLine(string.Format("Shell command {0} returned with {1}",
                    commandText,
                    result));

                return result;
            }
        }

        public string GetIronPortVersion()
        {
            if (!IsConnected)
                throw new IronPortNotConnectedException();

            var getVersion = new IronPortGetVersionCommand();
            return getVersion.Execute(this);
        }

        public bool AddDomainToBlacklist(string domain)
        {
            if (!IsConnected)
                throw new IronPortNotConnectedException();

            var addToBlacklist = new IronPortAddDomainToBlacklistCommand(domain);

            StartShell();
            var result = addToBlacklist.ExecuteAndCommit(this);
            CloseShell();

            var isSucceeded = addToBlacklist.IsSucceeded();

            if (!isSucceeded)
            {
                Debug.WriteLine(string.Format("Failed adding {0} to blacklist, command result is: {1}",
                    domain, result));
            }

            return isSucceeded;
        }

        public bool AddSendersToBlackList(List<string> senders, string filterName)
        {
            if (!IsConnected)
            {
                throw new IronPortNotConnectedException();
            }

            var addToBlacklist = new IronPortAddSenderToBlacklistCommand(senders, filterName);

            StartShell();
            var result = addToBlacklist.ExecuteAndCommit(this);
            CloseShell();

            var isSucceeded = addToBlacklist.IsSucceeded(this);

            if (!isSucceeded)
            {
                Debug.WriteLine("Failed adding {0} to blacklist, command result is: {1}",
                    string.Join("|", senders), result);
            }

            return isSucceeded;
        }

        public void CloseShell()
        {
            _shell.Close();
            _isShellReady = false;
            
        }

        protected override void Dispose(bool disposing)
        {
            if (null != _shell)
            {
                CloseShell();
                _shell.Dispose();
            }
            
            Disconnect();
            base.Dispose(disposing);
        }

        private void IronPortShell_ErrorOccurred(object sender, ExceptionEventArgs e)
        {
            Debug.WriteLine(
                string.Format("Error occured in ssh client, source: {0} hresult: {1} message: {2}"),
                e.Exception.Source,
                e.Exception.HResult,
                e.Exception.Message);

            Console.WriteLine("Error occured in ssh client, source: {0} hresult: {1} message: {2}",
                e.Exception.Source,
                e.Exception.HResult,
                e.Exception.Message);
        }
    }
}
