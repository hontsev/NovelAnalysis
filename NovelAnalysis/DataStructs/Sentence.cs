using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JiebaNet.Segmenter.PosSeg;

namespace NovelAnalysis
{
    public class Sentence : IComparable
    {
        public int paragraphNumber;
        public int sentenceNumber;
        public List<Pair> words;

        public Sentence(int paragraph, int sentence, IEnumerable<Pair> words)
        {
            paragraphNumber = paragraph;
            sentenceNumber = sentence;
            this.words = new List<Pair>(words.ToArray());
        }

        public int CompareTo(object obj)
        {
            int res = 0;
            try
            {
                Sentence sObj = (Sentence)obj;
                if (
                    this.paragraphNumber > sObj.paragraphNumber 
                    || 
                    (this.paragraphNumber == sObj.paragraphNumber && this.sentenceNumber > sObj.sentenceNumber)
                    )
                {
                    res = 1;
                }
                else if (this.paragraphNumber < sObj.paragraphNumber
                    ||
                    (this.paragraphNumber == sObj.paragraphNumber && this.sentenceNumber < sObj.sentenceNumber))
                {
                    res = -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("比较异常", ex.InnerException);
            }
            return res;
        }
    }
}
