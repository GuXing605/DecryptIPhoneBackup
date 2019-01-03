using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DecryptBackup.SQLiteHelper
{
    /// <summary>
    /// ���ݿ���������ࡣ
    /// </summary>
    public abstract class DBOperate
    {
        /// <summary>
        /// ���ݿ����Ӷ���
        /// </summary>
        protected DbConnection conn;

        /// <summary>
        /// ���������
        /// </summary> 
        private DbTransaction trans;

        /// <summary>
        /// ָʾ��ǰ�����Ƿ��������С�
        /// </summary>
        private bool bInTrans = false;

        #region �򿪹ر����ݿ�����

        /// <summary>
        /// �����ݿ�����
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
        /// �ر����ݿ�����
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

        #region ����֧��

        /// <summary>
        /// ��ʼһ������
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
        /// �ύһ������
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
        /// �ع�һ������
        /// </summary>
        /// <author>��־</author>
        /// <log date="2007-04-05">����</log>
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

        #region ִ���޷���ֵSQL���

        /// <summary>
        /// ִ����ӣ��޸ģ�ɾ��֮��Ĳ�����
        /// </summary>
        /// <param name="strSql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <returns>��Ӱ�������</returns>
        public int ExecuteNonQuery(string sql, List<IDataParameter> parameters)
        {
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // �����ݿ�����
                this.Open();
                return cmdSql.ExecuteNonQuery();
            }
            finally
            {
                // �������������
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        #endregion

        #region ���ص���ֵ

        /// <summary>
        /// ���ؽ�����е�һ�еĵ�һ�С�
        /// </summary>
        /// <param name="sql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <returns>���ض���</returns>
        public object ExecuteScalar(string sql, List<IDataParameter> parameters)
        {
            //��ʼ��һ��command����
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                //�ж��Ƿ���������
                if (this.bInTrans)
                {
                    cmdSql.Transaction = this.trans;
                }

                // �����ݿ�����
                this.Open();
                return cmdSql.ExecuteScalar();
            }
            finally
            {
                //�ж��Ƿ���������
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region ����DataReader

        /// <summary>
        /// ����DataReader��
        /// </summary>
        /// <param name="sql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <returns>DataReader����</returns>
        public IDataReader ExecuteReader(string sql, List<IDataParameter> parameters)
        {
            //��ʼ��һ��command����
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // �����ݿ�����
                this.Open();

                //����DataReader����
                return cmdSql.ExecuteReader(CommandBehavior.CloseConnection);
            }
            finally
            {
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region ����DataTable

        /// <summary>
        /// ����DataTable��
        /// </summary>
        /// <param name="sql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <returns>DataTable����</returns>
        public DataTable ExecuteDataTable(string sql, List<IDataParameter> parameters)
        {
            //��ʼ��һ��DataTable����һ��DataAdapter����
            DataTable dt = new DataTable();
            DbDataAdapter da = this.CreateDataAdapter(sql);

            //��ʼ��һ��command����
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                //����DataTable����
                da.SelectCommand = cmdSql;

                // �����ݿ�����
                this.Open();
                da.Fill(dt);
                return dt;
            }
            finally
            {
                //�ж��Ƿ���������
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }

        }

        #endregion

        #region ����DataSet
        public void ClearDataSet()
        {
            ds.Clear();
        }

        /// <summary>
        /// ����DataSet����
        /// </summary>
        /// <param name="sql">sql�������</param>
        /// <param name="tableName">�����������</param>
        /// <returns>DataSet����</returns> 
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
            //��ʼ��һ��DataSet����һ��DataAdapter����
            DbDataAdapter da = this.CreateDataAdapter(sql);
            //��ʼ��һ��command����
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // ����DataSet����
                da.SelectCommand = cmdSql;
                // �����ݿ�����
                this.Open();
                da.Fill(target, tableName);
                return target;
            }
            finally
            {
                //�ж��Ƿ���������
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        DataSet ds = new DataSet();
        /// <summary>
        /// ����DataSet����
        /// </summary>
        /// <param name="sql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <param name="TableName">�����������</param>
        /// <returns>DataSet����</returns>
        public DataSet ExecuteDataSet(string sql, List<IDataParameter> parameters, string tableName)
        {
            //��ʼ��һ��DataSet����һ��DataAdapter����
            DbDataAdapter da = this.CreateDataAdapter(sql);
            //��ʼ��һ��command����
            DbCommand cmdSql = this.GetPreCommand(sql, parameters);
            try
            {
                // ����DataSet����
                da.SelectCommand = cmdSql;
                // �����ݿ�����
                this.Open();
                da.Fill(ds, tableName);
                return ds;
            }
            finally
            {
                //�ж��Ƿ���������
                if (!this.bInTrans)
                {
                    this.Close();
                }
                cmdSql.Parameters.Clear();
            }
        }

        #endregion

        #region �����������

        /// <summary>
        /// ��ȡһ��DbCommand����
        /// </summary>
        /// <param name="strSql">sql�������</param>
        /// <param name="parameters">��������</param>
        /// <param name="strCommandType">��������</param>
        /// <returns>OdbcCommand����</returns>
        private DbCommand GetPreCommand(string sql, List<IDataParameter> parameters)
        {
            // ��ʼ��һ��command����
            DbCommand cmdSql = conn.CreateCommand();
            cmdSql.CommandText = sql;
            cmdSql.CommandType = this.GetCommandType(sql);

            // �ж��Ƿ���������
            if (this.bInTrans)
            {
                cmdSql.Transaction = this.trans;
            }

            if (parameters != null)
            {
                //ָ������������ȡֵ
                foreach (IDataParameter sqlParm in parameters)
                {
                    cmdSql.Parameters.Add(sqlParm);
                }
            }
            return cmdSql;

        }

        /// <summary>
        /// ȡ��SQL�����������͡�
        /// </summary>
        /// <param name="sql">SQL���</param>
        /// <returns>��������</returns> 
        private CommandType GetCommandType(string sql)
        {
            //��¼SQL���Ŀ�ʼ�ַ�
            if (sql.Length > 7)
            {
                //ȡ���ַ�����ǰλ
                string topText = sql.Substring(0, 7).ToUpper();

                // ������Ǵ洢����
                if (topText.Equals("INSERT ") || topText.Equals("DELETE ") || topText.Equals("UPDATE ") || topText.Equals("SELECT ") ||//��¼����ɾ�Ĳ�
                    topText.Equals("CREATE ") || topText.Equals("DROP TA") || topText.Equals("ALTER T") || topText.Equals("BACKUP ") ||//��Ĳ���
                    topText.Equals("RESTORE") || topText.Equals("DROP DA"))//���ݿ�Ĳ���
                {
                    return CommandType.Text;
                }
            }
            return CommandType.StoredProcedure;

        }

        #endregion

        /// <summary>
        /// ����������(�޲�)
        /// </summary>
        /// <returns></returns>
        protected abstract DbDataAdapter CreateDataAdapter();

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected abstract DbDataAdapter CreateDataAdapter(string sql);
    }

}
