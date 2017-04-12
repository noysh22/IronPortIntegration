using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IronPortIntegration;
using IronPortIntegration.Exceptions;

namespace IronPortIntegration.Exe
{
    class ProgramMain
    {
        static void Main(string[] args)
        {
            var controller = new IronPortController();

            try
            {
                //var versionOutput = controller.GetIronPortVersion();

                var addSenderOutput = controller.AddSenderToBlacklist("test5.com");

                if (null == addSenderOutput)
                    Console.WriteLine("Failed adding sender to blacklist, look at output window");
                else
                    Console.WriteLine(addSenderOutput);
            }
            catch (IronPortException ex)
            {
                Console.WriteLine("Failed with: {0}", ex.Message + 
                    Environment.NewLine + ex.InnerException.Message);
            }

#if DEBUG
            Console.WriteLine("Press anykey to continue...");
            Console.ReadLine();
#endif
        }
    }
}
