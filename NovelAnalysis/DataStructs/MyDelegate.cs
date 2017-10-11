using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovelAnalysis
{
    public class MyDelegate
    {
        public delegate void sendStringDelegate(string str);
        public delegate void sendStringTextboxDelegate(string str);
        public delegate void sendVoidDelegate();
        public delegate void sendIntDelegate(int n);
        public delegate void sendStatusDelegate(Status s);
        public delegate bool getBoolDelegate();
        public delegate void getStatusDelegate(Status s);
    }
}
