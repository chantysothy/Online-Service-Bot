using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace OCSBot.Utility
{
    public class HttpHelper
    {
        WebRequest http = null;
        private string DoHttpRequest(string body)
        {
            if (!string.IsNullOrEmpty(body))
            {
                using (var req = http.GetRequestStream())
                {


                    using (var sw = new BinaryWriter(req))
                    {
                        sw.Write(body);
                        sw.Flush();
                    }
                }
            }
            using (var respObje = http.GetResponse())
            {
                using (var sr = new StreamReader(respObje.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();

                    return result;
                }
            }
        }
        public string POST(string body, Dictionary<string,string> headers)
        {
            http.Method = "POST";
            foreach(var header in headers)
            {
                http.Headers.Add(header.Key, header.Value);
            }
            return DoHttpRequest(body);
        }
        public string GET(Dictionary<string, string> headers)
        {
            http.Method = "GET";
            ((HttpWebRequest)http).Accept = "application/json";
            foreach (var header in headers)
            {
                http.Headers.Add(header.Key, header.Value);
            }
            return DoHttpRequest("");
        }
        public HttpHelper(string contentType, string url)
        {
            http = HttpWebRequest.Create(url) as WebRequest;
            http.ContentType = contentType;
        }
    }
}