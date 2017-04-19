using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Siemplify.Integrations.IronPort;
using Siemplify.Integrations.IronPort.Exceptions;

namespace Siemplify.Integrations.IronPort.Exe
{
    class ProgramMain
    {
        static void Main(string[] args)
        {

            using (var controller = new IronPortController())
            {
                try
                {
                    //var addSenderOutput = controller.AddSenderToBlacklist("test5.com");

                    //if (null == addSenderOutput)
                    //    Console.WriteLine("Failed adding sender to blacklist, look at output window");
                    //else
                    //    Console.WriteLine(addSenderOutput);

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    var recipients1 = controller.GetAllRecipientsBySubject("This is a test subject");
                    stopwatch.Stop();

                    Console.WriteLine("Elapsed time for GetAllRecipientsBySubject = {0}s", stopwatch.ElapsedMilliseconds / 1000);
                    stopwatch.Restart();
                    var recipients = controller.GetAllRecipientsBySender("noy.sh22@gmail.com");
                    stopwatch.Stop();

                    Console.WriteLine("Elapsed time for GetAllRecipientsBySender = {0}s", stopwatch.ElapsedMilliseconds / 1000);

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

                        Console.WriteLine("=====================================");

                        foreach (var recipient in recipients1)
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
                    Console.WriteLine("Failed with: {0}", ex.Message);
                }
            }

#if DEBUG
            Console.WriteLine("Press anykey to continue...");
            Console.ReadLine();
#endif
        }
    }
}
