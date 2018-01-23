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
using System.Reflection;
using System.Collections;

namespace Catcher
{
    //public class CatchUrlEnum : IEnumerator
    //{
    //    public int pageSize = 20;
    //    public string _baseUrl;
    //    public int _index = 0;
    //    public CatchUrlEnum(string inBaseUrl, int inIndex = 0, int inPageSize = 20)
    //    {
    //        _index = inIndex;
    //        pageSize = inPageSize;
    //        _baseUrl = inBaseUrl;
    //    }

    //    public bool MoveNext()
    //    {
    //        _index++;
    //        return (_index < 100);
    //    }

    //    public void Reset()
    //    {
    //        _index = -1;
    //    }

    //    public string Current
    //    {
    //        get
    //        {
    //            try
    //            {
    //                return string.Format(_baseUrl, _index);
    //            }
    //            catch (IndexOutOfRangeException)
    //            {
    //                throw new InvalidOperationException();
    //            }
    //        }
    //    }
    //}

    public static class Utility
    {
        public static string utf2hz(string str)
        {
            Regex regex = new Regex(@"\\u(\w{4})");
            string result = "";
            foreach (Match m in regex.Matches(str.ToLower().Trim()))
                result += ((char)int.Parse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)).ToString();
            return result;
        }

        static public string DeUnicode(string s)
        {
            string ret = null;
            Regex reUnicode = new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
            ret = reUnicode.Replace(s, m =>
            {
                short c;
                if (short.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out c))
                {
                    return "" + (char)c;
                }
                return m.Value;
            });

            ret = ret.Replace("\\/", "/");
            return ret;
        }

        static public void CreateIfMissing(string path)
        {
            path = GetWrokPath(path);
            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);
        }

        static public string GetWrokPath(string fileName)
        {
            string str = Assembly.GetExecutingAssembly().Location;
            str = Path.GetDirectoryName(str);
            str = Path.Combine(str, fileName);
            return str;
        }

        static public string GetVariableString(string inString)
        {
            string ret = "";
            int startIndex = inString.IndexOf('=') + 1;
            int endIndex = inString.LastIndexOf(';');
            ret = inString.Substring(startIndex, endIndex - startIndex);

            return ret;

        }

        static public void BackCode()
        {

            //var blueListItemsLinq = document.DocumentElement.QuerySelectorAll("*").Where(m => m.Id == "J_ShopAsynSearchURL");

            //HttpRequestMessage myHttpRequestMessage = new HttpRequestMessage();
            //myHttpRequestMessage.Version = new Version(2, 0);

            //            user - agent: Mozilla / 5.0(Windows NT 10.0; WOW64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 63.0.3239.132 Safari / 537.36
            //accept - encoding: gzip, deflate, br
            //accept - language: zh - CN,zh; q = 0.9,en; q = 0.8

            // ... Target page.

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

            byte[] responseBytes = null;
            //FileStream fs = new FileStream("F:/test.txt", FileMode.OpenOrCreate);
            //StreamWriter sw = new StreamWriter(fs);
            ////string result = Encoding.GetEncoding("GBK").GetString(responseBytes);
            //responseBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, responseBytes);
            //fs.Write(responseBytes, 0, responseBytes.Length);

            //string result = await content.ReadAsStringAsync();
            //responseBytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("GBK"), responseBytes);
            //string result = Encoding.Unicode.GetString(responseBytes, 0, responseBytes.Length - 1);

            string result = Encoding.UTF8.GetString(responseBytes);
            //Console.WriteLine(result);
            //var config = Configuration.Default.WithJavaScript();

            var parser = new HtmlParser();
            //Just get the DOM representation
            var document = parser.Parse(result);
            //Console.WriteLine(document.DocumentElement);
            //Serialize it back to the console
            //Console.WriteLine(document.DocumentElement.OuterHtml);
            var blueListItemsLinq = document.DocumentElement.QuerySelectorAll("*").Where(m => m.LocalName == "script");
            FileStream fs = new FileStream("F:/test.txt", FileMode.Create);
            //StreamWriter sw = new StreamWriter(fs);
            foreach (var item in blueListItemsLinq)
            {
                if (item.TextContent.Contains("g_page_config"))
                {
                    string[] jsonSrings = item.TextContent.Split('\n');
                    string jsonSring = jsonSrings[2];

                    jsonSring = GetVariableString(jsonSring);
                    JObject jo = (JObject)JsonConvert.DeserializeObject(jsonSring);
                    string itemlist = jo["mods"]["shoplist"]["data"]["shopItems"].ToString();
                    //p4p = System.Uri.UnescapeDataString(p4p);
                    itemlist = DeUnicode(itemlist);
                    byte[] writeBytes = System.Text.Encoding.UTF8.GetBytes(itemlist);
                    //string value = @"\u65b0\u7586";
                    //value = DeUnicode(p4p);
                    //value = Encoding.Unicode.GetString(value);
                    //writeBytes = System.Text.Encoding.UTF8.GetBytes(value);
                    fs.Write(writeBytes, 0, writeBytes.Length);
                    fs.Flush();
                    fs.Close();
                    Console.WriteLine("done");
                }
            }
            //Console.WriteLine(item.OuterHtml);
            //string result = Encoding.Unicode.GetString(responseBytes);
            //Uri uri = new Uri("https://s.taobao.com/");
            //CookieCollection responseCookies = cookies.GetCookies(uri);
            //foreach (Cookie cookie in responseCookies)
            //    Console.WriteLine(cookie.Name + ": " + cookie.Value);

        }
    }
}
