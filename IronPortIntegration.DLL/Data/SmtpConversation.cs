using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siemplify.Integrations.IronPort
{
    public class SmtpConversation
    {
        public int MID { get; private set; }
        public string SmtpConversationStr { get; private set; }

        public SmtpConversation(int mid, string smtpConversation)
        {
            MID = mid;
            SmtpConversationStr = smtpConversation;
        }
    }
}
