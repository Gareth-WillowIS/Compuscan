using CCS2._0.App_Classes.Compuscan;
using CCS2._0.CONTROLLER;
using Ionic.Zip;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;

namespace CCS2._0.Compuscan
{


    public partial class compuscanCreditReport : System.Web.UI.Page
    {
        CompuscanResponse objCompuscanResponse = new CompuscanResponse();

        protected void Page_Load(object sender, EventArgs e)
        {
            string status = "";

            if(string.IsNullOrEmpty(Request.QueryString["firstName"]) || string.IsNullOrEmpty(Request.QueryString["lastName"]) || string.IsNullOrEmpty(Request.QueryString["IDNumber"]) || string.IsNullOrEmpty(Request.QueryString["salesAgentID"]) || string.IsNullOrEmpty(Request.QueryString["applicationID"]))
            {
                status = "Insufficient URL parameters passed";
                lblStatus.Text = status;
                return;
            }

            if (!Page.IsPostBack)
            {
                
                objCompuscanResponse.conumserFirstName = Request.QueryString["firstName"]; // "John";
                objCompuscanResponse.consumerLastName = Request.QueryString["lastName"]; //"Doe";
                objCompuscanResponse.consumerIDnumber = Request.QueryString["IDNumber"]; //"8209147250087";
                objCompuscanResponse.salesAgentID = Convert.ToInt32(Request.QueryString["salesAgentID"]); //333;
                objCompuscanResponse.applicationID = Convert.ToInt32(Request.QueryString["applicationID"]); // 123456789;

                StringBuilder pTransaction = new StringBuilder();

                try
                {
                    status = getCompuscanCreditReport();
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
                    sendErrorMail("Compuscan Credit Report", errMessage, innerException, "Error on Compuscan Credit Report");
                    status = "We encountered an error retrieving the Compuscan report";
                }
            }

            lblStatus.Text = status;
        }

        private string getCompuscanCreditReport()
        {
            string status = "";

            CompuscanService.NormalEnqRequestParamsType request = new CompuscanService.NormalEnqRequestParamsType();
            request.pUsrnme = ConfigurationManager.AppSettings["compuscanRequestUsername"];  // "77833-1";
            request.pPasswrd = ConfigurationManager.AppSettings["compuscanRequestPassword"]; //"devtest";
            request.pVersion = "1.0";
            request.pOrigin = ConfigurationManager.AppSettings["compuscanRequestOrigin"]; // "TEST";
            request.pOrigin_Version = ConfigurationManager.AppSettings["compuscanRequestOriginVersion"]; //"1.0";
            request.pInput_Format = "XML";
            request.pTransaction = string.Format( @"<Transactions>
                        <Search_Criteria>
                        <CS_Data>Y</CS_Data>
                        <CPA_Plus_NLR_Data>Y</CPA_Plus_NLR_Data>
                        <Deeds_Data>N</Deeds_Data>
                        <Directors_Data>N</Directors_Data>
                        <Identity_number>{0}</Identity_number>
                        <Surname>{1}</Surname>
                        <Forename>{2}</Forename>
                        <Forename2></Forename2>
                        <Forename3></Forename3>
                        <Gender></Gender>
                        <Passport_flag>N</Passport_flag>
                        <DateOfBirth></DateOfBirth>
                        <Address1></Address1>
                        <Address2></Address2>
                        <Address3></Address3>
                        <Address4></Address4>
                        <PostalCode></PostalCode>
                        <HomeTelCode></HomeTelCode>
                        <HomeTelNo></HomeTelNo>
                        <WorkTelCode></WorkTelCode>
                        <WorkTelNo></WorkTelNo>
                        <CellTelNo></CellTelNo>
                        <ResultType>XHML</ResultType>
                        <RunCodix>Y</RunCodix>
                        <CodixParams>
                        <PARAMS>
                        <PARAM_NAME></PARAM_NAME>
                        <PARAM_VALUE></PARAM_VALUE>
                        </PARAMS>
                        <PARAMS>
                        <PARAM_NAME></PARAM_NAME>
                        <PARAM_VALUE></PARAM_VALUE>
                        </PARAMS>
                        </CodixParams>
                        <Adrs_Mandatory>N</Adrs_Mandatory>  
                        <Enq_Purpose>12</Enq_Purpose>
                        <Run_CompuScore>Y</Run_CompuScore> 
                        <ClientConsent>Y</ClientConsent>
                        </Search_Criteria>
                        </Transactions>", objCompuscanResponse.consumerIDnumber, objCompuscanResponse.consumerLastName, objCompuscanResponse.conumserFirstName);


            CompuscanService.NormalSearchService comp = new CompuscanService.NormalSearchService();        
            var response = comp.DoNormalEnquiry(request);

            if (response.transactionCompleted == true)
            {
                status = Decompress(Convert.FromBase64String(response.retData), objCompuscanResponse);
            }
            else
            {
                string err = response.errorString;
                status = err;
                
            }

            return status;
        }
        

        public static string Decompress(byte[] data, CompuscanResponse objCompuscanResponse)
        {
            string status = "Compuscan data saved";

            using (MemoryStream stream = new MemoryStream(data))
            using (ZipFile zout = ZipFile.Read(stream))
            {
                MemoryStream ms = new MemoryStream();
                MemoryStream msHTML = new MemoryStream();

                foreach (ZipEntry z in zout.Where(x => x.FileName.EndsWith("xml")))
                {
                    z.Extract(ms);                    
                }

                ms.Seek(0, System.IO.SeekOrigin.Begin);
                XDocument xDoc = XDocument.Load(ms);


                var xmlDoc2 = new XmlDocument();
                string xmlData = Encoding.UTF8.GetString(ms.ToArray());
                xmlDoc2.LoadXml(xmlData);
                var nodes = xmlDoc2.SelectNodes("/ROOT/EnqCC_CompuSCORE/ROW[@num=1]/SCORE");
                string compuscoreValue = nodes[0].InnerText;

                int compuscore = 0;
                if (!string.IsNullOrEmpty(compuscoreValue))
                {
                    compuscore = Convert.ToInt32(compuscoreValue);
                }
                
                StringBuilder html = new StringBuilder();

                foreach (ZipEntry z in zout.Where(x => x.FileName.EndsWith("html")))
                {                    
                    z.Extract(msHTML);
                    html.Append(Encoding.UTF8.GetString(msHTML.ToArray()));
                }

                status = CompuscanController.insertCompuscanResponseData(objCompuscanResponse, ms.ToArray(), html, compuscore);

                return status;
            }
        }


        public static void sendErrorMail(string page, string exception, string innerException, string subject, string additionalInfo = null)
        {
            string mailBody = "An error occurred at " + DateTime.Now + " on " + page + ":" + Environment.NewLine + "Exception:" + exception;
            if (innerException != "")
            {
                mailBody += Environment.NewLine + "Inner Exception: " + innerException;
            }

            if (!string.IsNullOrEmpty(additionalInfo))
            {
                mailBody += Environment.NewLine + additionalInfo;
            }

            MailMessage mail = new MailMessage(ConfigurationManager.AppSettings["errorMailEmailAddress"], ConfigurationManager.AppSettings["errorMailEmailAddress"]);
            SmtpClient client = new SmtpClient();
            client.Port = 25;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = ConfigurationManager.AppSettings["smtp"];
            mail.Subject = subject;
            mail.Body = mailBody;

            client.Send(mail);
        }
    }
}