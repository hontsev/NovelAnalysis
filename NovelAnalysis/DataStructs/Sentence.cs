using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiebaNet.Segmenter.PosSeg;

namespace NovelAnalysis
{
    public class Sentence
    {
        public int paragraphNumber;
        public int sentenceNumber;
        public IEnumerable<JiebaNet.Segmenter.PosSeg.Pair> words;
        public Sentence(int paragraph, int sentence, IEnumerable<Pair> words)
        {
            paragraphNumber = paragraph;
            sentenceNumber = sentence;
            this.words = words;
        }
    }
}
