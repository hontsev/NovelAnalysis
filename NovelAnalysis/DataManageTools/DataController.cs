using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovelAnalysis
{
    public class DataController
    {
        public string preContent { get; set; }
        public List<List<string>> preResult { get; set; }
        public List<Sentence> sentences { get; set; }
        public List<FileInfo> fileinfo { get; set; }
        public List<WordInfo> wordinfo { get; set; }

        public DataController()
        {
            preContent = "";
            preResult = new List<List<string>>();
            sentences = new List<Sentence>();
            fileinfo = new List<FileInfo>();
            wordinfo = new List<WordInfo>();
        }
    }
}
