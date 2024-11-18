using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace AdminSide.Models
{
    public class Utility
    {
        private static object lockObject = new object();
        string CS = string.Empty;

        public Utility()
        {
            if (string.IsNullOrEmpty(CS))
            {
                CS = ConfigurationManager.ConnectionStrings["DBCS"].ToString();
            }
        }
        #region DataSet
        public DataSet fn_DataSet(string procedure, params SqlParameter[] sqlParameters)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlDataAdapter tda = new SqlDataAdapter("dbo" + "." + procedure, con))
                    {
                        tda.SelectCommand.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter param in sqlParameters)
                        {
                            tda.SelectCommand.Parameters.Add(param);
                        }
                        DataSet DS = new DataSet();
                        lock (lockObject)
                        {
                            tda.Fill(DS);
                        }

                        return DS;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region Data Reader
        public IDataReader fn_DataReader(string procedure, params SqlParameter[] _Sqlparam)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo" + "." + procedure, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter param in _Sqlparam)
                        {
                            cmd.Parameters.Add(param);
                        }
                        con.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader())
                        {
                            var dt = new DataTable();
                            dt.Load(rdr);
                            return dt.CreateDataReader();
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region DataTable
        public DataTable fn_DataTable(string procedure)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlDataAdapter tda = new SqlDataAdapter("dbo" + "." + procedure, con))
                    {
                        tda.SelectCommand.CommandType = CommandType.StoredProcedure;
                        DataTable DT = new DataTable();
                        lock (lockObject)
                        {
                            tda.Fill(DT);
                        }

                        return DT;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region DataTable
        public DataTable fn_DataTable(string procedure, params SqlParameter[] sqlParameters)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlDataAdapter tda = new SqlDataAdapter("dbo" + "." + procedure, con))
                    {
                        tda.SelectCommand.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter param in sqlParameters)
                        {
                            tda.SelectCommand.Parameters.Add(param);
                        }
                        DataTable DT = new DataTable();
                        lock (lockObject)
                        {
                            tda.Fill(DT);
                        }

                        return DT;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region DataTable
        public DataTable fn_DataTable(string procedure, int timeout, params SqlParameter[] sqlParameters)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlDataAdapter tda = new SqlDataAdapter("dbo" + "." + procedure, con))
                    {
                        tda.SelectCommand.CommandType = CommandType.StoredProcedure;
                        tda.SelectCommand.CommandTimeout = timeout; // Set the command timeout

                        foreach (SqlParameter param in sqlParameters)
                        {
                            tda.SelectCommand.Parameters.Add(param);
                        }

                        DataTable DT = new DataTable();
                        lock (lockObject)
                        {
                            tda.Fill(DT);
                        }

                        return DT;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region Execute Non Query
        public int func_ExecuteNonQuery(string procedure, params SqlParameter[] _SqlParam)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo" + "." + procedure.ToString(), con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter para in _SqlParam)
                        {
                            cmd.Parameters.Add(para);
                        }
                        con.Open();
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion

        #region Execute Scalar
        public object func_ExecuteScalar(string procedure, params SqlParameter[] _SqlParam)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(CS))
                {
                    using (SqlCommand cmd = new SqlCommand("dbo" + "." + procedure.ToString(), con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        foreach (SqlParameter para in _SqlParam)
                        {
                            cmd.Parameters.Add(para);
                        }
                        con.Open();
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Log or handle SQL specific exceptions
                throw new Exception("An SQL exception occurred: " + sqlEx.Message, sqlEx);
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                throw new Exception("An error occurred while executing the stored procedure: " + ex.Message, ex);
            }
        }
        #endregion


    }
}