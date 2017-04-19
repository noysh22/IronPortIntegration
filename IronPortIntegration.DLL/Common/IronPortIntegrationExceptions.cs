using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemplify.Integrations.IronPort.Exceptions
{
    public class IronPortException : Exception
    {
        public IronPortException() : base() { }
        public IronPortException(string message) : base(message) { }
        public IronPortException(string message, Exception innerEx) : base(message, innerEx) { }
    }

    public class IronPortNotConnectedException : IronPortException { }

    public class IronPortSshCommandException : IronPortException
    {
        public int ExitStatus { get; private set; }

        public IronPortSshCommandException(int exitStatus, string message) :
            base(message)
        {
            ExitStatus = exitStatus;
        }
    }

    public class IronPortSshConnentionException : IronPortException
    {
        public IronPortSshConnentionException(string message, Exception innerEx) : 
            base(message, innerEx) { }
    }

    public class IronPortParsingException : IronPortException
    {
        public IronPortParsingException(string message) :
            base(message)
        { }
    }
}
