using PetLand.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetLand.Models
{
    public class CartItemViewModel
    {
        public KhachHang KhachHang { get; set; }
        public int Giohangid { get; set; }
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }
        public decimal TongGia { get; set; }
        public int KhachHangid { get; set; }
        public List<int> SelectedItems { get; set; } // Danh sách ID sản phẩm đã chọn
        public List<CartItemViewModel> CartItems { get; set; }
        public string Tenchitiet { get; set; }
        public decimal TongTatCaSanPham { get; set; }
    }
}
