using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WzorzecBot.Helpers
{
    public class ClassHelper
    {
        public class Sender
        {
            public string id { get; set; }
        }

        public class Recipient
        {
            public string id { get; set; }
        }

        public class Postback
        {
            public string payload { get; set; }
            public string title { get; set; }
        }

        public class RootObject
        {
            public Sender sender { get; set; }
            public Recipient recipient { get; set; }
            public long timestamp { get; set; }
            public Postback postback { get; set; }
        }
    }
}