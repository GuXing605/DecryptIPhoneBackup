using System.Data.Common;
using System.Data.SQLite;

namespace DecryptBackup.SQLiteHelper
{
    public class SQLiteOperate : DBOperate
    {
        public SQLiteOperate(string sqlFile)
        {
            SQLiteConnectionStringBuilder connectString = new SQLiteConnectionStringBuilder();
            connectString.DataSource = sqlFile;
            connectString.FailIfMissing = true;
            connectString.Pooling = false;
            connectString.Add("Max Pool Size", 5);
            connectString.JournalMode = SQLiteJournalModeEnum.Wal;
            conn = new SQLiteConnection(connectString.ToString());
        }

        protected override DbDataAdapter CreateDataAdapter()
        {
            SQLiteCommand cmd = conn.CreateCommand() as SQLiteCommand;
            cmd.CommandType = System.Data.CommandType.Text;
            return new SQLiteDataAdapter(cmd);
        }

        protected override DbDataAdapter CreateDataAdapter(string sql)
        {
            return new SQLiteDataAdapter(sql, (SQLiteConnection)conn);
        }
    }
}
