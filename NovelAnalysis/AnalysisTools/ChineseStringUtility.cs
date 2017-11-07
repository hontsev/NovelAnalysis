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
                string v = "";
                string t = "";
                //Dictionary<char, char> changepairs = new Dictionary<char, char>();
                foreach (var p in pairs)
                {
                    string[] pair = p.Split(',');
                    v += pair[1];
                    t += pair[0];
                    //if (!changepairs.ContainsKey(pair[1][0]))
                    //{
                    //    changepairs[pair[1][0]] = pair[0][0];
                    //}
                }
                string tmp = "";
                char c;
                int index = 0;
                StringBuilder sb = new StringBuilder(res.Length);
                for(int i = 0; i < res.Length; i++)
                {
                    c = res[i];
                    index = v.IndexOf(c);
                    if (index > 0)
                        sb.Append(t[index]);
                    else
                        sb.Append(c);

                    //tmp += c;
                    //if (changepairs.ContainsKey(c)) tmp += changepairs[c];
                    //else tmp += c;
                }
                res = sb.ToString();
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
                string s = "";
                string t = "";
                //Dictionary<string, string> changepairs = new Dictionary<string, string>();
                foreach(var p in pairs)
                {
                    string[] pair = p.Split(',');
                    s += pair[0];
                    t += pair[1];
                    //if (!changepairs.ContainsKey(pair[1]))
                    //{
                    //    changepairs[pair[1]] = pair[0];
                    //}
                }
                StringBuilder sb = new StringBuilder(res.Length);
                foreach(var c in res)
                {
                    //if (changepairs.ContainsKey(c.ToString())) tmp += changepairs[c.ToString()];
                    //else tmp += c;
                    int index = t.IndexOf(c);
                    if (index >= 0) sb.Append(s[index]);
                    else sb.Append(c);
                }
                res = sb.ToString();
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

