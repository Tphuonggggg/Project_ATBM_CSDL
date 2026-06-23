# HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG NHANH
## PHÂN HỆ 2 - NHÓM 09 (ỨNG DỤNG QUẢN LÝ DỮ LIỆU Y TẾ)

Tài liệu này giúp bạn lập tức biết cách cài đặt Cơ sở dữ liệu Oracle, chạy ứng dụng WinForms và thực hiện các kịch bản demo một cách nhanh chóng và dễ dàng nhất.

---

## ⚡ PHẦN 1: HƯỚNG DẪN CHẠY SCRIPT SQL (ORACLE)

> [!IMPORTANT]
> **PHÂN ĐỊNH VAI TRÒ QUẢN TRỊ TRONG DỰ ÁN**
> - **`CQ09`**: Là tài khoản **Quản trị viên trực tiếp của dự án** (Project DBA / Schema Owner). Toàn bộ cấu trúc cơ sở dữ liệu y tế, phân quyền nghiệp vụ (RBAC, VPD, dữ liệu mẫu, view, procedure...) đều được tạo dưới schema và quản lý bởi tài khoản `CQ09`. Đây là tài khoản dùng để đăng nhập và vận hành chính.
> - **`SYS`**: Là tài khoản **Quản trị hệ thống Oracle (System DBA)**. Tài khoản này **không can thiệp sâu** vào dữ liệu nghiệp vụ của dự án, mà chỉ được sử dụng cho các cấu hình cấp hệ thống/database ban đầu (kích hoạt OLS, thiết lập tham số Audit, tạo user `CQ09` và cấp quyền DBA cho `CQ09`).

### Hướng dẫn thiết lập Cơ sở dữ liệu (Chạy thủ công từng bước)
> **HƯỚNG DẪN CHẠY TRÊN SQL DEVELOPER**

Hãy chạy các file trong thư mục `script SQL/` theo đúng thứ tự và sử dụng đúng tài khoản kết nối sau:

| Thứ tự | File chạy | Tài khoản kết nối | Chức năng chi tiết |
| :---: | :--- | :---: | :--- |
| **1** | `01_StoredProcedures.sql` | **`SYS AS SYSDBA`** | Khởi tạo schema `CQ09`, cấp các quyền Admin và tạo các thủ tục (Stored Procedure) quản trị user/role/grant. |
| **2** | `02_schema_data.sql` | **`CQ09`** | Tạo các bảng nghiệp vụ (`NHANVIEN`, `BENHNHAN`, `HSBA`, `HSBA_DV`, `DONTHUOC`, `THONGBAO`), tạo index và nạp dữ liệu mẫu lớn. |
| **3** | `03_role.sql` | **`CQ09`** | Khởi tạo các vai trò (`RL_DIEUPHOI`, `RL_BACSI`,...) và tự động tạo tài khoản database tương ứng cho từng nhân viên/bệnh nhân. |
| **4** | `04_RBAC.sql` | **`CQ09`** | Tạo các view bảo mật cơ bản (`vw_benhnhan`, `vw_nhanvien_canhan`,...) và cấp quyền SELECT, UPDATE có giới hạn cột cho các Role (RBAC truyền thống). |
| **5** | `05_VPD.sql` | **`CQ09`** | Áp dụng chính sách kiểm soát truy cập mức dòng/cột nâng cao bằng VPD (Virtual Private Database) lên các bảng gốc để chống bypass. |
| **6** | `06_OLS_setup.sql` | **`SYS AS SYSDBA`** | Cấu hình Oracle Label Security trên bảng `THONGBAO` phục vụ phát tán thông tin khẩn cấp theo nhãn (Level, Compartment, Group) cho user `u1` - `u8`. |
| **7** | `07_audit_setup.sql` | **`SYS AS SYSDBA`** | Cấu hình Standard Audit và Fine-Grained Audit (FGA) hệ thống để ghi lại nhật ký khi có các thao tác nhạy cảm trên dữ liệu y tế. |

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
* **SYSDBA**: **Không tích chọn** ô này khi đăng nhập bằng tài khoản quản trị dự án **`CQ09`** hoặc các user nghiệp vụ. Chỉ tích chọn khi đăng nhập bằng tài khoản **`SYS`** (khi thật sự cần thiết).

---

## 🔑 PHẦN 3: DANH SÁCH TÀI KHOẢN DEMO & GIAO DIỆN TƯƠNG ỨNG

Ứng dụng WinForms tự động nhận diện vai trò của tài khoản đăng nhập để mở màn hình chức năng phù hợp:

| Tài khoản | Mật khẩu | Vai trò hệ thống | Màn hình hiển thị | Tính năng chính cần demo |
| :--- | :---: | :---: | :--- | :--- |
| **`CQ09`** | `ATBM123` | **Project DBA (Quản trị chính)** | `MainForm` | Quản trị dự án: tạo/khóa/xóa User nghiệp vụ, tạo Role nghiệp vụ, cấp/thu hồi quyền hệ thống và quyền đối tượng trên schema, xem log kiểm toán (Standard & FGA). *(Khuyến nghị sử dụng tài khoản này)* |
| **`SYS`** | *(Theo máy)* | **System DBA (Hệ thống)** | `MainForm` | Quản trị viên cấp cao nhất của hệ thống database. *(SYS không can thiệp sâu vào các luồng nghiệp vụ của dự án)* |
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
1. Chạy kịch bản tạo log nghiệp vụ: Mở SQL Developer đăng nhập bằng các tài khoản tương ứng hoặc chạy script `@script SQL/08_audit_test_actions.sql`.
2. Đọc log kiểm toán: Đăng nhập ứng dụng bằng **`CQ09`** hoặc chạy script `@script SQL/09_audit_read_logs.sql` dưới tài khoản **`CQ09`** để xem lịch sử Standard & FGA Audit.
3. Demo sự cố khôi phục đơn thuốc:
   * **Tạo sự cố**: Đăng nhập app bằng bác sĩ `BS001` và thực hiện sửa sai liều dùng của một đơn thuốc (hoặc chạy trực tiếp bằng tài khoản bác sĩ).
   * **Khôi phục qua giao diện**: Đăng nhập app bằng tài khoản **`CQ09`**, chọn tab `8. Recovery`, bấm `Tải audit`, chọn dòng audit sửa sai đơn thuốc tương ứng và bấm `Restore audit đã chọn` để khôi phục nhanh liều thuốc ban đầu bằng Flashback Query.
   * **Khôi phục thủ công bằng SQL**: Nếu không dùng giao diện ứng dụng, đăng nhập SQL Developer bằng tài khoản **`CQ09`** và chạy script `@script SQL/Backup_recovery/02_demo_recovery.sql` để tìm mốc sự cố, xem trước dữ liệu cũ và tiến hành khôi phục.

---

## 🛠️ PHẦN 5: CÁC LỖI THƯỜNG GẶP VÀ CÁCH XỬ LÝ
| Lỗi gặp phải | Nguyên nhân | Cách xử lý |
| :--- | :--- | :--- |
| **`SP2-0310: Unable to open file...`** | Chạy lệnh `@` khi chưa mở trực tiếp file script hoặc sai thư mục hoạt động. | Dùng **File -> Open** mở trực tiếp file script cần chạy trong SQL Developer trước khi nhấn **F5**. |
| **`ORA-01031: insufficient privileges`** | Chạy script bằng tài khoản không có quyền DBA/SYSDBA. | Đảm bảo kết nối bằng đúng tài khoản **`SYS AS SYSDBA`** ở các bước 1, 6 và 7. |
| **Không thấy log kiểm toán** | Oracle chưa bật tham số ghi nhận log `audit_trail`. | Chạy lệnh `ALTER SYSTEM SET audit_trail = DB, EXTENDED SCOPE = SPFILE;` bằng `SYS AS SYSDBA` rồi **khởi động lại database**. |
| **OLS không lọc dữ liệu** | Tính năng OLS chưa được kích hoạt trên database. | Chạy script `06_OLS_setup.sql` bằng `SYS AS SYSDBA` để bật cấu hình OLS và thiết lập chính sách nhãn. |
