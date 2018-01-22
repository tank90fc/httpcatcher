﻿using System;
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
            //Encoding.Default = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine(Encoding.Default);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //Task t = new Task<>(DownloadPageAsync);

            CatcherHander catcherHander = new CatcherHander();
            catcherHander.DoTest();

            //DownloadPageAsync()
            //Task t = new Task(DownloadPageAsync);
            //t.Start();

            Console.WriteLine("Downloading page...");
            Console.ReadLine();
        }

        
    }
}
