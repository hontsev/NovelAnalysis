using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovelAnalysis
{
    public class FileInfo
    {
        public string fileName;
        public string filePath;
        public int characterNum;
        public int paragraphNum;
        public int sentenceNum;
        public string summary;
        public List<Sentence> sentences;
    }
}
