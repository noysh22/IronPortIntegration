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
            var controller = new IronPortSSHController();

            try
            {
                var versionOutput = controller.GetIronPortVersion();

                if (null == versionOutput)
                    Console.WriteLine("Failed getting version, look at output window");
                else
                    Console.WriteLine(versionOutput);
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
