using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DecryptBackup.SQLiteHelper
{
    /// <summary>
    /// 数据库操作抽象类。
    /// </summary>
    public abstract class DBOperate
    {
        /// <summary>
        /// 数据库连接对象。
        /// </summary>
        protected DbConnection conn;

        /// <summary>
        /// 事务处理对象。
        /// </summary> 
        private DbTransaction trans;

        /// <summary>
        /// 指示当前操作是否在事务中。
        /// </summary>
        private bool bInTrans = false;

        #region 打开关闭数据库连接

        /// <summary>
        /// 打开数据库连接
        /// </summary> 
        public bool Open()
        {
            try
            {
                if (conn.State.Equals(ConnectionState.Closed))
                {
                    conn.Open();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public bool Close()
        {
            try
            {
                if (conn.State.Equals(ConnectionState.Open))
                {
                    conn.Close();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region 事务支持

        /// <summary>
        /// 开始一个事务
        /// </summary>
        public void BeginTran()
        {
            if (!this.bInTrans)
            {
                this.Open();
                trans = conn.BeginTransaction();
                bInTrans = true;
            }
        }

        /// <summary>
        /// 提交一个事务
        /// </summary>
        public void CommitTran()
        {
            if (this.bInTrans)
            {
                trans.Commit();
                bInTrans = false;
                this.Close();
            }
        }

        /// <summary>
        /// 回滚一个事务
        /// </summary>
        /// <author>天志</author>
        /// <log date="2007-04-05">创建</log>
        public void RollBackTran()
        {
            if (this.bInTrans)
            {
                trans.Rollback();
                bInTrans = false;
                this.Close();
            }
        }

        #endregion

        #region 执行无返回值SQL语句

        /// <summary>
        /// 执行添加，修改，删除之类的操作。
        /// </summary>
        /// <param name="strSql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>受影响的条数</returns>
        public int ExecuteNonQuery(string sql, List<IDataParameter> parameters)
        {
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // 打开数据库连接
                this.Open();
                return cmdSql.ExecuteNonQuery();
            }
            finally
            {
                // 如果不在事务中
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        #endregion

        #region 返回单个值

        /// <summary>
        /// 返回结果集中第一行的第一列。
        /// </summary>
        /// <param name="sql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>返回对象</returns>
        public object ExecuteScalar(string sql, List<IDataParameter> parameters)
        {
            //初始化一个command对象
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                //判断是否在事务中
                if (this.bInTrans)
                {
                    cmdSql.Transaction = this.trans;
                }

                // 打开数据库连接
                this.Open();
                return cmdSql.ExecuteScalar();
            }
            finally
            {
                //判断是否在事务中
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region 返回DataReader

        /// <summary>
        /// 返回DataReader。
        /// </summary>
        /// <param name="sql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>DataReader对象</returns>
        public IDataReader ExecuteReader(string sql, List<IDataParameter> parameters)
        {
            //初始化一个command对象
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // 打开数据库连接
                this.Open();

                //返回DataReader对象
                return cmdSql.ExecuteReader(CommandBehavior.CloseConnection);
            }
            finally
            {
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region 返回DataTable

        /// <summary>
        /// 返回DataTable。
        /// </summary>
        /// <param name="sql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>DataTable对象</returns>
        public DataTable ExecuteDataTable(string sql, List<IDataParameter> parameters)
        {
            //初始化一个DataTable对象，一个DataAdapter对象
            DataTable dt = new DataTable();
            DbDataAdapter da = this.CreateDataAdapter(sql);

            //初始化一个command对象
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                //返回DataTable对象
                da.SelectCommand = cmdSql;

                // 打开数据库连接
                this.Open();
                da.Fill(dt);
                return dt;
            }
            finally
            {
                //判断是否在事务中
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region 返回DataSet
        public void ClearDataSet()
        {
            ds.Clear();
        }

        /// <summary>
        /// 返回DataSet对象。
        /// </summary>
        /// <param name="sql">sql语句名称</param>
        /// <param name="tableName">操作表的名称</param>
        /// <returns>DataSet对象</returns> 
        public DataSet ExecuteDataSet(string sql, string tableName)
        {
            return this.ExecuteDataSet(sql, null, tableName);
        }

        public DataSet ExecuteDataSet(string sql, string tableName, DataSet target)
        {
            return this.ExecuteDataSet(sql, null, tableName, target);
        }

        private DataSet ExecuteDataSet(string sql, List<IDataParameter> parameters, string tableName, DataSet target)
        {
            if (target == null) target = new DataSet();
            //初始化一个DataSet对象，一个DataAdapter对象
            DbDataAdapter da = this.CreateDataAdapter(sql);
            //初始化一个command对象
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // 返回DataSet对象
                da.SelectCommand = cmdSql;
                // 打开数据库连接
                this.Open();
                da.Fill(target, tableName);
                return target;
            }
            finally
            {
                //判断是否在事务中
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        DataSet ds = new DataSet();
        /// <summary>
        /// 返回DataSet对象。
        /// </summary>
        /// <param name="sql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="TableName">操作表的名称</param>
        /// <returns>DataSet对象</returns>
        public DataSet ExecuteDataSet(string sql, List<IDataParameter> parameters, string tableName)
        {
            //初始化一个DataSet对象，一个DataAdapter对象
            DbDataAdapter da = this.CreateDataAdapter(sql);
            //初始化一个command对象
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // 返回DataSet对象
                da.SelectCommand = cmdSql;
                // 打开数据库连接
                this.Open();
                da.Fill(ds, tableName);
                return ds;
            }
            finally
            {
                //判断是否在事务中
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        #endregion

        #region 生成命令对象

        /// <summary>
        /// 获取一个DbCommand对象。
        /// </summary>
        /// <param name="strSql">sql语句名称</param>
        /// <param name="parameters">参数数组</param>
        /// <param name="strCommandType">命令类型</param>
        /// <returns>OdbcCommand对象</returns>
        private DbCommand GetPreCommand(string sql, List<IDataParameter> parameters)
        {
            // 初始化一个command对象
            DbCommand cmdSql = conn.CreateCommand();
            cmdSql.CommandText = sql;
            cmdSql.CommandType = this.GetCommandType(sql);

            // 判断是否在事务中
            if (this.bInTrans)
            {
                cmdSql.Transaction = this.trans;
            }

            if (parameters != null)
            {
                //指定各个参数的取值
                foreach (IDataParameter sqlParm in parameters)
                {
                    cmdSql.Parameters.Add(sqlParm);
                }
            }
            return cmdSql;

        }

        /// <summary>
        /// 取得SQL语句的命令类型。
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns>命令类型</returns> 
        private CommandType GetCommandType(string sql)
        {
            //记录SQL语句的开始字符
            if (sql.Length > 7)
            {
                //取出字符串的前位
                string topText = sql.Substring(0, 7).ToUpper();

                // 如果不是存储过程
                if (topText.Equals("INSERT ") || topText.Equals("DELETE ") || topText.Equals("UPDATE ") || topText.Equals("SELECT ") ||//记录的增删改查
                    topText.Equals("CREATE ") || topText.Equals("DROP TA") || topText.Equals("ALTER T") || topText.Equals("BACKUP ") ||//表的操作
                    topText.Equals("RESTORE") || topText.Equals("DROP DA"))//数据库的操作
                {
                    return CommandType.Text;
                }
            }
            return CommandType.StoredProcedure;

        }

        #endregion

        /// <summary>
        /// 创建适配器(无参)
        /// </summary>
        /// <returns></returns>
        protected abstract DbDataAdapter CreateDataAdapter();

        /// <summary>
        /// 创建适配器
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected abstract DbDataAdapter CreateDataAdapter(string sql);
    }

}
