using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Net.Http.Headers;

namespace Catcher
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(Encoding.Default);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            DoCatchProcess();

            Console.WriteLine("Downloading page...");
            Console.ReadLine();
            return;
        }
        static void DoCatchProcess()
        {
            CatcherHander catcherHander = new CatcherHander();
            catcherHander.DoCatchShops();

        }
        static void DoDataProcess()
        {
            DataProcess dataProcess = new DataProcess();
            dataProcess.ParseShops();
        }


    }
}
