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

namespace Catcher
{
    public class ShopDBBase
    {
        public Dictionary<long, ShopData> shops = new Dictionary<long, ShopData>();
    }

    public class ItemDBBase
    {
        public Dictionary<long, ItemData> items;
    }

    public class ShopData
    {
        public long uid;
        public string shopUrl;
        public string title;
    }

    public class ItemData
    {
        public long itemID;
        public string itemUrl;
        public string itemName;
    }


    class DataProcess
    {
        public string DATA_CENTER_PATH;
        public ShopDBBase shopDBBase = null;

        public string GetJsonFilePath<T>()
        {
            Utility.CreateIfMissing("data_center");
            if(typeof(T) == typeof(ShopDBBase))
                return Utility.GetWrokPath("data_center/shops.json");
            else if (typeof(T) == typeof(ItemDBBase))
                return Utility.GetWrokPath("data_center/items.json");

            return null;
        }

        public DataProcess()
        {

            //   ShopDBBase jp = (ShopDBBase)JsonConvert.DeserializeObject(result);//result为上面的Json数据  
        }

        public void Serializer<T>(T dbBase)
        {
            JsonSerializer serializer = new JsonSerializer();
            //serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter(GetJsonFilePath<T>()))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, dbBase);
            }
        }

        public void Deserializer<T>(out T dbBase) where T : new()
        {
            string path = GetJsonFilePath<T>();
            if (!File.Exists(path))
            {
                dbBase = new T();
                return;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                string json = sr.ReadToEnd();
                dbBase = JsonConvert.DeserializeObject<T>(json);
            }
        }

        public void ParseHtml(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string html = sr.ReadToEnd();
                var parser = new HtmlParser();
                //Just get the DOM representation
                var document = parser.Parse(html);
                var blueListItemsLinq = document.DocumentElement.QuerySelectorAll("*").Where(m => m.LocalName == "script");
                bool findPageConfig = false;
                foreach (var item in blueListItemsLinq)
                {
                    if (item.TextContent.Contains("g_page_config"))
                    {
                        findPageConfig = true;
                        string[] jsonSrings = item.TextContent.Split('\n');
                        var tempSelect = jsonSrings.Where(xxx => xxx.Contains("g_page_config"));
                        string jsonSring = Utility.GetVariableString(tempSelect.FirstOrDefault());
                        JObject jo = (JObject)JsonConvert.DeserializeObject(jsonSring);
                        try
                        {
                            string itemlist = jo["mods"]["shoplist"]["data"]["shopItems"].ToString();
                            itemlist = Utility.DeUnicode(itemlist);

                            JArray jArray = (JArray)JArray.Parse(itemlist);
                            foreach (var json in jArray)
                            {
                                ShopData rowsResult = Newtonsoft.Json.JsonConvert.DeserializeObject<ShopData>(json.ToString());
                                if (!shopDBBase.shops.ContainsKey(rowsResult.uid))
                                    shopDBBase.shops[rowsResult.uid] = rowsResult;
                            }
                        }
                        catch ( Exception ex)
                        {
                            Console.WriteLine("g_page_config error, {0}",filePath);
                            //Console.Write(ex);
                        }
                    }
                }

                if (!findPageConfig)
                    Console.WriteLine("g_page_config not find, {0}",filePath);
            }
        }

        public void ParseShops()
        {
            string shopsearchPath = Utility.GetWrokPath("shopsearch");
            Utility.CreateIfMissing(shopsearchPath);
            var outerNumFile = 0;
            var tempList = Directory.GetFiles(shopsearchPath).ToList();
            var files = (from c in tempList
                              let parseSuccess = int.TryParse(Path.GetFileNameWithoutExtension(c), out outerNumFile)
                              let numFile = outerNumFile
                              where parseSuccess == true
                              orderby numFile
                              select c);
            
            if (files.Count() > 0)
            {
                Deserializer(out shopDBBase);

                files.ToList().ForEach(r => ParseHtml(r));

                Serializer(shopDBBase);
            }
        }
    }
}
