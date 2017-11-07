using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 

using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using System.Threading;
using System.Text.RegularExpressions;
using JiebaNet.Segmenter.PosSeg;
using Microsoft.VisualBasic;

namespace NovelAnalysis
{
    public class WordCut
    {
        //private Form1 mainForm;
        private DataController dc;
        private MyDelegate.sendStringDelegate printEvent;

        public WordCut(DataController data,string preString,MyDelegate.sendStringDelegate pevent)
        {
            this.printEvent = pevent;
            dc = data;
            dc.preResult = new List<List<string>>();
            dc.sentences = new List<Sentence>();
            //mainForm.dc.fileinfo = new List<FileInfo>();
            dc.preContent = preString;
            
        }

        private void print(string str)
        {
            if (printEvent != null)
            {
                printEvent(str);
            }
        }

        public static string removeBlanks(string ori)
        {
            string[] blanks = { "\t", " ", "　", "\r" };
            string res = ori;
            foreach (var b in blanks) res = res.Replace(b,string.Empty);
            return res;
        }



        /// <summary>
        /// 子线程分割文本为段落、单句
        /// </summary>
        public void workCutSentence()
        {
            dc.preResult = new List<List<string>>();
            print("开始分割句子和段落。");
            string[] paragraphs = dc.preContent.Split('\n');
            foreach (var parag in paragraphs)
            {
                List<string> thisparag = new List<string>();
                Regex regex = new Regex("[^。？！；…]+[。？！；…”’』」]*(?=[^。？！；…”’』」]*)");
                var res = regex.Matches(removeBlanks(parag));
                foreach (var ress in res)
                {
                    if (ress.ToString().Length >= 2)
                        thisparag.Add(ChineseStringUtility.ToSimplified(ress.ToString()));
                }
                if (thisparag.Count == 0) continue;
                dc.preResult.Add(thisparag);
            }
            
            print("分割完成。");
        }

        /// <summary>
        /// 使用分词器进行分词
        /// </summary>
        public void workCutString()
        {
            dc.sentences = null;
            dc.sentences = new List<Sentence>();
            print("开始分词。");
            try
            {
                MyDelegate.sendIntDelegate cutWordEvent=new MyDelegate.sendIntDelegate(workCutParagraph);
                MultiThreadingController mtc = new MultiThreadingController(dc.preResult.Count(), cutWordEvent);
                mtc.bStart();
                print("分词结束");
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }

        public void workSortSentences()
        {
            foreach (var f in dc.fileinfo)
            {
                f.sentences.Sort();
            }
        }

        /// <summary>
        /// 对单段分词的子线程函数
        /// </summary>
        private void workCutParagraph(int nowNum)
        {
            PosSegmenter segmenter = new PosSegmenter();
            print(string.Format("正在对第{0}段分词（共{1}段，{2}%）", 
                nowNum, 
                dc.preResult.Count, 
                Math.Round((double)nowNum * 100.0 / dc.preResult.Count, 
                2)
                ));
            try
            {
                for (int j = 0; j < dc.preResult[nowNum].Count; j++)
                {
                    try
                    {
                        //对单句分词
                        var words = segmenter.Cut(dc.preResult[nowNum][j]);
                        //标注句子索引
                        Sentence s = new Sentence(nowNum, j, words);

                        dc.sentences.Add(s);
                    }
                    catch
                    {
                        Sentence s = new Sentence(nowNum, j, new List<Pair>());
                        dc.sentences.Add(s);
                    }                   
                }
            }
            catch (Exception ex)
            {
                print("分词失败：" + ex.Message);
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
            foreach (var f in dc.fileinfo)
            {
                foreach (var s in f.sentences)
                {
                    if (s == null) continue;
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
            foreach (var f in dc.fileinfo)
            {
                foreach (var s in f.sentences)
                {
                    if (s == null) continue;
                    //mainForm.print("人名修正 - 词频统计（第" + (mainForm.dc.fileinfo.IndexOf(f) + 1) + "篇" + (f.sentneces.ToList().IndexOf(s) + 1) + "句)");
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
            //mainForm.print("修正分词结果完毕。");
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
        /// 将全部分词结果中包含给定单词的长词以给定词为基准分为两个词。新词标注为未知词u
        /// </summary>
        /// <param name="word"></param>
        private void resetCuts_Cut(Pair word)
        {
            //mainForm.print("修正词汇：" + word.Word);
            for (int i = 0; i < dc.fileinfo.Count; i++)
            {
                for (int j = 0; j < dc.fileinfo[i].sentences.Count(); j++)
                {
                    List<Pair> tmpwords = new List<Pair>();
                    int maxlen = dc.fileinfo[i].sentences[j].words.Count();
                    for (int k = 0; k < maxlen; k++)
                    {
                        Pair tmpword = dc.fileinfo[i].sentences[j].words.ToArray()[k];
                        int n = tmpword.Word.IndexOf(word.Word);
                        if (tmpword.Word != word.Word
                            && n >= 0
                            )
                        {
                            string newWord = tmpword.Word.Substring(n + word.Word.Length, tmpword.Word.Length - n - word.Word.Length);
                            Pair p = new Pair(newWord, "u");
                            tmpwords.Add(word);
                            tmpwords.Add(p);
                            k++;
                        }
                        else
                        {
                            tmpwords.Add(tmpword);
                        }
                    }
                    dc.fileinfo[i].sentences[j].words = tmpwords;
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
            //mainForm.print("修正词汇：" + word.Word);
            for (int i = 0; i < dc.fileinfo.Count; i++)
            {
                for (int j = 0; j < dc.fileinfo[i].sentences.Count(); j++)
                {
                    List<Pair> tmpwords = new List<Pair>();
                    int maxlen = dc.fileinfo[i].sentences[j].words.Count();
                    for (int k = 0; k < maxlen; k++)
                    {
                        if (k < maxlen - 1
                            &&
                            word.Word
                            ==
                            dc.fileinfo[i].sentences[j].words.ToArray()[k].Word
                            +
                            dc.fileinfo[i].sentences[j].words.ToArray()[k + 1].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 1;
                        }
                        else if (k < maxlen - 2
                            &&
                            word.Word
                            ==
                            dc.fileinfo[i].sentences[j].words.ToArray()[k].Word
                            +
                            dc.fileinfo[i].sentences[j].words.ToArray()[k + 1].Word
                            +
                            dc.fileinfo[i].sentences[j].words.ToArray()[k + 2].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 2;
                        }
                        else if (k < maxlen - 3
                           &&
                           word.Word
                           ==
                           dc.fileinfo[i].sentences[j].words.ToArray()[k].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 1].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 2].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 3].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 3;
                        }
                        else if (k < maxlen - 4
                           &&
                           word.Word
                           ==
                           dc.fileinfo[i].sentences[j].words.ToArray()[k].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 1].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 2].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 3].Word
                           +
                           dc.fileinfo[i].sentences[j].words.ToArray()[k + 4].Word
                        )
                        {
                            tmpwords.Add(word);
                            k += 4;
                        }
                        else
                        {
                            tmpwords.Add(dc.fileinfo[i].sentences[j].words.ToArray()[k]);
                        }
                    }
                    dc.fileinfo[i].sentences[j].words = tmpwords;
                }
            }
        }

    }
}
