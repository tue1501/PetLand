using PetLand.Areas.Admin.Models;
using System;
using System.Collections.Generic;
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
                var khachHangTonTai = db.KhachHangs.FirstOrDefault(kh => kh.sdt == sdt);
                if (khachHangTonTai == null)
                {
                    ViewBag.ErrorMessage = "Số điện thoại không chính xác. Vui lòng thử lại.";
                }
                else
                {
                    ViewBag.ErrorMessage1 = "Mật khẩu không chính xác. Vui lòng thử lại.";
                }
                return View();
            }
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        //[ValidateAntiForgeryToken]
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
        public ActionResult Info()
        {
            return View();
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
            return RedirectToAction("Info");
        }
    }
}