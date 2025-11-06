using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class PromotionViewModel
    {
        public int PromotionID { get; set; }
        public string PromotionName { get; set; }
        public string Description { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}