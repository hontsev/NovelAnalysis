using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JiebaNet.Segmenter.PosSeg;
using System.Text.RegularExpressions;

namespace NovelAnalysis
{
    public class WordAnalysis
    {
        private DataController dc;
        //private Form1 mainForm;

        public WordAnalysis(DataController data)
        {
            this.dc = data;
        }

        public void initAnalysis()
        {
            dc.wordinfo = null;
            dc.wordinfo = new List<WordInfo>();
        }

        public List<WordInfo> getResult()
        {
            dc.wordinfo.Sort();
            return dc.wordinfo;
        }

        /// <summary>
        /// 添加一条词的出现记录
        /// </summary>
        /// <param name="word"></param>
        /// <param name="sentence"></param>
        public void addWordRecord(Pair word,Sentence sentence)
        {
            foreach (WordInfo winfo in dc.wordinfo)
            {
                if (word.Word == winfo.word)
                {
                    winfo.sum++;
                    winfo.appearSentences.Add(sentence);
                    return;
                }
            }
            WordInfo newwinfo = new WordInfo();
            newwinfo.word = word.Word;
            newwinfo.sum = 1;
            newwinfo.wordType = word.Flag;
            newwinfo.appearSentences = new List<Sentence>();
            newwinfo.appearSentences.Add(sentence);
            dc.wordinfo.Add(newwinfo);
        }

        /// <summary>
        /// 判断这个词是否为标点符号
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public bool isSymbol(Pair w)
        {
            Regex reg = new Regex(@"\p{P}");
            return reg.IsMatch(w.Word);
        }

        /// <summary>
        /// 获取某个句子的原本文字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string getSentenseString(Sentence s)
        {
            string res = "";
            foreach (var w in s.words)
            {
                res += w.Word;
            }
            return res;
        }

        /// <summary>
        /// 获取包含此词的句子们
        /// </summary>
        /// <param name="itemStr"></param>
        /// <returns></returns>
        public string[,] getItemStrings(string itemStr)
        {
            List<string> res = new List<string>();
            List<string> resFrom = new List<string>();
            foreach (FileInfo file in dc.fileinfo)
            {
                foreach (Sentence sen in file.sentences)
                {
                    foreach (Pair w in sen.words)
                    {
                        if (w.Word == itemStr)
                        {
                            res.Add(getSentenseString(sen));
                            resFrom.Add(file.fileName);
                            break;
                        }
                    }
                }
            }
            string[,] resdata = new string[res.Count, 2];
            for(int i = 0; i < res.Count; i++)
            {
                resdata[i, 0] = res[i];
                resdata[i, 1] = resFrom[i];
            }
            return resdata;
        }

        /// <summary>
        /// 获取相应类型的词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<WordInfo> getWordsByName(string keyword)
        {
            List<WordInfo> winfo = new List<WordInfo>();
            foreach (var w in dc.wordinfo)
            {
                if (w.word.IndexOf(keyword) >= 0)
                {
                    winfo.Add(w);
                }
            }
            return winfo;
        }

        /// <summary>
        /// 获取相应类型的词
        /// </summary>
        /// <param name="typeNames"></param>
        /// <returns></returns>
        public List<WordInfo> getWordsByType(List<string> typeNames)
        {
            List<WordInfo> winfo = new List<WordInfo>();
            foreach (var w in dc.wordinfo)
            {
                foreach (var typeName in typeNames)
                {
                    if (w.wordType == typeName)
                    {
                        winfo.Add(w);
                        break;
                    }
                }
            }
            return winfo;
        }


        public void setAnalysisSentences()
        {

        }



        private static string getFlagChineseName(string flag)
        {
            string res = string.Empty;

            //if (flag.IndexOf('n') == 0) res = "名词";
            //else if (flag.IndexOf('a') == 0) res = "副词或形容词";
            if (flag.IndexOf('v') == 0) res = "动词";

            return res;
        }

        public string getVerbsInfo()
        {
            string res = "";

            for (int i = 0; i < dc.fileinfo[0].sentences.Count; i++)
            {
                //res += string.Format("{0}-{1}:\r\n", dc.fileinfo[0].sentences[i].paragraphNumber, dc.fileinfo[0].sentences[i].sentenceNumber);
                for (int j=0;j<dc.fileinfo[0].sentences[i].words.Count;j++)
                {
                    Pair w = dc.fileinfo[0].sentences[i].words[j];
                    string flag = getFlagChineseName(w.Flag);
                        if(flag=="动词"){
                            if(j>=2)res+=string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j-2].Word, dc.fileinfo[0].sentences[i].words[j-2].Flag);
                            if(j>=1)res+=string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j-1].Word, dc.fileinfo[0].sentences[i].words[j-1].Flag);
                            res += string.Format("【{0}】", w.Word, flag);
                            if(j<dc.fileinfo[0].sentences[i].words.Count-1)res+=string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j+1].Word, dc.fileinfo[0].sentences[i].words[j+1].Flag);
                            if(j<dc.fileinfo[0].sentences[i].words.Count-2)res+=string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j+2].Word, dc.fileinfo[0].sentences[i].words[j+2].Flag);
                            res+="\r\n";
                        }else{
                            //res += w.Word;
                        }
                                           
                }
                res += "\r\n";
            }

            return res;
        }

        public string getGsInfo()
        {
            string res = "";

            for (int i = 0; i < dc.fileinfo[0].sentences.Count; i++)
            {
                //res += string.Format("{0}-{1}:\r\n", dc.fileinfo[0].sentences[i].paragraphNumber, dc.fileinfo[0].sentences[i].sentenceNumber);
                for (int j = 0; j < dc.fileinfo[0].sentences[i].words.Count; j++)
                {
                    Pair w = dc.fileinfo[0].sentences[i].words[j];
                    if (w.Flag.EndsWith("g"))
                    {
                        if (j >= 2) res += string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j - 2].Word, dc.fileinfo[0].sentences[i].words[j - 2].Flag);
                        if (j >= 1) res += string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j - 1].Word, dc.fileinfo[0].sentences[i].words[j - 1].Flag);
                        res += string.Format("【{0}】", w.Word);
                        if (j < dc.fileinfo[0].sentences[i].words.Count - 1) res += string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j + 1].Word, dc.fileinfo[0].sentences[i].words[j + 1].Flag);
                        if (j < dc.fileinfo[0].sentences[i].words.Count - 2) res += string.Format("({0}/{1})", dc.fileinfo[0].sentences[i].words[j + 2].Word, dc.fileinfo[0].sentences[i].words[j + 2].Flag);
                        res += "\r\n";
                    }
                    else
                    {
                        //res += w.Word;
                    }

                }
                res += "\r\n";
            }

            return res;
        }

        public string getWordInfo()
        {
            string res = "";


            for (int i = 0; i < dc.fileinfo[0].sentences.Count; i++)
            {
                res += string.Format("{0}-{1}:\r\n", dc.fileinfo[0].sentences[i].paragraphNumber, dc.fileinfo[0].sentences[i].sentenceNumber);
                string resstr = "";
                int verbNumber = 0;
                foreach (var w in dc.fileinfo[0].sentences[i].words)
                {
                    
                    string flag = getFlagChineseName(w.Flag);
                    if (w.Word == "，" || w.Word == "；")
                    {
                        if (verbNumber == 1)
                        {
                            resstr += string.Format("{0}\r\n", w.Word);
                            res += resstr + "\r\n";
                        }
                        resstr = "";
                        verbNumber = 0;
                    }
                    else if (string.IsNullOrEmpty(flag))
                        resstr += w.Word;
                    else if (flag=="动词")
                    {
                        resstr += string.Format("【{0}<{1}>】", w.Word, flag);
                        verbNumber++;
                    }
                       
                }
                if (verbNumber != 1) resstr = "";
                if(!string.IsNullOrWhiteSpace(resstr))
                    res += resstr+"\r\n";
            }

           return res;
        }



    }
}
