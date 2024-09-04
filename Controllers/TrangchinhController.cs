using PetLand.Areas.Admin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using System;

namespace PetLand.Areas.Admin.Controllers
{
    public class TrangchinhController : Controller
    {
        // GET: Trangchinh
        public ActionResult Index(string filter)
        {
             if (Session["KhachHang"] == null)
             {
                    return RedirectToAction("Login", "Account");
             }

            PetLandEntities db = new PetLandEntities();
            if (string.IsNullOrEmpty(filter))
            {
                List<SanPham> dssanpham = db.SanPhams.ToList();
                ViewBag.Filter = filter; // Truyền biến filter đến View
                return View(dssanpham);
            }
            List<SanPham> dsSanpham = db.SanPhams.Where(m =>
            m.tensp.ToLower().Contains(filter) == true ||
            m.mota.Contains(filter)).ToList();
            return View(dsSanpham);
        }
        public ActionResult Index_loaisp()
        {
            PetLandEntities db = new PetLandEntities();
                List<LoaiSanPham> dssanpham = db.LoaiSanPhams.ToList();
                return View(dssanpham);
        }
        //public ActionResult AddCart(int id)
        //{
        //    PetLandEntities db = new PetLandEntities();
        //    Giohang model1 = db.Giohangs.Find(id);
        //    return View(model1);
        //}

        [HttpPost]
        public ActionResult AddCart(int idSanPham, int idKhachHang, int soluong)
        {         
            using (PetLandEntities db = new PetLandEntities())
            {
                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                var existingItem = db.Giohangs.FirstOrDefault(item => item.idsanpham == idSanPham && item.idkhachhang == idKhachHang);
                var cartItems = db.Giohangs
                          .Where(g => g.idkhachhang == idKhachHang)
                          .ToList();

                if (existingItem != null)
                {
                    // Nếu sản phẩm đã có trong giỏ hàng, tăng số lượng lên theo giá trị truyền vào
                    existingItem.sl += soluong;
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ hàng, thêm sản phẩm mới với số lượng nhập vào
                    var newItem = new Giohang
                    {
                        idsanpham = idSanPham,
                        idkhachhang = idKhachHang,
                        sl = soluong,
                    };
                    db.Giohangs.Add(newItem);
                }
                // Lưu các thay đổi vào cơ sở dữ liệu
                db.SaveChanges();
            }
            // Chuyển hướng đến trang giỏ hàng
            return RedirectToAction("detail_sanpham", new { id = idSanPham });
        }

        public ActionResult detail_sanpham(int id)
        { 
            PetLandEntities db = new PetLandEntities();
            SanPham model1 = db.SanPhams.Find(id);
            return View(model1);
        }

        public ActionResult detail_danhgia(int id)
        {
            PetLandEntities db = new PetLandEntities();
            SanPham sanpham = db.SanPhams.Find(id);
            List<Danhgia> dsdanhgia = db.Danhgias
                                  .Where(d => d.idSanPham == id)
                                  .ToList();
            return View(dsdanhgia);
        }
        public ActionResult Order()
        {
            PetLandEntities db = new PetLandEntities();
            List<ChiTietDonHang> dsdonhang = db.ChiTietDonHangs.ToList();
            return View(dsdonhang);
        }
        public ActionResult Order_xacnhan(int id)
        {
            PetLandEntities db = new PetLandEntities();
            List<ChiTietDonHang> dssanpham = db.ChiTietDonHangs
                                 .Where(d => d.idChiTietDonHang == id)
                                 .ToList();
            return View(dssanpham);
        }
        public ActionResult cart()
        {
            var khachhang = Session["KhachHang"] as KhachHang;
            int idKhachHang = khachhang.idKhachHang;
            using (var db = new PetLandEntities())
            {
                // Lọc sản phẩm theo ID khách hàng
                var cartItems = db.Giohangs
                                  .Where(g => g.idkhachhang == idKhachHang)
                                  .Include(g => g.SanPham) // Include để tải dữ liệu sản phẩm
                                  .ToList();
                // Truyền danh sách sản phẩm đến view
                return View(cartItems);
            }
        }
        public ActionResult cart_delete(int id)
        {
                PetLandEntities db = new PetLandEntities();
                var updateModel = db.Giohangs.Find(id);
                db.Giohangs.Remove(updateModel);
                db.SaveChanges();
                return RedirectToAction("cart");
        }

        public ActionResult cart_deleteall()
        {

            PetLandEntities db = new PetLandEntities();
            var allItems = db.Giohangs.ToList();
            db.Giohangs.RemoveRange(allItems);
            db.SaveChanges();
            return RedirectToAction("cart");
        }
    }
}
