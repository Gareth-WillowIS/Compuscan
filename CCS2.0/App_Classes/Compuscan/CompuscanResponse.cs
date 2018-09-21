using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CCS2._0.App_Classes.Compuscan
{
    public class CompuscanResponse
    {
        public string conumserFirstName { get; set; }
        public string consumerLastName { get; set; }
        public string consumerIDnumber { get; set; }
        public string codix { get; set; }
        public int compuScore { get; set; }
        public string generic1 { get; set; }
        public bool debtReview { get; set; }
        public bool administration { get; set; }
        public int salesAgentID { get; set; }
        public DateTime dateCreated { get; set; }
        public int applicationID { get; set; }
        public string xmlReportData { get; set; }
        public string htmlReportData { get; set; }
    }
}