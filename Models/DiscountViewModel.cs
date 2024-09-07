using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetLand.Models
{
    public class DiscountViewModel
    {
        public string TenGiamGia { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }
}