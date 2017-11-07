using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NovelAnalysis.IOTools;

namespace NovelAnalysis.AnalysisTools
{
    public class WordDF
    {
        public string Word;
        public int dn;
        public int n;

        public WordDF(string word = "", int tn = 0, int tdn = 0)
        {
            Word = word;
            dn = tdn;
            n = tn;
        }
    }

    public class AnalyzeTool
    {
        public static string wordDFfile = @"WordDFs.txt";
        public static string dataDic = @"./MyData/";

        public static List<WordPair> getTFIDF(string str, Dictionary<string, WordDF> dfs)
        {
            //List<WordPair> words = WordCutTool.cut(str, CutTool.nlpir);

            Dictionary<string, WordPair> res = getTF(str);
            //foreach (var w in words)
            //{
            //    if (!w.Flag.ToUpper().StartsWith("N") && !w.Flag.ToUpper().StartsWith("V")) continue;
            //    if (!res.ContainsKey(w.Word)) { res[w.Word] = new WordPair(w); res[w.Word].Num = 1; }
            //    else res[w.Word].Num += 1;
            //}
            foreach (var w in res)
            {
                if (dfs.ContainsKey(w.Key))
                {
                    double idf = dfs[w.Key].dn / dfs[w.Key].n;
                    double tf = (double)w.Value.Num / res.Count;
                    w.Value.Num = tf * idf;
                }
            }
            List<WordPair> resList = new List<WordPair>();
            foreach (var w in res)
            {
                if (w.Value.Word.Length > 1) resList.Add(w.Value);
            }
            resList.Sort();
            return resList;
        }

        public static List<WordPair> getTFWithSort(string str,bool all = true)
        {
            Dictionary<string, WordPair> tfs = getTF(str, all);
            List<WordPair> words = tfs.Values.ToList();
            words.Sort();
            return words;
        }
        
        public static Dictionary<string, WordPair> getTF(string str,bool all=true)
        {
            List<WordPair> words = WordCutTool.cut(str, CutTool.nlpir);
            Dictionary<string, WordPair> res = new Dictionary<string, WordPair>();
            foreach (var w in words)
            {
                string flag = w.Flag.ToUpper();
                if (all && !flag.StartsWith("N") && !flag.StartsWith("V")) continue;
                if (flag.StartsWith("W")) continue;

                if (!res.ContainsKey(w.Word)) { res[w.Word] = new WordPair(w); res[w.Word].Num = 1; }
                else res[w.Word].Num += 1;
            }
            return res;
        }

        public static Dictionary<string,bool> loadStopWords()
        {
            string filename = "StopWords.txt";
            if (!File.Exists(filename)) File.Create(filename);
            string[] stopwords = File.ReadAllLines(filename, Encoding.UTF8);
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            foreach (var v in stopwords) dic[v.Trim()] = true;
            return dic;
        }

        public static Dictionary<string, WordDF> loadDF()
        {

            Dictionary<string, WordDF> dfs = new Dictionary<string, WordDF>();
            try
            {
                string[] lines = File.ReadAllLines(wordDFfile, Encoding.UTF8);
                foreach (var l in lines)
                {
                    string[] tmp = l.Split(',');
                    if (tmp.Length >= 3)
                    {
                        string Word = tmp[0];
                        int n = int.Parse(tmp[1]);
                        int dn = int.Parse(tmp[2]);
                        dfs[Word] = new WordDF(Word, n, dn);
                    }
                }
            }
            catch
            {

            }


            return dfs;
        }

        public static void saveDF(Dictionary<string, WordDF> dfs)
        {
            using (FileStream fs = new FileStream(wordDFfile, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var df in dfs.Values)
                    {
                        sw.WriteLine(string.Format("{0},{1},{2}", df.Word, df.n, df.dn));
                    }
                }
            }
        }

        public static Dictionary<string, WordDF> getIDF(Dictionary<string, WordDF> dfs = null, string[] files = null)
        {

            //string[] files = Directory.GetFiles(dataDic, "*.txt");
            if (files == null || files.Length <= 0) return new Dictionary<string, WordDF>();
            int filenum = files.Length;
            if (dfs == null) dfs = new Dictionary<string, WordDF>();

            int oldfilenum = 0;
            foreach (var df in dfs) if (df.Value.dn > oldfilenum) oldfilenum = df.Value.dn;

            foreach(string file in files)
            {
                string filecontent = File.ReadAllText(file,TxtIOController.getEncoding2(file));
                var pairs = WordCutTool.cut(filecontent,CutTool.nlpir);
                Dictionary<string, WordPair> wordpairs = new Dictionary<string, WordPair>();
                foreach (var p in pairs) wordpairs[p.Word] = p;
                foreach(var word in wordpairs)
                {
                    if (word.Value.Flag.ToUpper().StartsWith("N") || word.Value.Flag.ToUpper().StartsWith("V"))
                    {
                        // 只分析名词和动词
                        if (!dfs.ContainsKey(word.Key))
                        {
                            dfs[word.Key] = new WordDF(word.Key, 0, 0);
                        }
                        dfs[word.Key].n += 1;
                    }
                }
            }
            foreach(var key in dfs.Keys)
            {
                dfs[key].dn = oldfilenum + filenum;
            }
            return dfs;
        }

        public static Dictionary<string,double> getTFIDF(string str)
        {
            return null;
        }
    }
}
