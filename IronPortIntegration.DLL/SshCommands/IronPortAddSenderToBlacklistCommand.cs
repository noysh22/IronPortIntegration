﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort.SshCommands
{
    public class IronPortAddSenderToBlacklistCommand : IronPortCommitCommand
    {
        private static readonly string CMD_INPUT_READY_INDICATOR = " Enter \'.\' on its own line to end.";
        private static readonly string BLOCK_FILTER_FORMAT = "{0}: if(mail-from == \"(?i){1}$\")";
        private static readonly string BLOCK_FILTER_ACTION = " { drop(); }";
        private const string JOIN_SEPARATOR = "|";
        private const string STOP_FILTER_TYPING_STRING = ".";
        private const string FILTER_LIST_COMMAND = "filters list";
        private const string FILTER_ADDED_MESSAGE = "1 filters added.";

        private readonly List<string> _senders;
        private readonly string _blockFilterName;

        public IronPortAddSenderToBlacklistCommand(List<string> senders, string filterName)
        {
            if (senders.Count == 0)
            {
                throw new IronPortException("Invalid senders list, senders count must be bigger than 0");
            }
            _senders = senders;
            _blockFilterName = filterName;

            CommitMessage = string.Format("Adding senders {0} to filter name {1}",
                string.Join(JOIN_SEPARATOR, _senders), _blockFilterName);
            CommitCommandText = string.Format(_supportedCommands[IronPortSupportedCommand.Commit], CommitMessage);
            CommandText = _supportedCommands[IronPortSupportedCommand.AddSenderToBlackList];
            CommandResult = null;
        }

        private bool IsFilterExists(IronPortShell sshClient)
        {
            string cmdResult = sshClient.RunCommand(FILTER_LIST_COMMAND).Result;

            return cmdResult.Contains(_blockFilterName);
        }

        public override string Execute(IronPortShell sshClient)
        {
            if (IsFilterExists(sshClient))
            {
                throw new IronPortBlockFilterAlreadyExistsException(string.Format("Could not add senders to filter {0}, filter with that name already exists",
                    _blockFilterName));
            }

            string filterText = string.Format(BLOCK_FILTER_FORMAT, _blockFilterName, string.Join(JOIN_SEPARATOR, _senders));
            filterText += BLOCK_FILTER_ACTION;

            sshClient.RunShellCommand(CommandText, CMD_INPUT_READY_INDICATOR);

            sshClient.RawShellWrite(filterText);
            sshClient.RawShellWrite(STOP_FILTER_TYPING_STRING);

            return sshClient.WaitForShellOutput(TimeSpan.FromSeconds(1));
        }

        public bool IsSucceeded(IronPortShell sshClient)
        {
            return CommandResult.Contains(FILTER_ADDED_MESSAGE);
        }

        public override Task<string> ExecuteAsync(IronPortShell sshClient)
        {
            throw new NotImplementedException();
        }
    }
}
