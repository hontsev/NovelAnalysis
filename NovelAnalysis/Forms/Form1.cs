using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using JiebaNet.Segmenter.PosSeg;
using System.Threading;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using NovelAnalysis.AnalysisTools;

namespace NovelAnalysis
{
    /// <summary>
    /// 表示系统当前处于的状态
    /// </summary>
    public enum Status
    {
        init,       //系统初始化
        reading,    //正在读取文件
        readed,     //文件读取完毕
        anaingWord, //正在做词语分析
        anaedWord   //词语分析完毕
    }


    public partial class Form1 : Form
    {

        //MyDelegate.sendVoidDelegate setFileInfoListEvent;
        //MyDelegate.sendVoidDelegate setWordInfoListEvent;
        //MyDelegate.sendStatusDelegate setControllersStatusEvent;
        MyDelegate.sendStringDelegate printEvent;
        public WordCut wc;
        public DataController dc;

        string[] inputFileName ;

        List<WordInfo> wordinfo;
        //List<Pair> tmpWordInfo;

        WordAnalysis ana;

        public Form1()
        {
            InitializeComponent();
            tabControl1.Parent = this;

            dc = new DataController();
            ana = new WordAnalysis(this.dc);
            printEvent = new MyDelegate.sendStringDelegate(print);

            //setFileInfoListEvent = new MyDelegate.sendVoidDelegate(setFileInfoList);
            //setWordInfoListEvent = new MyDelegate.sendVoidDelegate(setWordInfoList);
            //setControllersStatusEvent = new MyDelegate.sendStatusDelegate(setControllersStatus);

            setControllersStatus(Status.init);

            print("初始化程序完毕。");
        }

        #region 控件操纵函数

        /// <summary>
        /// 设置控件们的状态
        /// </summary>
        /// <param name="s"></param>
        private void setControllersStatus(Status s)
        {
            if (listBox1.InvokeRequired)
            {
                MyDelegate.sendStatusDelegate mEvent = new MyDelegate.sendStatusDelegate(setControllersStatus);
                Invoke(mEvent, (object)(s));
            }
            else
            {
                switch (s)
                {
                    case Status.init:
                        保存为JSONToolStripMenuItem.Enabled = false;
                        listBox1.Enabled = false;
                        groupBox1.Enabled = false;
                        button1.Enabled = false;
                        groupBox2.Enabled = false;
                        panel1.Enabled = false;
                        break;
                    case Status.reading:
                        保存为JSONToolStripMenuItem.Enabled = false;
                        listBox1.Enabled = false;
                        groupBox1.Enabled = false;
                        button1.Enabled = false;
                        groupBox2.Enabled = false;
                        panel1.Enabled = false;
                        break;
                    case Status.readed:
                        保存为JSONToolStripMenuItem.Enabled = true;
                        listBox1.Enabled = true;
                        groupBox1.Enabled = true;
                        button1.Enabled = true;
                        groupBox2.Enabled = false;
                        panel1.Enabled = false;
                        break;
                    case Status.anaingWord:
                        保存为JSONToolStripMenuItem.Enabled = true;
                        listBox1.Enabled = true;
                        groupBox1.Enabled = true;
                        button1.Enabled = false;
                        groupBox2.Enabled = false;
                        panel1.Enabled = false;
                        break;
                    case Status.anaedWord:
                        保存为JSONToolStripMenuItem.Enabled = true;
                        listBox1.Enabled = true;
                        groupBox1.Enabled = true;
                        button1.Enabled = false;
                        groupBox2.Enabled = true;
                        panel1.Enabled = true;
                        break;
                    default:
                        break;
                }
            }
        }


        

        /// <summary>
        /// 输出到信息栏
        /// </summary>
        /// <param name="str"></param>
        public void print(string str)
        {
            if (label2.InvokeRequired)
            {
                MyDelegate.sendStringDelegate printEvent = new MyDelegate.sendStringDelegate(print);
                Invoke(printEvent, (object)(str));
            }
            else
            {
                label2.Text = str;
            }
        }

        /// <summary>
        /// 打印文章概要
        /// </summary>
        /// <param name="str"></param>
        private void printText(string str)
        {
            if (textBoxSummary.InvokeRequired)
            {
                MyDelegate.sendStringDelegate printTextEvent = new MyDelegate.sendStringDelegate(printText);
                Invoke(printTextEvent, (object)(str));
            }
            else
            {
                textBoxSummary.Text =  str + "\r\n……";
            }
        }

        /// <summary>
        /// 打印文章概要
        /// </summary>
        /// <param name="str"></param>
        private void printChangeResult(string str)
        {
            if (textBox4.InvokeRequired)
            {
                MyDelegate.sendStringDelegate printTextEvent = new MyDelegate.sendStringDelegate(printChangeResult);
                Invoke(printTextEvent, (object)(str));
            }
            else
            {
                textBox4.Text = str;
            }
        }

        /// <summary>
        /// 更新文件列表信息
        /// </summary>
        private void setFileInfoList()
        {
            if (listBox1.InvokeRequired)
            {
                MyDelegate.sendVoidDelegate mEvent = new MyDelegate.sendVoidDelegate(setFileInfoList);
                Invoke(mEvent);
            }
            else
            {
                listBox1.Items.Clear();
                foreach (var info in dc.fileinfo)
                {
                    listBox1.Items.Add(info.fileName);
                }
                if (listBox1.Items.Count >= 1)
                {
                    listBox1.SelectedIndex = 0;
                    setFileInfo(dc.fileinfo[0]);
                }
            }

        }

        /// <summary>
        /// 更新文件详情内容
        /// </summary>
        /// <param name="finfo"></param>
        private void setFileInfo(FileInfo finfo=null)
        {
            if (finfo == null)
            {
                labelName.Text = "未知";
                labelCharacter.Text = "未知";
                labelSentence.Text = "未知";
                labelParagraph.Text = "未知";
                textBoxSummary.Text = "";
            }
            else
            {
                labelName.Text = finfo.fileName.Length > 10 ? finfo.fileName.Substring(0, 10) + "…" : finfo.fileName;
                labelCharacter.Text = finfo.characterNum.ToString();
                labelSentence.Text = finfo.sentenceNum.ToString();
                labelParagraph.Text = finfo.paragraphNum.ToString();
                textBoxSummary.Text = finfo.summary;
            }
        }

        /// <summary>
        /// 更新词语列表
        /// </summary>
        private void setWordInfoList()
        {
            if (listView1.InvokeRequired)
            {
                MyDelegate.sendVoidDelegate mEvent = new MyDelegate.sendVoidDelegate(setWordInfoList);
                Invoke(mEvent);
            }
            else
            {
                listView1.Items.Clear();
                foreach (var winfo in wordinfo)
                {
                    ListViewItem item = new ListViewItem(winfo.word);
                    item.SubItems.Add(winfo.wordType);
                    item.SubItems.Add(winfo.sum.ToString());
                    listView1.Items.Add(item);
                }
            }
        }

        private void setWordsInfo()
        {
            if (richTextBox2.InvokeRequired)
            {
                MyDelegate.sendVoidDelegate mEvent = new MyDelegate.sendVoidDelegate(setWordsInfo);
                Invoke(mEvent);
            }
            else
            {
                richTextBox2.Text = ana.getGsInfo();
            }
            
        }

        /// <summary>
        /// 显示包含该词的句子信息
        /// </summary>
        private void setSentensesInfo()
        {
            if (listView1.SelectedItems.Count >= 1)
            {
                var item = listView1.SelectedItems[0];
                string wordStr = item.Text.ToString();
                string[,] res = ana.getItemStrings(wordStr);
                richTextBox1.Text = "";
                for(int i = 0; i < res.GetLength(0); i++)
                {
                    Regex r = new Regex(wordStr);
                    var begins = getIndexes(res[i,0], wordStr);
                    for (int j = 0; j < res[i,0].Length; j++)
                    {
                        if (begins.IndexOf(j) >= 0)
                        {
                            richTextBox1.SelectionStart = richTextBox1.TextLength;
                            richTextBox1.SelectionLength = 0;
                            richTextBox1.SelectionColor = Color.Red;
                            for (int k = 0; k < wordStr.Length; k++)
                            {
                                richTextBox1.AppendText(res[i,0][j].ToString());
                                j++;
                            }
                            j--;
                        }
                        else
                        {
                            richTextBox1.SelectionColor = richTextBox1.ForeColor;
                            richTextBox1.AppendText(res[i, 0][j].ToString());
                        }
                    }

                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
                    richTextBox1.AppendText(" - (" + res[i, 1] + ")");
                    richTextBox1.AppendText("\r\n\r\n");
                }
            }
        }



        /// <summary>
        /// 获取一个字符串中某个子串所有出现的位置
        /// </summary>
        /// <param name="str"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private List<int> getIndexes(string str, string target)
        {
            string tmpstr = str;
            List<int> res=new List<int>();
            int n=tmpstr.IndexOf(target);
            int passn = 0;
            while (n >= 0)
            {
                res.Add(passn + n);
                passn += n + target.Length;
                tmpstr = tmpstr.Substring(n + target.Length);
                n = tmpstr.IndexOf(target);
            }
            return res;
        }

        #endregion

        #region 文件读取与存储


        /// <summary>
        /// 子线程读入文本文件
        /// </summary>
        private void workReadText()
        {
            string preContent = "";
            WordCut wc = null;
            for (int i = 0; i < inputFileName.Length; i++)
            {
                string thisFileName = inputFileName[i];
                print("开始读取第" + i + "个文件：" + thisFileName);
                try
                {
                    preContent = IOTools.TxtIOController.readTxtFile(thisFileName);
                    print("读取完毕。正在预处理，请等待。");

                    wc = new WordCut(this.dc, preContent, printEvent);

                    //分割文本
                    wc.workCutSentence();

                    //分词
                    wc.workCutString();

                    

                    //生成文件信息
                    FileInfo finfo = new FileInfo();
                    finfo.fileName = thisFileName.Substring(thisFileName.LastIndexOf('\\') + 1, thisFileName.LastIndexOf('.') - thisFileName.LastIndexOf('\\') - 1);
                    finfo.filePath = thisFileName;
                    finfo.summary = preContent.Substring(0, preContent.Length <= 100 ? preContent.Length : 100);
                    finfo.characterNum = preContent.Length;
                    finfo.paragraphNum = dc.preResult.Count;
                    finfo.sentenceNum = dc.sentences.Count;
                    finfo.sentences = dc.sentences;

                    dc.fileinfo.Add(finfo);
                    setFileInfoList();
                    
                }
                catch (Exception ex)
                {
                    print(ex.Message);
                }
            }
            //修正分词结果
            //workResetWordCutwc.workResetWordCut();

            //句子排序
            wc.workSortSentences();

            setControllersStatus(Status.readed);
            setFileInfoList();
            setWordsInfo();

            print("预处理完毕。");
        }


        /// <summary>
        /// 子线程读入Json文件
        /// </summary>
        private void workReadJson()
        {
            string fileName = openFileDialog2.FileName;
            print("开始读取文件：" + fileName);
            try
            {
                dc.fileinfo = IOTools.TxtIOController.getFileInfoFromJson(fileName);
                setFileInfoList();
                print("文件读取完毕");
                setControllersStatus(Status.readed);
            }
            catch (Exception ex)
            {
                print(ex.Message);
            }
        }

        /// <summary>
        /// 子线程函数保存全部信息为json
        /// </summary>
        private void workSaveFileAsJson()
        {
            try
            {
                print("正在保存分词结果");
                string fileName=saveFileDialog1.FileName.ToString();
                IOTools.TxtIOController.saveFileAsJson(fileName, dc.fileinfo);
                print("分词结果保存完毕。文件路径：" + fileName);
            }
            catch (Exception ex)
            {
                print(ex.Message);
            }
        }

        #endregion

        #region 名词分析

        //private void 

        /// <summary>
        /// 子线程分析名词
        /// </summary>
        private void workAnalyzeNword()
        {
            print("开始分析名词");
            ana.initAnalysis();
            foreach (FileInfo file in dc.fileinfo)
            {
                foreach (Sentence sen in file.sentences)
                {
                    print("词频统计（第" + (dc.fileinfo.IndexOf(file) + 1) + "篇" + (file.sentences.ToList().IndexOf(sen) + 1) + "句)");
                    foreach (Pair w in sen.words)
                    {
                        if (w.Flag != "x")
                        {
                            ana.addWordRecord(w, sen);
                        }
                    }
                }
            }
            wordinfo = ana.getResult();
            print("分析结束");
            setControllersStatus(Status.anaedWord);
            setWordInfoList();
            
        }

        /// <summary>
        /// 更新词列表信息
        /// </summary>
        private void workUpdateAnalyzeInfo()
        {
            List<string> types = new List<string>();
            if (checkBox1.Checked)
            {
                types.Add("ns");
            }
            if (checkBox2.Checked)
            {
                types.Add("v");
                types.Add("vd");
                types.Add("vn");
            }
            if (checkBox3.Checked)
            {
                types.Add("nr");
            }
            if (checkBox4.Checked)
            {
                types.Add("n");
                types.Add("nt");
                types.Add("nz");
                types.Add("l");
            }
            wordinfo = ana.getWordsByType(types);
            setWordInfoList();
        }

        #endregion


        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否要关闭当前分析？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
             == DialogResult.OK)
            {
                Environment.Exit(0);
            }
        }

        private void 打开TXTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //textBox2.Text = "";
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            inputFileName = openFileDialog1.FileNames;
            setControllersStatus(Status.reading);
            new Thread(workReadText).Start();
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (listBox1.Items.Count > 0 && listBox1.SelectedIndex >= 0)
            {
                if (e.Button == MouseButtons.Left)
                {
                    setFileInfo(dc.fileinfo[listBox1.SelectedIndex]);
                }
                else if(e.Button==MouseButtons.Right)
                {
                    contextMenuStrip1.Show(MousePosition);
                }
            }
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            setFileInfo();
            dc.fileinfo.Remove(dc.fileinfo[listBox1.SelectedIndex]);
            setFileInfoList();
            setControllersStatus(Status.readed);
        }

        private void 打开JSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否要关闭当前分析？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.OK)
            {
                dc.fileinfo = null;
                openFileDialog2.ShowDialog();
            }
            
        }

        private void 保存为JSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog1.FileName = "文本分析结果.json";
            saveFileDialog1.ShowDialog();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            new Thread(workSaveFileAsJson).Start();
        }

        private void openFileDialog2_FileOk(object sender, CancelEventArgs e)
        {
            setControllersStatus(Status.init);
            new Thread(workReadJson).Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setControllersStatus(Status.anaingWord);
            new Thread(workAnalyzeNword).Start();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            workUpdateAnalyzeInfo();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            workUpdateAnalyzeInfo();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            workUpdateAnalyzeInfo();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            workUpdateAnalyzeInfo();
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            setSentensesInfo();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string keyword = textBox2.Text;
            wordinfo = ana.getWordsByName(keyword);
            setWordInfoList();
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string keyword = textBox2.Text;
                wordinfo = ana.getWordsByName(keyword);
                setWordInfoList();
            }
        }

        private void 开始分词ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void workChangeWords()
        {
            string text1 = textBox1.Text;
            string text2 = textBox3.Text;
            print("开始分析文本1");
            var res = WordCutTool.cut(text1);
            var verbs = WordCutTool.sumPair(res, "V");
            var nones = WordCutTool.sumPair(res, "N");
            var adjs = WordCutTool.sumPair(res, "A");

            print("开始分析文本2");
            var res2 = WordCutTool.cut(text2);
            var verbs2 = WordCutTool.sumPair(res2, "V");
            var nones2 = WordCutTool.sumPair(res2, "N");
            var adjs2 = WordCutTool.sumPair(res2, "A");

            List<WordPair[]> changeres = new List<WordPair[]>();
            for(int i = 0; i < verbs.Count; i++)
            {
                WordPair[] c = new WordPair[2];
                c[0] = verbs[i];
                if (verbs2.Count > i) c[1] = verbs2[i];
                else c[1] = c[0];
                changeres.Add(c);


            }
            for(int i = 0; i < nones.Count; i++)
            {
                WordPair[] c = new WordPair[2];
                c[0] = nones[i];
                if (nones2.Count > i) c[1] = nones2[i];
                else c[1] = c[0];
                changeres.Add(c);
            }
            for (int i = 0; i < adjs.Count; i++)
            {
                WordPair[] c = new WordPair[2];
                c[0] = adjs[i];
                if (adjs2.Count > i) c[1] = adjs2[i];
                else c[1] = c[0];
                changeres.Add(c);
            }

            List<WordPair> respair = new List<WordPair>();
            foreach(var w in res)
            {
                bool change = false;
                foreach(var wp in changeres)
                {
                    if(w.Flag==wp[0].Flag && w.Word == wp[0].Word)
                    {
                        respair.Add(new WordPair(wp[1].Word, wp[1].Flag));
                        change = true;
                        break;
                    }
                }
                if (!change) respair.Add(w);
            }
            printChangeResult(WordCutTool.getString(respair));
            //print();
            print("分析结束");
        }

        public void analysisTest1()
        {
            string text1 = textBox1.Text;
            var res = WordCutTool.cut(text1);

            List<WordPair> out1 = new List<WordPair>();
            foreach (var w in res)
            {
                //if (w.Flag.ToUpper().StartsWith("V")) out1.Add(new Pair("[ V " + w.Word + " ]", "v"));
                //else if (w.Flag.ToUpper().StartsWith("N")) out1.Add(new Pair("[ N " + w.Word + " ]", "n"));
                //else if (w.Flag.ToUpper().StartsWith("A")) out1.Add(new Pair("[ A " + w.Word + " ]", "a"));
                // else if (w.Flag.ToUpper().StartsWith("D")) out1.Add(new Pair("[D" + w.Word + "]", "d"));
                if (w.Flag == "x" && ("，。？！…—；,.?!()（）“”‘’\r\n\t ".Contains(w.Word) || w.Word.Length<=0) )
                {
                    out1.Add(new WordPair(" [" + w.Word + "/" + w.Flag + "]\r\n\r\n", ""));
                }
                else
                {
                    out1.Add(new WordPair(" [" + w.Word + "/" + w.Flag + "]", ""));
                }
                    
            }
            printChangeResult(WordCutTool.getString(out1));
        }

        public void analysisTest2()
        {
            string text1 = textBox1.Text;
            

            var res = WordCutTool.cut(text1);

            List<WordPair> out1 = new List<WordPair>();
            List<WordPair> verbs = WordCutTool.sumPair(res, "V");
            foreach (var v in verbs)
            {
                for (int i = 1; i < res.Count - 1; i++)
                {
                    if (res[i].Flag == v.Flag && res[i].Word == v.Word)
                    {
                        //verb
                        out1.Add(new WordPair(string.Format("{0}\t{1}\t{2}\r\n", res[i - 1].Flag, res[i].Word, res[i + 1].Flag), ""));
                    }
                }
            }


            printChangeResult(WordCutTool.getString(out1));
        }

        //public void analysisTest3()
        //{
        //    string path = @"D:\文档\人工智能\语料\聊天机器人词库\纯化\";
        //    string[] files = Directory.GetFiles(path);
        //    string outfile = path + @"result.txt";
        //    Dictionary<string, string> dir = new Dictionary<string, string>();
        //    foreach(string file in files)
        //    {
        //        string[] res = File.ReadAllLines(file,Encoding.Default);
        //        List<string> reslist = new List<string>();
        //        for(int i = 0; i < res.Length; i++)
        //        {

        //            if (!string.IsNullOrWhiteSpace(res[i]))
        //            {
        //                reslist.Add(res[i]);
        //            }
        //            else
        //            {
        //                if (reslist.Count >= 2)
        //                {
        //                    if (reslist[0].Contains("[") && reslist[0].Contains("]")) { reslist.Clear(); continue; }
        //                    reslist[0] = reslist[0].Replace("[name]", "你").Replace("[cqname]", "我").Replace("[enter]"," ");
        //                    reslist[1] = reslist[1].Replace("[name]", "你").Replace("[cqname]", "我").Replace("[enter]", " ");
        //                    reslist[1] = Regex.Replace(reslist[1], @"\[.*?\]", "");
        //                    if (dir.ContainsKey(reslist[0]))
        //                    {
        //                        //have it
        //                        if (reslist[0].Length > dir[reslist[0]].Length)
        //                        {
        //                            if (reslist[1].Contains("http")) { reslist.Clear(); continue; }
        //                            //change
        //                            dir[reslist[0]] = reslist[1];
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //dont have
        //                        dir[reslist[0]] = reslist[1];
        //                    }

        //                }
        //                reslist.Clear();

        //            }

        //        }
        //    }
        //    using(FileStream fs=new FileStream(outfile, FileMode.Create))
        //    {
        //        using(StreamWriter sw=new StreamWriter(fs,Encoding.UTF8))
        //        {
        //            foreach (var k in dir)
        //            {
        //                sw.WriteLine(string.Format("{0}\r\n{1}\r\n",k.Key,k.Value));
        //            }
        //        }
        //    }

        //}

        //private void analysisTest4()
        //{
        //    string file = @"D:\文档\人工智能\语料\聊天机器人词库\纯化\result.txt";
        //    //Dictionary<string, List<Pair>> dir = new Dictionary<string, List<Pair>>();
        //    string[] res = File.ReadAllLines(file, Encoding.UTF8);


        //    string resstr = "";
        //    for (int i = 0; i < res.Length; i++)
        //    {
        //        string key = res[i++];
        //        string value = res[i++];
        //        List<Pair> valueWords = WordCutTool.cut(value);

        //        resstr += string.Format("{0}\t{1}\t{2}\r\n", key, value, 1);
        //        foreach (var p in valueWords)
        //        {
        //            if (p.Flag.StartsWith("n") || p.Flag.StartsWith("v") || p.Flag.StartsWith("a"))
        //            {
        //                resstr += string.Format("{0}\t{1}\t{2}\r\n", value, p.Word, 2);
        //            }
        //        }
        //        //dir[key] = valueWords;
        //        if (i > 1000) break;
        //    }

        //    printChangeResult(resstr);

        //}

        private void analysis5()
        {
            var res=WordCutTool.cutSentence(textBox1.Text);
            string output = "";

            foreach(var sen in res)
            {
                foreach(var w in sen)
                {

                    if (w.Flag.ToUpper().StartsWith("N") || w.Flag.ToUpper().StartsWith("R")) output += "[" + w.Word + "]";
                    else if (w.Flag.ToUpper().StartsWith("V")) output += "" + w.Word + "";
                    else if (w.Flag.ToUpper().StartsWith("U")) ;
                    else output += w.Word + w.Flag;
                }
                output += "\r\n\r\n";
            }

            printChangeResult(output);
        }

        private void analysisTF()
        {
            string str = textBox1.Text;
            List<WordPair> words = AnalyzeTool.getTFWithSort(str, true);

            string output = "";
            foreach (var w in words)
            {
                output += string.Format("{0} {1} {2}\r\n", w.Word, w.Flag, w.Num);
            }
            printChangeResult(output);

        }

        /// <summary>
        /// 文档词频预处理，得到一个文件 WordDFs.txt
        /// </summary>
        private void analysisDF()
        {
            string path = AnalyzeTool.dataDic;
            string[] files = Directory.GetFiles(path, "*.txt");
            for (int i = 0; i < files.Length; i++)
            {
                string f = files[i];
                var dfs = AnalyzeTool.loadDF();
                dfs = AnalyzeTool.getIDF(dfs, new string[] { f });
                AnalyzeTool.saveDF(dfs);
                print(string.Format("完成：{0}({1}/{2})", f, i + 1, files.Length));

                string tpath = "./backup/";
                if (!Directory.Exists(tpath)) Directory.CreateDirectory(tpath);
                File.Move(f, tpath + Path.GetFileName(f));
            }
            print(string.Format("over.({0})",files.Length));
        }

        private void analysisTFIDF()
        {
            string content = textBox1.Text;
            var dfs = AnalyzeTool.loadDF();
            var words=AnalyzeTool.getTFIDF(content, dfs);
            string output = "";
            foreach(var w in words)
            {
                output += string.Format("{0}-{1}\r\n", w.Word, w.Num);
            }
            printChangeResult(output);
        }

        private void addtoForbiddenWord()
        {
            string[] input = textBox1.Text.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string filename = "ForbiddenWords.txt";
            if (!File.Exists(filename)) File.Create(filename);
            string[] oldwords = File.ReadAllLines(filename, Encoding.UTF8);
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            foreach (var v in oldwords) dic[v.Trim()] = true;
            foreach (var v2 in input) dic[v2.Trim()] = true;
            string output = "";
            foreach (var k in dic.Keys) output += k + "\r\n";
            File.WriteAllText(filename, output);
            printChangeResult(DateTime.Now.ToShortDateString());
        }

        private void formatinfos()
        {
            string source = @"D:\CProjects\WebsiteGetter\WebsiteGetter\bin\Debug\tmp\20171027.txt";
            Dictionary<string, string> data = new Dictionary<string, string>();
            string[] datas = File.ReadAllLines(source,Encoding.Default);
            for(int i = 0; i < datas.Length; i += 5)
            {
                string name = datas[i];
                string email = datas[i + 1];
                if(!string.IsNullOrWhiteSpace(email)) data[email] = name;
            }
            string output = "";
            foreach(var d in data)
            {
                output += d.Value + "," + d.Key + "\r\n";
            }
            printChangeResult(output);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new Thread(workChangeWords).Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
           

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.A)
            {
                textBox1.SelectAll();
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.A)
            {
                textBox3.SelectAll();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            new Thread(analysisDF).Start();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            new Thread(analysisTFIDF).Start();
        }
    }
}
