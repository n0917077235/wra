using System.Data.SqlClient;
using System.Data;
using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace SQLHelper
{
    public class SQLHelper
    {
        private SqlConnection conn = null;
        private SqlCommand cmd = null;
        private SqlDataReader sdr = null;

        public SQLHelper(string connStr)
        {
            //string connStr = System.Configuration.ConfigurationManager.ConnectionStrings[ConnectionString].ConnectionString;
            // string connStr = "";
            conn = new SqlConnection(connStr);
        }

        public SQLHelper(SqlConnection _conn) 
        {
            conn = _conn;
        }

        private SqlConnection GetConn()
        {
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }

        public int ExecuteNonQuery(string sql)
        {
            int res;
            try
            {
                cmd = new SqlCommand(sql, GetConn());
                cmd.CommandTimeout = 600;
                res = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                res = -1;
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return res;
        }

        public int ExecuteNonQuery(string sql, SqlParameter[] paras)
        {
            int res;
            using (cmd = new SqlCommand(sql, GetConn()))
            {

                cmd.CommandTimeout = 600;
                cmd.Parameters.AddRange(paras);
                try
                {
                    res = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    cmd.Parameters.Clear();
                    throw;
                }

            }
            if (conn.State == ConnectionState.Open)
            {
                cmd.Parameters.Clear();
                conn.Close();
            }
            return res;
        }

        public int ExecuteScaler(string sql, SqlParameter[] paras)
        {
            int res;
            using (cmd = new SqlCommand(sql, GetConn()))
            {

                cmd.CommandTimeout = 600;
                cmd.Parameters.AddRange(paras);
                try
                {
                    res = (int)cmd.ExecuteScalar();

                }
                catch (Exception ex)
                {
                    cmd.Parameters.Clear();
                    throw;
                }

            }
            if (conn.State == ConnectionState.Open)
            {
                cmd.Parameters.Clear();
                conn.Close();
            }
            return res;
        }

        public object ExecuteScalerObject(string sql, SqlParameter[] paras)
        {
            object res = null;
            using (cmd = new SqlCommand(sql, GetConn()))
            {

                cmd.CommandTimeout = 600;
                cmd.Parameters.AddRange(paras);
                try
                {
                    res = cmd.ExecuteScalar();

                }
                catch (Exception ex)
                {
                    cmd.Parameters.Clear();
                    throw;
                }

            }
            if (conn.State == ConnectionState.Open)
            {
                cmd.Parameters.Clear();
                conn.Close();
            }
            return res;
        }



        public DataTable ExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            cmd = new SqlCommand(sql, GetConn());
            cmd.CommandTimeout = 600;
            using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(sdr);
            }
            return dt;
        }

        public DataSet GetDataSet(string sql)
        {

            SqlDataAdapter sda = new SqlDataAdapter(sql, GetConn());
            sda.SelectCommand.CommandTimeout = 600;
            DataSet dset = new DataSet();
            sda.Fill(dset);

            return dset;
        }


        public DataTable ExecuteQuery(string sql, SqlParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SqlCommand(sql, GetConn());
            cmd.CommandTimeout = 600;
            cmd.Parameters.AddRange(paras);
            using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(sdr);
            }
            cmd.Parameters.Clear();
            return dt;
        }

        public DataTable ExecuteStoreProcedureQuery(string storeProcedure, SqlParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SqlCommand(storeProcedure, GetConn());
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 6000;
            cmd.Parameters.AddRange(paras);
            try
            {
                using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    dt.Load(sdr);
                }
                cmd.Parameters.Clear();
            }
            catch(Exception ex)
            {
                cmd.Parameters.Clear();
                throw;
            }
            if (conn.State == ConnectionState.Open)
            {
                cmd.Parameters.Clear();
                conn.Close();
            }
            return dt;
        }


        public bool BoolExecuteQuery(string sql, SqlParameter[] paras)
        {
            DataTable dt = new DataTable();
            cmd = new SqlCommand(sql, GetConn());
            cmd.CommandTimeout = 600;
            cmd.Parameters.AddRange(paras);
            try
            {
                using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    dt.Load(sdr);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            DataRow[] rows = dt.Select();
            bool temp = false;
            if (rows.Length > 0)
            {
                temp = true;
            }
            return temp;
        }


        public bool BoolExecuteQuery(string sql)
        {
            DataTable dt = new DataTable();
            cmd = new SqlCommand(sql, GetConn());
            cmd.CommandTimeout = 600;
            using (sdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                dt.Load(sdr);
            }
            DataRow[] rows = dt.Select();
            bool temp = false;
            if (rows.Length > 0)
            {
                temp = true;
            }
            return temp;
        }

        /*
        public static void WriteTextLog(string data)
        {
            string locationPath = EntryAssemblyLocation();
            try
            {
                if (!Directory.Exists(locationPath + @"\log"))
                {
                    Directory.CreateDirectory(locationPath + @"\log");
                }

                lock (syncObject)
                {
                    using (StreamWriter sw = new StreamWriter(
                        locationPath + @"\log\" +
                        DateTime.Now.ToString("yyyyMMdd") + @".log", true))
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + @"::" + data);
                        sw.Close();
                    }
                }
            }
            catch { }
        }
        */
        public static object syncObject = new object();

        public static void WriteDebug(string strMsg)
        {
            try
            {
                string path = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(
                   System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name, "");
                try
                {
                    if (!Directory.Exists(path + @"\Debug"))
                    {
                        Directory.CreateDirectory(path + @"\Debug");
                    }
                    //Directory.CreateDirectory(path.Substring(0, nPos) + @"\Debug");
                    lock (syncObject)
                    {
                        using (StreamWriter sw = new StreamWriter(
                            path + @"\Debug\" +
                            DateTime.Now.ToString("yyyyMMdd") + @"_DebugM.txt", true))
                        {
                            sw.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + @"::" + strMsg);
                            sw.Close();
                        }
                    }
                }
                catch
                {
                }
            }

            catch (Exception e)
            {
            }
        }

    }
}
