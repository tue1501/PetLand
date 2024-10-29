        [HttpPost]
        public ActionResult Pay(int id, string hoten, string diachi, string sdt, DateTime ngaytaodon, int tong, string ghichu, int? idgiamgia, List<int> idsp, List<int> sl, int tt_cod, int tt_online)
        {
            using (var db = new PetLandEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo đơn hàng mới (chưa lưu vào DB)
                        var newItem = new Donhang
                        {
                            idKhachHang = id,
                            sdtkh = sdt,
                            noinhan = diachi,
                            tenkh = hoten,
                            ngaytao = ngaytaodon,
                            idGiamGia = idgiamgia,
                            tongtien = tong,
                            ghichu = ghichu,
                            trangthai = 1, // Đơn hàng chờ thanh toán
                            tt_cod = tt_cod,
                            tt_online = tt_online,
                        };

                        db.Donhangs.Add(newItem);  // Thêm đơn hàng vào database
                        db.SaveChanges(); // Lưu đơn hàng vào DB để có ID đơn hàng
                        int donHangId = newItem.idDonhang;

                        // Thêm chi tiết đơn hàng vào bảng ChiTietDonHang
                        for (int i = 0; i < idsp.Count; i++)
                        {
                            int currentIdsp = idsp[i];
                            int currentSl = sl[i];

                            var chiTietDonHang = new ChiTietDonHang
                            {
                                idDonhang = donHangId,
                                idSanPham = currentIdsp,
                                sl = currentSl,
                                trangthai = 0,  // Chưa thanh toán
                            };
                            db.ChiTietDonHangs.Add(chiTietDonHang);

                            // Xóa sản phẩm khỏi giỏ hàng
                            var deleteItems = db.Giohangs.Where(gh => gh.idkhachhang == id && gh.idsanpham == currentIdsp).ToList();
                            foreach (var deleteItem in deleteItems)
                            {
                                db.Giohangs.Remove(deleteItem);
                            }

                            // Cập nhật tồn kho
                            var updateModel = db.SanPhams.Find(currentIdsp);
                            if (updateModel != null)
                            {
                                updateModel.tonkho -= currentSl;
                                if (updateModel.tonkho < 0)
                                {
                                    transaction.Rollback();
                                    return RedirectToAction("Cart", "Trangchinh", new { id = id, error = "Số lượng tồn kho không đủ" });
                                }
                            }
                            else
                            {
                                transaction.Rollback();
                                return RedirectToAction("Cart", "Trangchinh", new { id = id, error = "Sản phẩm không tồn tại." });
                            }
                        }

                        // Nếu có giảm giá, cập nhật trạng thái giảm giá
                        if (idgiamgia.HasValue)
                        {
                            var update = db.ChiTietGiamGias.FirstOrDefault(x => x.idGiamGia == idgiamgia.Value && x.idKhachHang == id);
                            if (update != null)
                            {
                                update.trangthai = 1;  // Đánh dấu trạng thái giảm giá đã sử dụng
                            }
                        }

                        db.SaveChanges();  // Lưu tất cả vào DB
                        transaction.Commit();  // Commit transaction

                        // Nếu tt_online == 1, tạo URL thanh toán VNPAY và chuyển hướng
                        if (tt_cod == 0)
                        {
                            // Đặt cờ trong session để theo dõi trạng thái thanh toán
                            Session["PaymentInProgress"] = true;

                            // Cấu hình URL trả về cho VNPAY
                            string vnp_Returnurl = Url.Action("ConfirmPayment", "Buy", new { id = donHangId }, Request.Url.Scheme);
                            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"];
                            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"];
                            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"];

                            VnPayLibrary vnpay = new VnPayLibrary();
                            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                            vnpay.AddRequestData("vnp_Command", "pay");
                            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                            vnpay.AddRequestData("vnp_Amount", (tong * 100).ToString()); // Số tiền thanh toán
                            vnpay.AddRequestData("vnp_BankCode", "VNBANK");
                            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                            vnpay.AddRequestData("vnp_CurrCode", "VND");
                            vnpay.AddRequestData("vnp_IpAddr", Request.UserHostAddress);
                            vnpay.AddRequestData("vnp_Locale", "vn");
                            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng: " + donHangId);
                            vnpay.AddRequestData("vnp_OrderType", "other");
                            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                            vnpay.AddRequestData("vnp_TxnRef", donHangId.ToString());

                            // Tạo URL thanh toán
                            string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                            // Chuyển hướng khách hàng đến trang thanh toán VNPAY
                            return Redirect(paymentUrl);
                        }
                        else
                        {
                            // Nếu tt_online != 1 (thanh toán COD), điều hướng đến trang đơn hàng thành công
                            return RedirectToAction("Order", "Order", new { id = id });
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return RedirectToAction("Cart", "Trangchinh", new { id = id, error = "Đã xảy ra lỗi khi thanh toán: " + ex.Message });
                    }
                }
            }
        }

        [HttpGet]
        public ActionResult ConfirmPayment(int id)
        {
            using (var db = new PetLandEntities())
            {
                // Giả định rằng bạn đã có logic để kiểm tra trạng thái thanh toán từ VNPAY
                bool paymentSuccess =false /* Logic để xác định trạng thái thanh toán từ VNPAY */;

                if (paymentSuccess)
                {
                    // Thanh toán thành công
                    Session["PaymentInProgress"] = null; // Xóa cờ thanh toán
                    return RedirectToAction("Orderupdate", "Order", new { id = id }); // Điều hướng đến trang đơn hàng thành công
                }
                else
                {
                    // Nếu thanh toán thất bại hoặc người dùng đã hủy, tiến hành hủy đơn hàng
                    var donHang = db.Donhangs.Find(id);
                    if (donHang != null)
                    {
                        // Cập nhật trạng thái đơn hàng thành đã hủy
                        donHang.trangthai = 6; // Giả sử trạng thái 2 là "Đã hủy"

                        // Cập nhật trạng thái giảm giá nếu có
                        if (donHang.idGiamGia.HasValue)
                        {
                            var updategg = db.ChiTietGiamGias.FirstOrDefault(kh => kh.GiamGia.idGiamGia == donHang.idGiamGia && kh.idKhachHang == donHang.idKhachHang);
                            if (updategg != null)
                            {
                                updategg.trangthai = 0; // Đánh dấu giảm giá đã được sử dụng lại
                            }
                        }

                        // Cập nhật lại tồn kho cho từng sản phẩm trong đơn hàng
                        var chiTietDonHangs = db.ChiTietDonHangs.Where(ct => ct.idDonhang == id).ToList();
                        foreach (var chiTiet in chiTietDonHangs)
                        {
                            var sanPham = db.SanPhams.Find(chiTiet.idSanPham);
                            if (sanPham != null)
                            {
                                sanPham.tonkho += chiTiet.sl; // Thêm số lượng sản phẩm vào tồn kho
                            }
                        }

                        // Xóa chi tiết đơn hàng
                        db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                    }

                    Session["PaymentInProgress"] = null; // Xóa cờ thanh toán
                    return RedirectToAction("Index", "Trangchinh"); // Quay về trang chính
                }
            }
        }
