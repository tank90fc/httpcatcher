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
            Console.WriteLine(Encoding.Default);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Task t = new Task<>(DownloadPageAsync);

            MyCatch AA = new MyCatch();
            AA.DoTest();

            //DownloadPageAsync()
            //Task t = new Task(DownloadPageAsync);
            //t.Start();

            Console.WriteLine("Downloading page...");
            Console.ReadLine();
        }

        
    }
}
