using CCS2._0.App_Classes.Compuscan;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using CompuscanUtils;

namespace CCS2._0.CONTROLLER
{
    public class CompuscanController
    {   

        internal static string insertCompuscanResponseData(CompuscanResponse objCompuscanResponse, byte[] data, StringBuilder htmlData, int compuscore)
        {
            string status = "Compuscan data saved";
            try
            {


                using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CCS_Con"].ToString()))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandText = "insertCompuscanCreditScore";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = conn;

                        cmd.Parameters.Add("@ConsumerName", SqlDbType.VarChar).Value = objCompuscanResponse.conumserFirstName;
                        cmd.Parameters.Add("@ConsumerLastName", SqlDbType.VarChar).Value = objCompuscanResponse.consumerLastName;
                        cmd.Parameters.Add("@ConsumerIDNumber", SqlDbType.VarChar).Value = objCompuscanResponse.consumerIDnumber;
                        cmd.Parameters.Add("@Codix", SqlDbType.VarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@Compuscore", SqlDbType.Int).Value = compuscore;
                        cmd.Parameters.Add("@Generic1", SqlDbType.VarChar).Value = DBNull.Value;
                        cmd.Parameters.Add("@DebtReview", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@Administration", SqlDbType.Bit).Value = 0;
                        cmd.Parameters.Add("@SalesAgentID", SqlDbType.Int).Value = objCompuscanResponse.salesAgentID;
                        cmd.Parameters.Add("@ApplicationID", SqlDbType.Int).Value = objCompuscanResponse.applicationID;
                        cmd.Parameters.Add("@XMLReportData", SqlDbType.VarBinary).Value = data;
                        cmd.Parameters.Add("@htmlReportData", SqlDbType.VarChar).Value = htmlData.ToString();
                        conn.Open();
                        cmd.ExecuteNonQuery();

                    }
                }
            }

            catch(Exception err)
            {
                string errMessage = "";
                string innerException = "";

                errMessage = err.Message.ToString();
                if (err.InnerException != null)
                {
                    innerException = err.InnerException.ToString();
                }
                Utils.sendErrorMail("Compuscan Controller", errMessage, innerException, "Error on Compuscan Controller");
                status = "We encountered an error inserting Compuscan data into the database";
            }

            return status;
        }
    }
}