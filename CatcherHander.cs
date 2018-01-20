using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using AngleSharp.Parser.Html;

namespace Catcher
{
    class CatcherHander
    {
        public Collection<ICompressor> Compressors { get; private set; }

        public CatcherHander()
        {
            Compressors = new Collection<ICompressor>();
            Compressors.Add(new GZipCompressor());
        }

        public void DoTest()
        {
            Task<string> ttt = Task<string>.Run(DownloadPageAsync);
            System.Runtime.CompilerServices.TaskAwaiter<string> awaiter = ttt.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                if (!ttt.IsCanceled)
                {
                    //string result = awaiter.GetResult();
                    //Console.WriteLine(result);
                }

            });
        }

        async Task<string> DownloadPageAsync()
        {
            string proxyUri = string.Format("{0}:{1}", "127.0.0.1", 8888);
            WebProxy proxy = new WebProxy(proxyUri);
            // {
            //UseDefaultCredentials = false,
            //Credentials = proxyCreds,
            // };
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = proxy,
                UseProxy = true,
                UseCookies = true,
                CookieContainer = cookies
            };


            //HttpRequestMessage myHttpRequestMessage = new HttpRequestMessage();
            //myHttpRequestMessage.Version = new Version(2, 0);

            //            user - agent: Mozilla / 5.0(Windows NT 10.0; WOW64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 63.0.3239.132 Safari / 537.36
            //accept - encoding: gzip, deflate, br
            //accept - language: zh - CN,zh; q = 0.9,en; q = 0.8

            // ... Target page.
            Uri url = new Uri("https://s.taobao.com/search?initiative_id=staobaoz_20180120&q=内存");
            string data = "";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            //httpContent.Content = 
            request.Version = new Version(2, 0);
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));

            
            //if (request.Content.Headers.ContentEncoding != null && request.Content.Headers.ContentEncoding.Any())
            //{
            //    //// request content is compressed, decompress it.
            //    var encoding = request.Content.Headers.ContentEncoding.First();
            //    var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
            //    if (compressor != null)
            //    {
            //        request.Content = new CompressedHttpContent(request.Content, compressor);
            //    }
            //}

            //var compressor = (from encoding in request.Headers.AcceptEncoding
            //                  let quality = encoding.Quality ?? 1.0
            //                  where quality > 0
            //                  join c in Compressors on encoding.Value.ToLowerInvariant() equals c.EncodingType.ToLowerInvariant()
            //                  orderby quality descending
            //                  select c).FirstOrDefault();
            //CompressionHttpContent
            // ... Use HttpClient.
            HttpClient client = new HttpClient(httpClientHandler);
            try
            {
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
                //client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8");
                //var vv2 = httpContent.Headers.AcceptEncoding.Add("gzip")

                HttpResponseMessage response = await client.SendAsync(request);

                IEnumerable<string> contentEncodings;
                if(response.Content.Headers.TryGetValues("content-encoding", out contentEncodings))
                {
                    var encoding = contentEncodings.FirstOrDefault();
                    var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
                    response.Content = new DecompressedHttpContent(response.Content, compressor);
                }

                HttpContent content = response.Content;
                //foreach (var head in content.Headers)
                //{
                //    Console.WriteLine(head.Key + " " + head.Value);
                //    foreach(var vv in head.Value)
                //    {
                //        Console.WriteLine(vv);

                //    }
                //}
                //responseHeaders.GetValues()
                byte[] responseBytes = await content.ReadAsByteArrayAsync();
                //string header = .First();
                //HttpResponseHeaders responseHeaders = response;
                //foreach (var v in response.Headers.GetValues("Content-Encoding"))
                //{
                //    Console.WriteLine(v);
                //    //Console.WriteLine(kv.Key + " " + kv.Value);
                //    //foreach (var v2 in kv.Value)
                //    //{
                //    //    Console.WriteLine(v2);
                //    //}
                //}
                //string result = await content.ReadAsStringAsync();
                responseBytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("GBK"), responseBytes);
                //string result = Encoding.Unicode.GetString(responseBytes, 0, responseBytes.Length - 1);
                string result = Encoding.GetEncoding("GBK").GetString(responseBytes);

                var parser = new HtmlParser();
                //Just get the DOM representation
                var document = parser.Parse(result);

                //Serialize it back to the console
                //Console.WriteLine(document.DocumentElement.OuterHtml);
                var blueListItemsLinq = document.All.Where(m => m.LocalName == "html");
                foreach (var item in blueListItemsLinq)

                    //Console.WriteLine(item.OuterHtml);
                //string result = Encoding.Unicode.GetString(responseBytes);
                //Uri uri = new Uri("https://s.taobao.com/");
                //CookieCollection responseCookies = cookies.GetCookies(uri);
                //foreach (Cookie cookie in responseCookies)
                //    Console.WriteLine(cookie.Name + ": " + cookie.Value);

                data = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return data;
        }
    }
}
