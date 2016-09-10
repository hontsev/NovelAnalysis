using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Mozilla.NUniversalCharDet;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace NovelAnalysis.IOTools
{
    public class TxtIOController
    {
        /// <summary>
        /// 返回流的编码格式
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static Encoding getEncoding(string streamName)
        {
            Encoding encoding = Encoding.Default;
            using (Stream stream = new FileStream(streamName, FileMode.Open))
            {
                MemoryStream msTemp = new MemoryStream();
                int len = 0;
                byte[] buff = new byte[512];
                while ((len = stream.Read(buff, 0, 512)) > 0)
                {
                    msTemp.Write(buff, 0, len);
                }
                if (msTemp.Length > 0)
                {
                    msTemp.Seek(0, SeekOrigin.Begin);
                    byte[] PageBytes = new byte[msTemp.Length];
                    msTemp.Read(PageBytes, 0, PageBytes.Length);
                    msTemp.Seek(0, SeekOrigin.Begin);
                    int DetLen = 0;
                    UniversalDetector Det = new UniversalDetector(null);
                    byte[] DetectBuff = new byte[4096];
                    while ((DetLen = msTemp.Read(DetectBuff, 0, DetectBuff.Length)) > 0 && !Det.IsDone())
                    {
                        Det.HandleData(DetectBuff, 0, DetectBuff.Length);
                    }
                    Det.DataEnd();
                    if (Det.GetDetectedCharset() != null)
                    {
                        encoding = Encoding.GetEncoding(Det.GetDetectedCharset());
                    }
                }
                msTemp.Close();
                msTemp.Dispose();
                return encoding;
            }
        }

        /// <summary>
        /// 读取单个txt文件内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string readTxtFile(string fileName)
        {
            Encoding encoding = Encoding.GetEncoding(getEncoding(fileName).BodyName);
            //Thread.Sleep(1000);
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                StreamReader reader = new StreamReader(file, encoding);
                string preContent = reader.ReadToEnd();
                reader.Dispose();
                return preContent;
            }
        }

        /// <summary>
        /// 解析JSON数组生成对象实体集合
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
        /// <returns>对象实体集合</returns>
        public static List<T> DeserializeJsonToList<T>(string json) where T : class
        {
            JsonSerializer serializer = new JsonSerializer();
            StringReader sr = new StringReader(json);
            object o = serializer.Deserialize(new JsonTextReader(sr), typeof(List<T>));
            List<T> list = o as List<T>;
            return list;
        }

        public static List<FileInfo> getFileInfoFromJson(string fileName)
        {
            List<FileInfo> fi = new List<FileInfo>();
            Encoding encoding = getEncoding(fileName);
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                StreamReader reader = new StreamReader(file, encoding);
                var fileContent = DeserializeJsonToList<FileInfo>(reader.ReadToEnd());
                foreach (var finfo in fileContent)
                {
                    fi.Add(finfo);
                }
                reader.Dispose();
            }
            return fi;
        }

        public static void saveFileAsJson(string fileName,List<FileInfo> fileinfo)
        {
            string saveJsonString = JsonConvert.SerializeObject(fileinfo);
            using (FileStream file = new FileStream(fileName, FileMode.CreateNew))
            {
                StreamWriter writer = new StreamWriter(file, Encoding.UTF8);
                writer.Write(saveJsonString);
                writer.Close();
            }
        }

        
        
    }
}
