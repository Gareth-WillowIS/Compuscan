using CompuscanUtils;
using DALC4NET;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CCS2._0.DATAACCESSLAYER
{
    public class DAL
    {
        private DBHelper _dbHelper = null;
        
        public DAL()
        {
            _dbHelper = new DBHelper();
        }

        public string InsertData(string storedProc, DBParameterCollection paramCollection)
        {


            /*
            DBParameter param1 = new DBParameter("@FIRSTNAME", "Yash");
            DBParameter param2 = new DBParameter("@LASTNAME", "Tripathi");
            DBParameter param3 = new DBParameter("@EMAIL", "yash.tripathi@yahoo.com");

            DBParameterCollection paramCollection = new DBParameterCollection();
            paramCollection.Add(param1);
            paramCollection.Add(param2);
            paramCollection.Add(param3);
            
            foreach (var param in paramCollection)
            {
                cmd.Parameters.Add(param);
            }
            */

            string status = _dbHelper.ExecuteNonQuery(storedProc, paramCollection, CommandType.StoredProcedure) > 0 ? "Record inserted successfully." : "Error in inserting record.";
            
            //if(status == "Error in inserting record.")
            //{
            //    string errMessage = status;
            //    string innerException = "";

                

            //    string additionalInfo = storedProc;
            //    Utils.sendErrorMail("Data Access Layer", errMessage, innerException, "Error on Data Access Layer", additionalInfo);
            //}

            return status;
        }

        public string updateData(string storedProc, DBParameterCollection paramCollection)
        {            
            string status = _dbHelper.ExecuteNonQuery(storedProc, paramCollection, CommandType.StoredProcedure) > 0 ? "Record inserted successfully." : "Error in inserting record.";
            
            return status;
        }

        #region sqlBulkcopy
        public static string doBulkDataInsert(DataTable dtBulkcopyData, string destinationTableName)
        {
            string status = "Data saved";
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CCS_Con"].ConnectionString;
            // Open a sourceConnection to the AdventureWorks database.
            using (SqlConnection sourceConnection = new SqlConnection(connectionString))
            {
                sourceConnection.Open();

                using (SqlConnection destinationConnection = new SqlConnection(connectionString))
                {
                    destinationConnection.Open();

                    // Column positions in the source
                    // data reader match the column positions in 
                    // the destination table so there is no need to
                    // map columns.
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
                    {
                        bulkCopy.DestinationTableName = destinationTableName;

                        try
                        {
                            // Write from the source to the destination.
                            bulkCopy.WriteToServer(dtBulkcopyData);
                        }
                        catch (Exception ex)
                        {
                            string errMessage = "";
                            string innerException = "";

                            status = ex.Message;

                            errMessage = ex.Message.ToString();
                            if (ex.InnerException != null)
                            {
                                innerException = ex.InnerException.ToString();
                            }
                            Utils.sendErrorMail("Data Access Layer", errMessage, innerException, "Error on Data Access Layer");
                        }
                    }

                    // Perform a final count on the destination 
                    // table to see how many rows were added.
                    //long countEnd = System.Convert.ToInt32(
                    //    commandRowCount.ExecuteScalar());                    
                }
            }
            return status;
        }

        public static string mergeBankTransactions(DataTable dtBulkcopyData)
        {
            string status = "Data saved";
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["CCS_Con"].ConnectionString;
            // Open a sourceConnection to the AdventureWorks database.
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                
                using (SqlCommand cmd = new SqlCommand("BulkInsertUpdateYodleeTransactions"))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@tblYodleeTransactions", dtBulkcopyData);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }                
            }
            return status;
        }


        internal DataTable StoredProcSelect(string storedProcName, DBParameterCollection sqlParams = null)
        {            
            DataTable table = new DataTable();            

            try
            {
                table = _dbHelper.ExecuteDataTable(storedProcName, sqlParams, CommandType.StoredProcedure);                
            }
            catch (Exception err)
            {
                string errMessage = "";
                string innerException = "";

                errMessage = err.Message.ToString();
                if (err.InnerException != null)
                {
                    innerException = err.InnerException.ToString();
                }
                Utils.sendErrorMail("Data Access Layer", errMessage, innerException, "Error on Data Access Layer");
            }            

            return table;
        }

        internal object StoredProcExecuteScalar(string storedProcName, DBParameterCollection sqlParams = null)
        {
            object objResult = new object();

            try
            {
                objResult = _dbHelper.ExecuteScalar(storedProcName, sqlParams, CommandType.StoredProcedure);
            }
            catch (Exception err)
            {
                string errMessage = "";
                string innerException = "";

                errMessage = err.Message.ToString();
                if (err.InnerException != null)
                {
                    innerException = err.InnerException.ToString();
                }
                Utils.sendErrorMail("Data Access Layer", errMessage, innerException, "Error on Data Access Layer");
            }

            return objResult;
        }

        #endregion
    }
}