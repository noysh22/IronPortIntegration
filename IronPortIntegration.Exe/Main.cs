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
                //var addSenderOutput = controller.AddSenderToBlacklist("test5.com");

                //if (null == addSenderOutput)
                //    Console.WriteLine("Failed adding sender to blacklist, look at output window");
                //else
                //    Console.WriteLine(addSenderOutput);

                var recipients = controller.GetAllRecipientsBySubject("This is a test subject");
                //var recipients = controller.GetAllRecipientsBySender("noy.sh22@gmail.com");

                if (null == recipients)
                {
                    Console.WriteLine("Nothing found");
                }
                else
                {
                    foreach (var recipient in recipients)
                    {
                        Console.WriteLine(recipient);
                    }
                }
            }
            catch (IronPortException ex)
            {
                Console.WriteLine("Failed with: {0}", ex.Message + 
                    Environment.NewLine + ex.InnerException.Message);
            }
            catch (Exception ex)
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
