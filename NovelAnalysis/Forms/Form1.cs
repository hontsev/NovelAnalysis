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
            ana = new WordAnalysis(this);

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

        /// <summary>
        /// 显示包含该词的句子信息
        /// </summary>
        private void setSentensesInfo()
        {
            if (listView1.SelectedItems.Count >= 1)
            {
                var item = listView1.SelectedItems[0];
                string wordStr = item.Text.ToString();
                List<string> res = ana.getItemStrings(wordStr);
                richTextBox1.Text = "";
                foreach (var s in res)
                {
                    Regex r = new Regex(wordStr);
                    var begins = getIndexes(s, wordStr);
                    for (int i = 0; i < s.Length; i++)
                    {
                        if (begins.IndexOf(i) >= 0)
                        {
                            richTextBox1.SelectionStart = richTextBox1.TextLength;
                            richTextBox1.SelectionLength = 0;
                            richTextBox1.SelectionColor = Color.Red;
                            for (int j = 0; j < wordStr.Length; j++)
                            {
                                richTextBox1.AppendText(s[i].ToString());
                                i++;
                            }
                            i--;
                        }
                        else
                        {
                            richTextBox1.SelectionColor = richTextBox1.ForeColor;
                            richTextBox1.AppendText(s[i].ToString());
                        }
                    }

                    richTextBox1.SelectionColor = richTextBox1.ForeColor;
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
                    print("读取完毕。");

                    wc = new WordCut(this, preContent);

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
                    finfo.sentneces = dc.sentences.ToArray();

                    dc.fileinfo.Add(finfo);
                    setFileInfoList();
                }
                catch (Exception ex)
                {
                    print(ex.Message);
                }
            }
            //修正分词结果
            wc.workResetWordCut();
            setControllersStatus(Status.readed);
            setFileInfoList();
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
                foreach (Sentence sen in file.sentneces)
                {
                    print("词频统计（第" + (dc.fileinfo.IndexOf(file) + 1) + "篇" + (file.sentneces.ToList().IndexOf(sen) + 1) + "句)");
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
    }
}
