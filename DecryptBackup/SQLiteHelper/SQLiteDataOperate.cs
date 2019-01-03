using System;
using System.Data;
using System.Diagnostics;

namespace DecryptBackup.SQLiteHelper
{
    /// <summary>数据库操作类</summary>
    public class SQLiteDataOperate
    {
        /// <summary>判断对象是否为Null</summary>
        /// <param name="obj">判断的对象</param>
        /// <returns>为空则返回空值，不为空则直接返回自身</returns>
        private static object IsNull(object obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            return obj;
        }

        /// <summary>生成一个GUID</summary>
        /// <returns>GUID值</returns>
        public static Guid NewID()
        {
            return Guid.NewGuid();
        }

        public static DataTable ReadFiles(string filePath)
        {
            DataTable result = default(DataTable);
            SQLiteOperate sqlite = new SQLiteOperate(filePath);
            try
            {
                string sqlSelect =
                    @"SELECT fileID, domain, relativePath, file FROM Files ORDER BY relativePath";
                result = sqlite.ExecuteDataTable(sqlSelect, null);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"查询数据库失败，错误信息:{ex.Message}");
            }
            sqlite.Close();
            sqlite = null;
            return result;
        }
    }
}
