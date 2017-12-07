using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBF_Work
{
    public class DBFManager : IDBFManager
    {
        private OdbcConnection _connection = null;

        public DBFManager()
        {
            try
            {
                _connection = new OdbcConnection();
                _connection.ConnectionString = @"Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;Exclusive=No;Collate=Machine;NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        /// <summary>
        /// Выполнение запроса базы DBF
        /// </summary>
        public DataTable Execute (string command)
        {
            DataTable dt = null;
            if (_connection != null)
            {
                try
                {
                    _connection.Open();
                    dt = new DataTable();
                    OdbcCommand oCmd = _connection.CreateCommand();
                    oCmd.CommandText = command;
                    dt.Load(oCmd.ExecuteReader());
                    _connection.Close();
                }
                catch (Exception e)
                {
                    if (_connection.State == ConnectionState.Open) _connection.Close();
                    _connection.Dispose();
                    throw new Exception("Запрос:\n" + command + "\n" + e.Message);
                }
            }
            return dt;
        }
    }
}
