using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovelAnalysis
{
    public class WordInfo : IComparable
    {
        public string word;
        public string wordType;
        public int sum;
        public List<Sentence> appearSentences;


        #region 实现比较接口的CompareTo方法
        public int CompareTo(object obj)
        {
            //这里是按sum倒序
            int res = 0;
            try
            {
                WordInfo sObj = (WordInfo)obj;
                if (this.sum > sObj.sum)
                {
                    res = -1;
                }
                else if (this.sum < sObj.sum)
                {
                    res = 1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("比较异常", ex.InnerException);
            }
            return res;
        }
        #endregion
    }
}
