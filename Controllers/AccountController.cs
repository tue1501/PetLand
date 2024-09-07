using PetLand.Areas.Admin.Models;
using PetLand.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace PetLand.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string sdt, string matkhau)
        {
            try
            {
                PetLandEntities db = new PetLandEntities();

                // Tìm kiếm khách hàng với số điện thoại và mật khẩu đã nhập
                var khachHang = db.KhachHangs.FirstOrDefault(kh => kh.sdt == sdt && kh.matkhau == matkhau);

                if (khachHang != null)
                {
                    // Nếu tìm thấy khách hàng, lưu thông tin vào Session và chuyển hướng đến trang chính
                    Session["KhachHang"] = khachHang;
                    return RedirectToAction("Index", "Trangchinh");
                }
                else
                {
                    // Kiểm tra xem số điện thoại có tồn tại không
                    var khachHangTonTai = db.KhachHangs.FirstOrDefault(kh => kh.sdt == sdt);
                    if (khachHangTonTai == null)
                    {
                        ViewBag.ErrorMessage = "Số điện thoại không chính xác. Vui lòng thử lại.";
                    }
                    else
                    {
                        ViewBag.ErrorMessage1 = "Mật khẩu không chính xác. Vui lòng thử lại.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi và hiển thị thông báo lỗi tổng quát cho người dùng
                ViewBag.ErrorMessage3 = "Đã xảy ra lỗi: " + ex.Message;
            }
            // Trả về view gốc nếu có lỗi
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string sdt, string matkhau, string hoten, string gmail, string confirmPassword)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                // Kiểm tra xem số điện thoại đã tồn tại chưa
                var khachhangs = db.KhachHangs.FirstOrDefault(kh => kh.sdt == sdt);
                if (khachhangs != null)
                {
                    // Nếu số điện thoại đã tồn tại, hiển thị thông báo lỗi
                    ViewBag.ErrorMessage2 = "Số điện thoại đã tồn tại. Vui lòng thử lại.";
                    return View(); // Trả lại View cùng với model để giữ nguyên thông tin đã nhập

                }
                else
                {
                    if (matkhau != confirmPassword)
                    {
                        ViewBag.ErrorMessage3 = "Mật khẩu và xác nhận mật khẩu không khớp.";
                        return View(); // Trả về view với thông báo lỗi
                    }
                    else
                    {
                        var newItem = new KhachHang
                        {
                            sdt = sdt,
                            matkhau = matkhau,
                            hoten = hoten,
                            gmail = gmail,
                        };
                        db.KhachHangs.Add(newItem);
                        db.SaveChanges();
                        return RedirectToAction("Login"); // Chuyển hướng sau khi thêm thành công
                    }
                }
            }
        }
        public ActionResult Changepass()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Changepass(int id, string matkhau, string matkhaunew, string confirmPassword)
        {
            try
            {
                PetLandEntities db = new PetLandEntities();
                var khachhangs = db.KhachHangs.FirstOrDefault(kh => kh.matkhau == matkhau);
                if (khachhangs != null)
                {
                    if (matkhaunew != confirmPassword)
                    {
                        ViewBag.ErrorMessage3 = "Mật khẩu và xác nhận mật khẩu không khớp.";
                        return View(); // Trả về view với thông báo lỗi
                    }
                    else
                    {
                        var updateModel = db.KhachHangs.Find(id);
                        updateModel.matkhau = matkhaunew;
                        db.SaveChanges();
                        return RedirectToAction("Info/", new { id = khachhangs.idKhachHang });
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Mật khẩu sai";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi và hiển thị thông báo lỗi tổng quát cho người dùng
                ViewBag.ErrorMessage3 = "Đã xảy ra lỗi: " + ex.Message;
            }
            // Trả về view gốc nếu có lỗi
            return View();
        }
        public ActionResult Info(int id)
        {
            PetLandEntities db = new PetLandEntities();
            KhachHang model12 = db.KhachHangs.Find(id);
            return View(model12);
        }
        public ActionResult Logout()
        {
            Session.Remove("KhachHang");
            FormsAuthentication.SignOut();
            return View("Login");
        }
        public ActionResult Address(int id)
        {
            PetLandEntities db = new PetLandEntities();
            KhachHang model12 = db.KhachHangs.Find(id);
            return View(model12);
        }
        public ActionResult add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Address(KhachHang model)
        {
            PetLandEntities db = new PetLandEntities();
            var updateModel = db.KhachHangs.Find(model.idKhachHang);
            updateModel.diachi = model.diachi;
            db.SaveChanges();
            return RedirectToAction("Info", new { id = model.idKhachHang });
        }
        public ActionResult DeleteAccount(int id)
        {
            PetLandEntities db = new PetLandEntities();
            var updateModel = db.KhachHangs.Find(id);
            db.KhachHangs.Remove(updateModel);
            db.SaveChanges();
            return RedirectToAction("Login");
        }
        public ActionResult Discount(int id)
        {
            using (PetLandEntities db = new PetLandEntities())
            {
                // Sử dụng LINQ để truy vấn dữ liệu
                var result = (from kh in db.KhachHangs
                              join ct in db.ChiTietGiamGias on kh.idKhachHang equals ct.idKhachHang
                              join gg in db.GiamGias on ct.idGiamGia equals gg.idGiamGia
                              where kh.idKhachHang == id
                              select new DiscountViewModel
                              {
                                  TenGiamGia = gg.tengiamgia,
                                  MoTa = gg.mota,
                                  NgayBatDau = gg.ngaybatdau,
                                  NgayKetThuc = gg.ngayketthuc
                              }).ToList();
                return View(result);
            }
        }
    }
}
