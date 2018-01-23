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
using AngleSharp;
using AngleSharp.Parser.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;

namespace Catcher
{
    class CatchUrl
    {
        public string url = "";
        public int index = 0;
    }

    class CahchData
    {
        public int pageSize = 20;
        public int maxPageSize = 100;
        public string baseUrl = "";
        public string outPath = "";
        protected List<CatchUrl> urls;

        public CahchData(string _outPath, string _baseUrl, int _pageSize = 20, int _maxPageSize = 100)
        {
            pageSize = _pageSize;
            maxPageSize = _maxPageSize;
            baseUrl = _baseUrl;
            outPath = _outPath;
        }

        public List<CatchUrl> URLs
        {
            get {
                if(urls == null)
                {
                    Utility.CreateIfMissing(outPath);
                    List<CatchUrl> ret = new List<CatchUrl>();
                    outPath = Utility.GetWrokPath(outPath);
                    string[] files = Directory.GetFiles(outPath);
                    var outerNumFile = 0;
                    var fileNums = (from c in files
                                    let parseSuccess = int.TryParse(Path.GetFileNameWithoutExtension(c), out outerNumFile)
                                    let numFile = outerNumFile
                                    where parseSuccess == true
                                    orderby numFile
                                    select int.Parse(Path.GetFileNameWithoutExtension(c)));

                    var dictNums = fileNums.ToDictionary(p => p);
                    for (int i = 0; i < maxPageSize; i++)
                    {
                        if (!dictNums.ContainsKey(i))
                        {
                            string tempURL = string.Format(baseUrl, pageSize * i);
                            CatchUrl catchUrl = new CatchUrl();
                            catchUrl.index = i;
                            catchUrl.url = tempURL;
                            ret.Add(catchUrl);
                        }
                    }
                    urls = ret;
                }
                return urls;
            }
        }
    }

    class CatcherHander
    {
        public Collection<ICompressor> Compressors { get; private set; }
        public string SHOP_SEARCH_PATH = "shopsearch";
        public int PAGE_SIZE = 20;


        public CatcherHander()
        {
            Compressors = new Collection<ICompressor>();
            Compressors.Add(new GZipCompressor());
        }


        public void DoCatchShops()
        {
            CahchData cahchData = new CahchData(SHOP_SEARCH_PATH, "https://shopsearch.taobao.com/search?app=shopsearch&q=显卡&s={0}");
            //List<CatchUrl> urls = GetCommonURLs(SHOP_SEARCH_PATH, "https://shopsearch.taobao.com/search?app=shopsearch&q=显卡&s={0}");
            Task<string> ttt = Task<string>.Run(() => DownloadPageAsync(cahchData));
        }

        public int GetExistsPageIndex()
        {
            string[] files = Directory.GetFiles(SHOP_SEARCH_PATH);
            var sss = files.Select(c => int.Parse(Path.GetFileNameWithoutExtension(c)));
            if (sss.Count() == 0)
                return 0;
            else
                return sss.Max();
        }


        async Task<string> DownloadPageAsync(CahchData cahchData)
        {
            string proxyUri = string.Format("{0}:{1}", "127.0.0.1", 8888);
            WebProxy proxy = new WebProxy(proxyUri);

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                //Proxy = proxy,
                //UseProxy = true,
                UseCookies = true,
                CookieContainer = cookies
            };

            HttpClient client = new HttpClient(httpClientHandler);
            client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
            client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("accept-language", "zh-CN,zh;q=0.9,en;q=0.8");
            client.Timeout = new TimeSpan(0, 0, 0, 3, 0);
            try
            {
                foreach (var catchURL in cahchData.URLs)
                {
                    Uri url = new Uri(catchURL.url);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    //httpContent.Content = 
                    request.Version = new Version(2, 0);

                    HttpResponseMessage response = await client.SendAsync(request);

                    IEnumerable<string> contentEncodings;
                    if (response.Content.Headers.TryGetValues("content-encoding", out contentEncodings))
                    {
                        var encoding = contentEncodings.FirstOrDefault();
                        var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.InvariantCultureIgnoreCase));
                        response.Content = new DecompressedHttpContent(response.Content, compressor);
                    }

                    HttpContent content = response.Content;
                    byte[] responseBytes = await content.ReadAsByteArrayAsync();

                    string filePath = Path.Combine(cahchData.outPath, catchURL.index.ToString());
                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    fs.Write(responseBytes, 0, responseBytes.Length);
                    fs.Flush();
                    fs.Close();

                    Console.WriteLine("finish write page {0}", catchURL.index);

                    await Task.Delay(3000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("all done");
            return "all done";
        }
    }
}

