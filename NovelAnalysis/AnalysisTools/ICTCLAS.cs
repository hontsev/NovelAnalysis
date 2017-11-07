using System;
using System.Runtime.InteropServices;

namespace NovelAnalysis
{
    [StructLayout(LayoutKind.Sequential)]
    public struct result_t
    {
        public int start; //start position,词语在输入句子中的开始位置
        public int length; //length,词语的长度
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public char[] sPOS;//word type，词性ID值，可以快速的获取词性表
        public int iPOS;//词性标注的编号
        public int word_ID; //该词的内部ID号，如果是未登录词，设成0或者-1
        public int word_type; //区分用户词典;1，是用户词典中的词；0，非用户词典中的词
        public int weight;//word weight,read weight
    };

    public class ICTCLAS
    {
        public const string path = "NLPIR.dll";
        public const string RootDir = "";
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Init")]
        public static extern bool NLPIR_Init(String sInitDirPath, int encoding, String sLicenseCode);
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcess")]
        public static extern IntPtr NLPIR_ParagraphProcess(String sParagraph, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_Exit")]
        public static extern bool NLPIR_Exit();

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ImportUserDict")]
        public static extern int NLPIR_ImportUserDict(String sFilename);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcess")]
        public static extern bool NLPIR_FileProcess(String sSrcFilename, String sDestFilename, int bPOStagged = 1);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_FileProcessEx")]
        public static extern bool NLPIR_FileProcessEx(String sSrcFilename, String sDestFilename);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetParagraphProcessAWordCount")]
        public static extern int NLPIR_GetParagraphProcessAWordCount(String sParagraph);
        //NLPIR_GetParagraphProcessAWordCount
        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_ParagraphProcessAW")]
        public static extern void NLPIR_ParagraphProcessAW(int nCount, [Out, MarshalAs(UnmanagedType.LPArray)] result_t[] result);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_AddUserWord")]
        public static extern int NLPIR_AddUserWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_SaveTheUsrDic")]
        public static extern int NLPIR_SaveTheUsrDic();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_DelUsrWord")]
        public static extern int NLPIR_DelUsrWord(String sWord);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Start")]
        public static extern bool NLPIR_NWI_Start();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Complete")]
        public static extern bool NLPIR_NWI_Complete();

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_AddFile")]
        public static extern bool NLPIR_NWI_AddFile(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_AddMem")]
        public static extern bool NLPIR_NWI_AddMem(String sText);

        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_NWI_GetResult")]
        public static extern IntPtr NLPIR_NWI_GetResult(bool bWeightOut = false);

        [DllImport(path, CharSet = CharSet.Ansi, EntryPoint = "NLPIR_NWI_Result2UserDict")]
        public static extern uint NLPIR_NWI_Result2UserDict();


        [DllImport(path, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "NLPIR_GetFileKeyWords")]
        public static extern IntPtr NLPIR_GetFileKeyWords(String sFilename, int nMaxKeyLimit = 50, bool bWeightOut = false);


    }
}
