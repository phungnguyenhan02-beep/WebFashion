using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LT_WebThoiTrang.Models
{
    public class AddressUserViewModel
    {
        public int IdUser { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Province { get; set; }
        public string Town { get; set; }
        public string Block { get; set; }
        public string SpecificAddress { get; set; }
        public string Email { get; set; }
    }
}