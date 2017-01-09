using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.KB
{
    public class KBSearchResult
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public float userfeedback { get; set; }
    }
}