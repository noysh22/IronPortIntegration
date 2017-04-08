using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using Renci.SshNet;
using Renci.SshNet.Common;

namespace IronPortIntegration
{
    class IronPortShell : SshClient
    {
        private static TimeSpan DefaultCommandTimeout;

        private const string DEFAULT_SHELL = "ironportshell";
        private const uint COLUMNS = 80;
        private const uint ROWS = 24;
        private const uint WIDTH = 800;
        private const uint HEIGHT = 600;
        private const int BUFFER_SIZE = 4 * 1024;

        private const string SHELL_READY_INDICATOR = "relay.dworld.co.uk>";
        private const string UNIX_LINEBREAK = "\n";

        private ShellStream _shell;
        private object _syncObj;
        private bool _isShellReady;

        static IronPortShell()
        {
            DefaultCommandTimeout = new TimeSpan(0, 1, 0); // Timeout is 1 minute
        }

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
            Connect();
            _shell = CreateShellStream(terminalName, columns, rows, width, height, bufferSize);

            // Wait for the shell to be ready by waiting for the console indicator to appear on the stream
            var result = _shell.Expect(SHELL_READY_INDICATOR, DefaultCommandTimeout);
            _isShellReady = true;
        }

        public string RunShellCommand(string commandText, string resultFormat = null)
        {
            if (!_isShellReady)
                return null;

            lock (_syncObj)
            {
                _shell.Write(commandText + UNIX_LINEBREAK);

                var result = _shell.Expect(SHELL_READY_INDICATOR, DefaultCommandTimeout);

                Console.WriteLine(result);

                return result;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _shell.Close();
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
