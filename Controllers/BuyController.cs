using PetLand.Areas.Admin.Models;
using PetLand.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PetLand.Controllers
{
    public class BuyController : Controller
    {
        [HttpPost]
        public ActionResult ProcessSelectedItems(List<int> selectedProductIds,int id)
        {
            PetLandEntities db = new PetLandEntities();
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                // Nếu không có sản phẩm nào được chọn
                TempData["ErrorMessage"] = "Bạn chưa chọn sản phẩm nào.";
                return RedirectToAction("Cart");
            }

            // Truy vấn thông tin sản phẩm được chọn từ CSDL dựa trên danh sách id sản phẩm
         var selectedProducts = db.Giohangs
        .Where(gh => selectedProductIds.Contains((int)gh.idsanpham) && gh.idkhachhang == id)
        .Join(
         db.SanPhams,
         gh => gh.idsanpham, // Khóa ngoại trong bảng Giohangs
         sp => sp.idSanPham, // Khóa chính trong bảng SanPhams
         (gh, sp) => new { gh, sp }  // Kết quả nối giữa Giohangs và SanPhams
        )
     .Join(
         db.ChiTietLoaiSanPhams,
         result => result.sp.idChiTietLoaiSanPham, // Khóa ngoại trong bảng SanPhams
         ctsp => ctsp.idChiTietLoaiSanPham, // Khóa chính trong bảng ChiTietSanPhams
         (result, ctsp) => new
         {
             result.gh,
             result.sp,
             ctsp // Thông tin chi tiết từ ChiTietSanPhams
         }  // Kết quả nối cuối cùng
     )
     .Select(result => new CartItemViewModel
     {
         KhachHangid = (int)result.gh.idkhachhang,
         Giohangid = result.gh.idgiohanghang,
         SanPhamId = result.sp.idSanPham,
         TenSanPham = result.sp.tensp,
         Gia = (decimal)result.sp.gia,
         SoLuong = (int)result.gh.sl,
         TongGia = (decimal)(result.gh.sl * result.sp.gia),
         Tenchitiet = result.ctsp.tenchitiet // Thông tin chi tiết từ ChiTietSanPhams
     })
     .ToList();
            decimal tongTatCaSanPham = selectedProducts.Sum(sp => sp.TongGia);

            var viewModel = new CartItemViewModel
            {
                CartItems = selectedProducts,
                TongTatCaSanPham = tongTatCaSanPham
            };
            return View("SelectedProducts", viewModel);
            // Trả về view hiển thị danh sách sản phẩm đã chọn
            //return View("SelectedProducts", selectedProducts);
        }
        // GET: Buy
        [HttpPost]
        public ActionResult ChangeAddress(int id, string diachi)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                var updateModel = db.KhachHangs.Find(id);
                if (updateModel == null)
                {
                    return Json(new { success = false, message = "Khách hàng không tồn tại." });
                }

                // Cập nhật địa chỉ
                updateModel.diachi = diachi;
                db.SaveChanges();

                return Json(new { success = true });
            }
        }
    }
}
