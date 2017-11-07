using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiebaNet.Segmenter.PosSeg;
using System.Runtime.InteropServices;

namespace NovelAnalysis.AnalysisTools
{
    public class WordPair:IComparable<WordPair>
    {
        public string Word;
        public string Flag;
        public double Num = 0;

        public WordPair(string w,string f, double n =1)
        {
            this.Word = w;
            this.Flag = f;
            this.Num = n;
        }

        public WordPair()
        {
            Word = "";
            Flag = "";
            Num = 0;
        }

        public WordPair(Pair p)
        {
            this.Word = p.Word;
            this.Flag = p.Flag;
            this.Num = 1;
        }

        public WordPair(WordPair p)
        {
            this.Word = p.Word;
            this.Flag = p.Flag;
            this.Num = p.Num;
        }

        public int CompareTo(WordPair obj)
        {
            return Num.CompareTo(obj.Num) * -1;
        }
    }

    public enum CutTool
    {
        jieba,nlpir
    }

    public class WordCutTool
    {
        public static List<WordPair> cut(string str, CutTool tool=CutTool.jieba)
        {
            try
            {
                str = ChineseStringUtility.ToSimplified(str);
                str = removeBlanks(str);
                List<WordPair> res = new List<WordPair>();
                List<Pair> tmp = new List<Pair>();
                switch (tool)
                {
                    case CutTool.jieba:
                        //jieba分词
                        PosSegmenter p = new PosSegmenter();
                        tmp=p.Cut(str).ToList();
                        break;
                    case CutTool.nlpir:
                        //NLPIR-ICTCLAS分词
                        tmp = cutByICTCLAS(str);
                        break;
                    default:
                        break;
                }
                foreach (var p in tmp) res.Add(new WordPair(p));

                return res;
            }
            catch
            {
                return new List<WordPair>();
            }

        }

        private static List<Pair> cutByICTCLAS(string str)
        {
            List<Pair> res = new List<Pair>();
            if(ICTCLAS.NLPIR_Init("", 0, ""))
            {
                //ICTCLAS.NLPIR_NWI_Start();
                //ICTCLAS.NLPIR_NWI_AddMem(str);
                //var ptr2 = ICTCLAS.NLPIR_NWI_GetResult();
                //string t2 = Marshal.PtrToStringAnsi(ptr2);
                //ICTCLAS.NLPIR_NWI_Result2UserDict();
                //ICTCLAS.NLPIR_NWI_Complete();
                //ICTCLAS.NLPIR_SaveTheUsrDic();
                var intptr = ICTCLAS.NLPIR_ParagraphProcess(str, 1);
                string t = Marshal.PtrToStringAnsi(intptr);
                string[] words = t.Split(' ');
                foreach(var w in words)
                {
                    if (!string.IsNullOrWhiteSpace(w))
                    {
                        string[] pairinfo = w.Split('/');
                        if (pairinfo.Length == 2)
                        {
                            Pair p = new Pair(pairinfo[0], pairinfo[1]);
                            res.Add(p);
                        }
                    }
                }
            }
            ICTCLAS.NLPIR_Exit();
            return res;
        }

        public static List<List<WordPair>> cutSentence(string str)
        {
            var result = new List<List<WordPair>>();

            var tmp = cut(str);
            List<WordPair> tmppair = new List<WordPair>();
            for(int i = 0; i < tmp.Count; i++)
            {
                //if ("。；,，—─.（）‘’“”？！…?!\r\n".Contains(tmp[i].Word))
                if(tmp[i].Flag.ToUpper().StartsWith("W"))
                {
                    if(tmppair.Count>0) result.Add(tmppair);
                    tmppair = new List<WordPair>();
                }
                else
                {
                    tmppair.Add(tmp[i]);
                }
            }

            return result;
        }

        private static string removeBlanks(string ori)
        {
            string[] blanks = { "\t", " ", "　", "\r" };
            string res = ori;
            foreach (var b in blanks) res = res.Replace(b, string.Empty);
            return res;
        }

        public static string getString(List<WordPair> p)
        {
            string res = "";
            foreach(var pair in p)
            {
                res = res + pair.Word;
            }
            return res;
        }


        public static List<WordPair> sumPair(List<WordPair> words,string wordtype)
        {
            Dictionary<string, WordPair> res = new Dictionary<string, WordPair>();

            foreach (var w in words)
            {
                if (w.Flag.ToUpper().StartsWith(wordtype))
                {
                    if (res.ContainsKey(w.Word))
                    {
                        res[w.Word].Num = res[w.Word].Num + 1;
                    }
                    else
                    {
                        res[w.Word] = new WordPair(w.Word, w.Flag, 1);
                    }
                }

            }
            List<WordPair> list = new List<WordPair>();
            foreach (var v in res) list.Add(v.Value);
            list.Sort();

            return list;
        }

    }
}
