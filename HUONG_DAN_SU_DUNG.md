# HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG NHANH
## PHÂN HỆ 2 - NHÓM 09 (ỨNG DỤNG QUẢN LÝ DỮ LIỆU Y TẾ)

Tài liệu này giúp bạn lập tức biết cách cài đặt Cơ sở dữ liệu Oracle, chạy ứng dụng WinForms và thực hiện các kịch bản demo một cách nhanh chóng và dễ dàng nhất.

---

## ⚡ PHẦN 1: HƯỚNG DẪN CHẠY SCRIPT SQL (ORACLE)

Bạn có hai cách để thiết lập Cơ sở dữ liệu: chạy tự động toàn bộ (Khuyến nghị) hoặc chạy thủ công từng bước.

### Cách 1: Thiết lập tự động toàn bộ 

> [!IMPORTANT]
> **HƯỚNG DẪN CHẠY BẰNG SQL DEVELOPER**
> 1. Mở phần mềm SQL Developer.
> 2. Chọn **File -> Open**, tìm đến thư mục dự án và chọn mở trực tiếp file **`script SQL/00_run_all.sql`**. *(Không được copy-paste nội dung vào Worksheet trắng để tránh lỗi đường dẫn relative)*.
> 3. Chọn kết nối bằng tài khoản quản trị hệ thống **`SYS`** với vai trò **`SYSDBA`**.
> 4. Nhấn phím **`F5`** (hoặc nút **Run Script** hình tờ giấy có nút Play xanh lá).
> 
> Hệ thống sẽ tự động khởi tạo User quản trị `CQ09`, nạp toàn bộ cấu trúc bảng, dữ liệu mẫu, thiết lập phân quyền RBAC, VPD, OLS và cấu hình Audit.
---

### Cách 2: Thiết lập thủ công từng bước (Nếu muốn kiểm tra từng phần)

Đăng nhập bằng tài khoản **`SYS AS SYSDBA`** và chạy các file trong thư mục `script SQL/` theo đúng thứ tự sau:

| Thứ tự | File chạy | Tài khoản chạy | Chức năng chi tiết |
| :---: | :--- | :---: | :--- |
| **1** | `01_StoredProcedures.sql` | `SYS AS SYSDBA` | Khởi tạo schema `CQ09`, cấp các quyền Admin và tạo các thủ tục (Stored Procedure) quản trị user/role/grant. |
| **2** | `02_schema_data.sql` | `SYS` hoặc `CQ09` | Tạo các bảng (`NHANVIEN`, `BENHNHAN`, `HSBA`, `HSBA_DV`, `DONTHUOC`, `THONGBAO`), tạo index và nạp dữ liệu mẫu. |
| **3** | `03_role.sql` | `SYS` hoặc `CQ09` | Khởi tạo các vai trò (`RL_DIEUPHOI`, `RL_BACSI`,...) và tự động tạo tài khoản database tương ứng cho từng nhân viên/bệnh nhân. |
| **4** | `04_RBAC.sql` | `SYS` hoặc `CQ09` | Tạo các view bảo mật cơ bản (`vw_benhnhan`, `vw_nhanvien_canhan`,...) và cấp quyền SELECT, UPDATE có giới hạn cột cho các Role (RBAC truyền thống). |
| **5** | `05_VPD.sql` | `SYS` hoặc `CQ09` | Áp dụng chính sách kiểm soát truy cập mức dòng/cột nâng cao bằng VPD (Virtual Private Database) lên các bảng gốc để chống bypass. |
| **6** | `06_OLS_setup.sql` | `SYS AS SYSDBA` | Cấu hình Oracle Label Security trên bảng `THONGBAO` phục vụ phát tán thông tin khẩn cấp theo nhãn (Level, Compartment, Group) cho user `u1` - `u8`. |
| **7** | `07_audit_setup.sql` | `SYS AS SYSDBA` | Cấu hình Standard Audit và Fine-Grained Audit (FGA) để ghi lại nhật ký khi có các thao tác nhạy cảm trên dữ liệu y tế. |
---

## 💻 PHẦN 2: HƯỚNG DẪN CHẠY ỨNG DỤNG WINFORMS

### 1. Build dự án
1. Mở Visual Studio và mở file solution: **`WindowsFormsApp1.slnx`** hoặc file project **`WindowsFormsApp1/WindowsFormsApp1.csproj`**.
2. Nhấn chuột phải vào Solution chọn **Restore NuGet Packages** (nếu Visual Studio chưa tự động tải thư viện `Oracle.ManagedDataAccess`).
3. Nhấn **F5** hoặc chọn **Build -> Build Solution** để biên dịch.
4. File chạy `.exe` sau khi biên dịch thành công sẽ nằm ở: `WindowsFormsApp1/bin/Debug/NHOM09.exe`.

### 2. Đăng nhập ứng dụng
Khi màn hình đăng nhập hiện ra, điền các thông tin kết nối sau:
* **Host**: `localhost` (hoặc IP máy chủ Oracle)
* **Port**: `1521` (mặc định của Oracle)
* **Service/PDB**: `XEPDB1` (PDB chứa schema dự án)
* **User & Password**: Nhập theo bảng tài khoản demo bên dưới.
* **SYSDBA**: Chỉ tích chọn ô này khi đăng nhập bằng tài khoản quản trị `SYS`.

---

## 🔑 PHẦN 3: DANH SÁCH TÀI KHOẢN DEMO & GIAO DIỆN TƯƠNG ỨNG

Ứng dụng WinForms tự động nhận diện vai trò của tài khoản đăng nhập để mở màn hình chức năng phù hợp:

| Tài khoản | Mật khẩu | Vai trò hệ thống | Màn hình hiển thị | Tính năng chính cần demo |
| :--- | :---: | :---: | :--- | :--- |
| **`SYS`** | *(Theo máy)* | **DBA / Admin** | `MainForm` | Quản trị viên: tạo/khóa/xóa User, Role, Cấp/Thu hồi quyền hệ thống, xem bảng quyền và duyệt cấu trúc database. |
| **`NV001`** | `ATBM123` | **Điều phối viên** | `CoordinatorForm` | Quản lý danh sách bệnh nhân; Tạo hồ sơ bệnh án (HSBA); Phân công bác sĩ điều trị và kỹ thuật viên dịch vụ. |
| **`BS001`** | `ATBM123` | **Bác sĩ / Y sĩ** | `DoctorForm` | Chỉ xem các HSBA mình phụ trách điều trị (VPD); Cập nhật chẩn đoán/điều trị; Kê đơn thuốc; Chỉ định dịch vụ y tế. |
| **`KTV01`** | `ATBM123` | **Kỹ thuật viên** | `TechnicianForm` | Chỉ xem dịch vụ được chỉ định cho mình (VPD); Chỉ được phép cập nhật cột Kết quả (`KETQUA`). |
| **`BN000001`** | `ATBM123` | **Bệnh nhân** | `PatientForm` | Chỉ tự xem thông tin cá nhân của mình; Chỉ được phép cập nhật địa chỉ, tiền sử bệnh, dị ứng thuốc. |
| **`u1`** đến **`u8`** | `ATBM123` | **OLS Demo User** | `OlsDemoForm` | Đọc các thông báo khẩn cấp từ bảng `THONGBAO` dựa trên nhãn bảo mật OLS (Ví dụ: `u1` đọc được tất cả, `u4` chỉ đọc được thông báo chung). |

---

## 🧪 PHẦN 4: KỊCH BẢN DEMO NHANH (MẪU)

### Kịch bản 1: Demo chính sách bảo mật VPD (Bác sĩ, KTV, Bệnh nhân)
1. **Bác sĩ**: Đăng nhập bằng `BS001`. Kiểm tra danh sách HSBA chỉ hiển thị các bệnh án do `BS001` phụ trách. Thực hiện cập nhật chẩn đoán hoặc thêm đơn thuốc mới.
2. **Kỹ thuật viên**: Đăng nhập bằng `KTV01`. Kiểm tra danh sách dịch vụ chỉ hiển thị dịch vụ giao cho mình. Thử sửa thông tin ngoài cột `KETQUA` hệ thống sẽ báo lỗi.
3. **Bệnh nhân**: Đăng nhập bằng `BN000001`. Kiểm tra chỉ thấy thông tin cá nhân của chính mình. Cập nhật địa chỉ thành công, nhưng không sửa được Mã BN hay Họ tên.

### Kịch bản 2: Demo Oracle Label Security (OLS)
1. Đăng nhập bằng **`u1`** (Ban Giám Đốc toàn viện) -> Xem được **tất cả 7 thông báo** từ bảng `THONGBAO`.
2. Đăng nhập bằng **`u4`** (Nhân viên khoa Thần kinh tại TP.HCM) -> Chỉ xem được đúng **1 thông báo chung** (t1).
3. Đăng nhập bằng **`u8`** (Nhân viên khoa Tiêu hóa tại Hà Nội) -> Xem được **2 thông báo**: thông báo chung (t1) và thông báo riêng cho khoa Tiêu hóa Hà Nội (t6).

### Kịch bản 3: Demo Kiểm toán (Audit) & Khôi phục dữ liệu (Recovery)
1. Đăng nhập bằng **`SYS AS SYSDBA`** và chạy file kịch bản tạo log: `@script SQL/08_audit_test_actions.sql`.
2. Đọc log kiểm toán để thấy các hành vi truy cập hợp lệ và bất hợp pháp bằng cách chạy: `@script SQL/09_audit_read_logs.sql`.
3. Để demo sự cố khôi phục:
   * Chạy kịch bản tạo sự cố (Bác sĩ `BS001` sửa sai liều dùng đơn thuốc): `@script SQL/04_backup_recovery/03_demo_su_co.sql`.
   * Đọc audit log định vị thời điểm xảy ra sự cố: `@script SQL/04_backup_recovery/04_check_audit_log.sql`.
   * Chạy script khôi phục đơn thuốc bằng Flashback Query về thời điểm trước đó: `@script SQL/04_backup_recovery/05_flashback_restore.sql`.
---
