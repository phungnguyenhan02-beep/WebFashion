using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class PaymentWebHookModel
    {
        public bool status { get; set; }
        public List<TransactionDataModel> Data { get; set; }

    }
    public class TransactionDataModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public string TransactionID { get; set; }
        public string Amount { get; set; }
        public string Description { get; set; }
        public string Date { get; set; }
        public string Bank { get; set; }
    }
}
