using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiebaNet.Segmenter.PosSeg;

namespace NovelAnalysis.AnalysisTools
{
    public class NumPair:IComparable<NumPair>
    {
        public string word;
        public string flag;
        public int num = 0;

        public NumPair(string w,string f,int n)
        {
            this.word = w;
            this.flag = f;
            this.num = n;
        }

        public int CompareTo(NumPair obj)
        {
            return num.CompareTo(obj.num) * -1;
        }
    }

    public class WordCutTool
    {

        public static List<Pair> cut(string str)
        {
            try
            {
                str = ChineseStringUtility.ToSimplified(str);
                str = removeBlanks(str);
                PosSegmenter p = new PosSegmenter();
                return p.Cut(str).ToList();
            }
            catch
            {
                return new List<Pair>();
            }

        }

        private static string removeBlanks(string ori)
        {
            string[] blanks = { "\t", " ", "　", "\r" };
            string res = ori;
            foreach (var b in blanks) res = res.Replace(b, string.Empty);
            return res;
        }

        public static string getString(List<Pair> p)
        {
            string res = "";
            foreach(var pair in p)
            {
                res = res + pair.Word;
            }
            return res;
        }


        public static List<NumPair> sumPair(List<Pair> words,string wordtype)
        {
            Dictionary<string, NumPair> res = new Dictionary<string, NumPair>();

            foreach (var w in words)
            {
                if (w.Flag.ToUpper().StartsWith(wordtype))
                {
                    if (res.ContainsKey(w.Word))
                    {
                        res[w.Word].num = res[w.Word].num + 1;
                    }
                    else
                    {
                        res[w.Word] = new NumPair(w.Word, w.Flag, 1);
                    }
                }

            }
            List<NumPair> list = new List<NumPair>();
            foreach (var v in res) list.Add(v.Value);
            list.Sort();

            return list;
        }

    }
}
