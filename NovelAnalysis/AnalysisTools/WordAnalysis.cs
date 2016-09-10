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
        private Form1 mainForm;

        public WordAnalysis(Form1 mainf)
        {
            mainForm = mainf;
        }

        public void initAnalysis()
        {
            mainForm.dc.wordinfo = null;
            mainForm.dc.wordinfo = new List<WordInfo>();
        }

        public List<WordInfo> getResult()
        {
            mainForm.dc.wordinfo.Sort();
            return mainForm.dc.wordinfo;
        }

        /// <summary>
        /// 添加一条词的出现记录
        /// </summary>
        /// <param name="word"></param>
        /// <param name="sentence"></param>
        public void addWordRecord(Pair word,Sentence sentence)
        {
            foreach (WordInfo winfo in mainForm.dc.wordinfo)
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
            mainForm.dc.wordinfo.Add(newwinfo);
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
        public List<string> getItemStrings(string itemStr)
        {
            List<string> res = new List<string>();
            foreach (FileInfo file in mainForm.dc.fileinfo)
            {
                foreach (Sentence sen in file.sentneces)
                {
                    foreach (Pair w in sen.words)
                    {
                        if (w.Word == itemStr)
                        {
                            res.Add(getSentenseString(sen));
                            break;
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// 获取相应类型的词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<WordInfo> getWordsByName(string keyword)
        {
            List<WordInfo> winfo = new List<WordInfo>();
            foreach (var w in mainForm.dc.wordinfo)
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
            foreach (var w in mainForm.dc.wordinfo)
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
    }
}
