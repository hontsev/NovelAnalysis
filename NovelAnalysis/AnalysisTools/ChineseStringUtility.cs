using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NovelAnalysis
{
    public static class ChineseStringUtility
    {
        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        /// <summary>
        /// 去掉异体字
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string RemoveVariant(string source)
        {
            string res = source;

            try
            {
                string[] pairs = File.ReadAllText(Directory.GetCurrentDirectory() + @"\v2t.txt").Split('|');
                Dictionary<string, string> changepairs = new Dictionary<string, string>();
                foreach (var p in pairs)
                {
                    string[] pair = p.Split(',');
                    if (!changepairs.ContainsKey(pair[1]))
                    {
                        changepairs[pair[1]] = pair[0];
                    }
                }
                string tmp = "";
                foreach (var c in res)
                {
                    if (changepairs.ContainsKey(c.ToString())) tmp += changepairs[c.ToString()];
                    else tmp += c;
                }
                res = tmp;
            }
            catch
            {

            }

            return res;
        }

        /// <summary>
        /// 转为简体
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToSimplified(string source)
        {
            source = RemoveVariant(source);
            string res = Microsoft.VisualBasic.Strings.StrConv(source, VbStrConv.SimplifiedChinese, 0);

            try
            {
                string[] pairs = File.ReadAllText(Directory.GetCurrentDirectory() + @"\s2t.txt").Split('|');
                Dictionary<string, string> changepairs = new Dictionary<string, string>();
                foreach(var p in pairs)
                {
                    string[] pair = p.Split(',');
                    if (!changepairs.ContainsKey(pair[1]))
                    {
                        changepairs[pair[1]] = pair[0];
                    }
                }
                string tmp = "";
                foreach(var c in res)
                {
                    if (changepairs.ContainsKey(c.ToString())) tmp += changepairs[c.ToString()];
                    else tmp += c;
                }
                res = tmp;
            }
            catch
            {

            }
            

            //String target = new String(' ', source.Length);
            //int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return res;
        }

        /// <summary>
        /// 转为繁体
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string ToTraditional(string source)
        {
            string target = Microsoft.VisualBasic.Strings.StrConv(source, VbStrConv.SimplifiedChinese, 1);
            return target;
        }
    }
}
