using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using System.Threading;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter.PosSeg;

namespace NovelAnalysis
{
    public class WordCut
    {
        private Form1 mainForm;

        public WordCut(Form1 mainf,string preString)
        {
            mainForm = mainf;
            mainForm.dc.preResult = new List<List<string>>();
            mainForm.dc.sentences = new List<Sentence>();
            //mainForm.dc.fileinfo = new List<FileInfo>();
            mainForm.dc.preContent = preString;
            
        }

        /// <summary>
        /// 子线程分割文本为段落、单句
        /// </summary>
        public void workCutSentence()
        {
            mainForm.dc.preResult = new List<List<string>>();
            mainForm.print("开始分割句子和段落。");
            string[] paragraphs = mainForm.dc.preContent.Split('\n');
            foreach (var parag in paragraphs)
            {
                List<string> thisparag = new List<string>();
                Regex regex = new Regex("[^。？！；]+[。？！；]?");
                var res = regex.Matches(parag);
                foreach (var ress in res)
                {
                    if (ress.ToString().Length >= 2)
                        thisparag.Add(ress.ToString());
                }
                mainForm.dc.preResult.Add(thisparag);
            }
            mainForm.print("分割完成。");
        }

        /// <summary>
        /// 使用分词器进行分词
        /// </summary>
        public void workCutString()
        {
            mainForm.dc.sentences = null;
            mainForm.dc.sentences = new List<Sentence>();
            mainForm.print("开始分词。共有" + mainForm.dc.preResult.Count() + "个段落");
            try
            {
                MyDelegate.sendIntDelegate cutWordEvent=new MyDelegate.sendIntDelegate(workCutParagraph);
                MultiThreadingController mtc = new MultiThreadingController(mainForm.dc.preResult.Count(), cutWordEvent);
                mtc.bStart();
                mainForm.print("分词结束");
            }
            catch (Exception e)
            {
                mainForm.print(e.Message);
            }
        }

        /// <summary>
        /// 对单段分词的子线程函数
        /// </summary>
        private void workCutParagraph(int nowNum)
        {
            PosSegmenter segmenter = new PosSegmenter();
            mainForm.print("正在对第" + nowNum + "段分词（共" + mainForm.dc.preResult.Count + "段，" + Math.Round((double)nowNum * 100.0 / mainForm.dc.preResult.Count, 2) + "%）");
            try
            {
                for (int j = 0; j < mainForm.dc.preResult[nowNum].Count; j++)
                {
                    var words = segmenter.Cut(mainForm.dc.preResult[nowNum][j]);
                    Sentence s = new Sentence(nowNum, j, words);
                    mainForm.dc.sentences.Add(s);
                }
            }
            catch (Exception ex)
            {
                mainForm.print("分词失败：" + ex.Message);
            }
        }

        /// <summary>
        /// 修正分词结果
        /// </summary>
        public void workResetWordCut()
        {
            List<Pair> tmpWordInfo;
            //修正：从全文分词结果来看，将被错误分割的词拼接起来
            tmpWordInfo = new List<Pair>();
            List<Pair> tmpChangeWordInfo = new List<Pair>();
            foreach (var f in mainForm.dc.fileinfo)
            {
                foreach (var s in f.sentneces)
                {
                    foreach (var w in s.words)
                    {
                        if (w.Word.Length > 1 && isInList(tmpWordInfo, w))
                        {
                            tmpWordInfo.Add(w);
                        }
                    }
                }
            }
            foreach (var w in tmpWordInfo)
            {
                resetCuts_Link(w);
            }
            //修正：以出现频率较高的人名为基准，将未正确分割的含人名的词分开
            List<WordInfo> tmpWord = new List<WordInfo>();
            foreach (var f in mainForm.dc.fileinfo)
            {
                foreach (var s in f.sentneces)
                {
                    mainForm.print("人名修正 - 词频统计（第" + (mainForm.dc.fileinfo.IndexOf(f) + 1) + "篇" + (f.sentneces.ToList().IndexOf(s) + 1) + "句)");
                    foreach (var w in s.words)
                    {
                        if (w.Word.Length > 1 && w.Flag == "nr")
                        {
                            bool haveit = false;
                            foreach (WordInfo winfo in tmpWord)
                            {
                                if (w.Word == winfo.word)
                                {
                                    winfo.sum++;
                                    haveit = true;
                                    break;
                                }
                            }
                            if (haveit) continue;
                            WordInfo newwinfo = new WordInfo();
                            newwinfo.word = w.Word;
                            newwinfo.sum = 1;
                            newwinfo.wordType = w.Flag;
                            tmpWord.Add(newwinfo);
                        }
                    }
                }
            }
            foreach (var wi in tmpWord)
            {
                if (wi.sum >= 10)
                {
                    Pair p = new Pair(wi.word, wi.wordType);
                    resetCuts_Cut(p);
                }
            }
            mainForm.print("修正分词结果完毕。");
        }

        /// <summary>
        /// 按值比较词是否在词列表内
        /// </summary>
        /// <param name="pairList"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool isInList(List<WordInfo> pairList, WordInfo element)
        {
            foreach (var p in pairList)
            {
                if (p.word == element.word) return true;
            }
            return false;
        }

        /// <summary>
        /// 按值比较词是否在词列表内
        /// </summary>
        /// <param name="pairList"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool isInList(List<Pair> pairList, Pair element)
        {
            foreach (var p in pairList)
            {
                if (p.Word == element.Word) return true;
            }
            return false;
        }

        /// <summary>
        /// 将全部分词结果中包含给定单词的长词以给定词为基准分为两个词。新词标注为动词v
        /// </summary>
        /// <param name="word"></param>
        private void resetCuts_Cut(Pair word)
        {
            mainForm.print("修正词汇：" + word.Word);
            for (int i = 0; i < mainForm.dc.fileinfo.Count; i++)
            {
                for (int j = 0; j < mainForm.dc.fileinfo[i].sentneces.Count(); j++)
                {
                    List<Pair> tmpwords = new List<Pair>();
                    int maxlen = mainForm.dc.fileinfo[i].sentneces[j].words.Count();
                    for (int k = 0; k < maxlen; k++)
                    {
                        Pair tmpword = mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k];
                        int n = tmpword.Word.IndexOf(word.Word);
                        if (tmpword.Word != word.Word
                            && n >= 0
                            )
                        {
                            string newWord = tmpword.Word.Substring(n + word.Word.Length, tmpword.Word.Length - n - word.Word.Length);
                            Pair p = new Pair(newWord, "v");
                            tmpwords.Add(word);
                            tmpwords.Add(p);
                            k++;
                        }
                        else
                        {
                            tmpwords.Add(tmpword);
                        }
                    }
                    mainForm.dc.fileinfo[i].sentneces[j].words = tmpwords;
                }
            }
        }

        /// <summary>
        /// 将该单词视作分词颗粒，将关于它的分开的词合并
        /// 具体做法是，在分词结果中遍历用各个词及其后面n（n取1、2、3、4）个词拼接起来的字符串，
        /// 如果这字符串与给出的要分割的词相同，则将词与后面n个词合并为单一词。
        /// 例如结果中有n=“中”，n+1=“国”，而传入参数为“中国”，则将词n设为“中国”，词n+1删掉。
        /// </summary>
        /// <param name="word"></param>
        private void resetCuts_Link(Pair word)
        {
            mainForm.print("修正词汇：" + word.Word);
            for (int i = 0; i < mainForm.dc.fileinfo.Count; i++)
            {
                for (int j = 0; j < mainForm.dc.fileinfo[i].sentneces.Count(); j++)
                {
                    List<Pair> tmpwords = new List<Pair>();
                    int maxlen = mainForm.dc.fileinfo[i].sentneces[j].words.Count();
                    for (int k = 0; k < maxlen; k++)
                    {
                        if (k < maxlen - 1
                            &&
                            word.Word
                            ==
                            mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k].Word
                            +
                            mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 1].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 1;
                        }
                        else if (k < maxlen - 2
                            &&
                            word.Word
                            ==
                            mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k].Word
                            +
                            mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 1].Word
                            +
                            mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 2].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 2;
                        }
                        else if (k < maxlen - 3
                           &&
                           word.Word
                           ==
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 1].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 2].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 3].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 3;
                        }
                        else if (k < maxlen - 4
                           &&
                           word.Word
                           ==
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 1].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 2].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 3].Word
                           +
                           mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k + 4].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 4;
                        }
                        else
                        {
                            tmpwords.Add(mainForm.dc.fileinfo[i].sentneces[j].words.ToArray()[k]);
                        }
                    }
                    mainForm.dc.fileinfo[i].sentneces[j].words = tmpwords;
                }
            }
        }

    }
}
