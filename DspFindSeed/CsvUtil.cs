using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using DysonSphereProgramSeed.Algorithms;

namespace DspFindSeed
{
    public class CsvUtil
    {
        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static bool OpenCSV(string filePath, out List<int> starIDs, out List<int> starCounts)
        {
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            //记录每次读取的一行记录
            string strLine = "";
            string[] tableHead = null;
            sr.ReadLine();//先读一次表头
            starIDs    = new List<int> ();
            starCounts = new List<int> ();
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                tableHead = strLine.Split(',');
                if(int.TryParse (tableHead[0],out var id) && int.TryParse (tableHead[1],out var starCount))
                {
                    starIDs.Add (id);
                    starCounts.Add (starCount);
                }
            }

            sr.Close();
            fs.Close();
            if (starIDs.Count == 0)
                return false;
            return true;
        }
    }
}