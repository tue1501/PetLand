using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetLand.Models
{
    public class CartItemViewModel
    {
        public int Giohangid { get; set; }
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal TongGia { get; set; }
        public int KhachHangid { get; set; }
    }
}