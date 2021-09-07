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
        public static List<int> OpenCSV(string filePath)
        {
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            StreamReader sr = new StreamReader(fs, Encoding.UTF8);
            //记录每次读取的一行记录
            string strLine = "";
            string[] tableHead = null;
            
            //标示列数
            int columnCount = 0;
            //标示是否是读取的第一行
            bool      IsFirst = true;
            List<int> ids     = new List<int> ();
            int       index   = 0;
            //逐行读取CSV中的数据
            while ((strLine = sr.ReadLine()) != null)
            {
                tableHead = strLine.Split(',');
                if (IsFirst)
                {
                    //用第一行的第一列或者第二列来确定用哪一列的ID
                    IsFirst = false;
                    int id;
                    if(int.TryParse (tableHead[index],out id))
                    {
                        ids.Add (id);
                    }
                    else if(int.TryParse (tableHead[++index],out id))
                    {
                        ids.Add (id);
                    }
                    else
                    {
                        return ids;
                    }
                }
                else
                {
                    int id;
                    if(int.TryParse (tableHead[index],out id))
                    {
                        ids.Add (id);
                    }
                }
            }

            sr.Close();
            fs.Close();
            return ids;
        }
    }
}