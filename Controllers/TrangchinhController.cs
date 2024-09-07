using PetLand.Areas.Admin.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using PetLand.Models;

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
        public ActionResult Cart(int id)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                // Sử dụng LINQ để truy vấn dữ liệu
                var cartItems = (from kh in db.KhachHangs
                                 join gh in db.Giohangs on kh.idKhachHang equals gh.idkhachhang
                                 join sp in db.SanPhams on gh.idsanpham equals sp.idSanPham
                                 where kh.idKhachHang == id
                                 select new CartItemViewModel
                                 {
                                     KhachHangid = kh.idKhachHang,
                                     Giohangid = gh.idgiohanghang,
                                     SanPhamId = sp.idSanPham,
                                     TenSanPham = sp.tensp,
                                     Gia = (decimal)sp.gia,
                                     SoLuong = (int)gh.sl,
                                     TongGia = (decimal)(gh.sl * sp.gia)
                                 }).ToList();
                return View(cartItems);
            }
        }
        public ActionResult cart_delete(int id)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                // Tìm sản phẩm trong giỏ hàng theo id
                var giohangItem = db.Giohangs.FirstOrDefault(g => g.idgiohanghang == id);

                if (giohangItem != null)
                {
                    int? idKhachHang = giohangItem.idkhachhang; 
                    // Xóa sản phẩm khỏi giỏ hàng
                    db.Giohangs.Remove(giohangItem);
                    db.SaveChanges();

                    // Điều hướng về trang khách hàng sử dụng idKhachHang
                    return RedirectToAction("cart", new { id = idKhachHang });
                }
                // Nếu không tìm thấy giỏ hàng, điều hướng trở lại trang giỏ hàng
                return RedirectToAction("cart");
            }
        }
        [HttpPost]
        public ActionResult Cart_bot(int idsp, int change,int idkhachhang) 
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                var product = db.Giohangs.FirstOrDefault(x => x.idsanpham == idsp && x.idkhachhang == idkhachhang);
                int? idKhachHang = product.idkhachhang;
                if (product != null)
                {
                    product.sl += change;

                    if (product.sl < 1)
                    {
                        product.sl = 1; // Đảm bảo số lượng không nhỏ hơn 1
                    }
                    db.SaveChanges();
                }
                // Quay về trang hiện tại hoặc chuyển hướng đến trang khác nếu cần
                return RedirectToAction("cart", new { id = idKhachHang });
            }
        }
        [HttpPost]
        public ActionResult Cart_them(int idsp, int change, int idkhachhang)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                var product = db.Giohangs.FirstOrDefault(x => x.idsanpham == idsp && x.idkhachhang == idkhachhang);
                int? idKhachHang = product.idkhachhang;
                if (product != null)
                {
                    product.sl -= change;

                    if (product.sl < 1)
                    {
                        product.sl = 1; // Đảm bảo số lượng không nhỏ hơn 1
                    }
                    db.SaveChanges();
                }
                // Quay về trang hiện tại hoặc chuyển hướng đến trang khác nếu cần
                return RedirectToAction("cart", new { id = idKhachHang });
            }
        }
    }
}
