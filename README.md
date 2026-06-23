# Project ATBM CSDL - Phân hệ 2

Repo này chứa mã nguồn và script Oracle cho phân hệ 2 của đồ án An toàn bảo mật hệ thống thông tin: ứng dụng quản lý dữ liệu y tế có phân quyền, VPD, OLS, audit và backup/recovery.

## Thành phần chính

- `WindowsFormsApp1/`: ứng dụng WinForms .NET Framework.
- `script SQL/`: script tạo schema, user/role, VPD, OLS, audit.
- `script SQL/Backup_recovery/`: script backup, restore, flashback và demo sự cố.
- `HUONG_DAN_SU_DUNG.md`: hướng dẫn chạy đầy đủ từ setup Oracle đến demo ứng dụng.

## Chức năng đã triển khai

- Quản trị DBA: tạo/sửa/xóa user, role, grant, revoke, xem quyền, object browser.
- Điều phối viên: quản lý bệnh nhân, tạo HSBA, phân công bác sĩ và kỹ thuật viên.
- Bác sĩ/y sĩ: cập nhật HSBA, đơn thuốc, chỉ định dịch vụ, thông tin y khoa bệnh nhân.
- Kỹ thuật viên: xem dịch vụ được phân công và cập nhật kết quả.
- Bệnh nhân: xem/cập nhật thông tin cá nhân được phép.
- OLS demo: user `u1` đến `u8` xem thông báo theo nhãn bảo mật.
- Audit/FGA: ghi nhận thao tác nhạy cảm.
- Backup/recovery: Data Pump, flashback restore và kịch bản demo sự cố.

## Yêu cầu môi trường

- Windows.
- Visual Studio có hỗ trợ WinForms .NET Framework.
- .NET Framework 4.8.
- Oracle Database với PDB `XEPDB1`.
- SQL Developer hoặc SQL*Plus.

## Thứ tự setup (Chạy thủ công)

> [!IMPORTANT]
> **Phân định vai trò quản trị:**
> - **`CQ09`**: Là tài khoản **Quản trị viên trực tiếp của dự án** (Project DBA / Schema Owner). Toàn bộ cấu trúc cơ sở dữ liệu y tế, phân quyền nghiệp vụ (RBAC, VPD, dữ liệu, view, procedure...) đều được tạo dưới schema và quản lý bởi tài khoản `CQ09`.
> - **`SYS`**: Là tài khoản **Quản trị hệ thống Oracle (System DBA)**. Tài khoản này **không can thiệp sâu** vào dữ liệu nghiệp vụ của dự án, mà chỉ được sử dụng cho các bước cấu hình cấp hệ thống ban đầu (kích hoạt OLS, thiết lập tham số Audit hệ thống, tạo user `CQ09` và cấp quyền DBA cho `CQ09`).

Hãy mở SQL Developer hoặc SQL*Plus và chạy lần lượt các script trong thư mục `script SQL/` theo đúng thứ tự đánh số sau (chú ý đăng nhập bằng tài khoản tương ứng):

```sql
-- Bước 1: Đăng nhập bằng tài khoản SYS AS SYSDBA
-- Khởi tạo schema CQ09, cấp các quyền Admin và tạo các thủ tục quản trị
@"script SQL/01_StoredProcedures.sql"

-- Bước 2: Đăng nhập bằng tài khoản CQ09
-- Tạo các bảng nghiệp vụ, tạo index và nạp dữ liệu mẫu
@"script SQL/02_schema_data.sql"

-- Bước 3: Đăng nhập bằng tài khoản CQ09
-- Khởi tạo các vai trò nghiệp vụ và tạo tài khoản database cho nhân sự/bệnh nhân
@"script SQL/03_role.sql"

-- Bước 4: Đăng nhập bằng tài khoản CQ09
-- Tạo các view bảo mật cơ bản và cấp quyền SELECT, UPDATE có giới hạn cho các Role
@"script SQL/04_RBAC.sql"

-- Bước 5: Đăng nhập bằng tài khoản CQ09
-- Áp dụng chính sách kiểm soát dòng/cột VPD lên các bảng dữ liệu gốc
@"script SQL/05_VPD.sql"

-- Bước 6: Đăng nhập bằng tài khoản SYS AS SYSDBA
-- Cấu hình Oracle Label Security (OLS) để phân nhãn bảo mật trên bảng THONGBAO
@"script SQL/06_OLS_setup.sql"

-- Bước 7: Đăng nhập bằng tài khoản SYS AS SYSDBA
-- Cấu hình Standard Audit và Fine-Grained Audit (FGA) hệ thống
@"script SQL/07_audit_setup.sql"
```

Sau đó mở project:

```text
WindowsFormsApp1/WindowsFormsApp1.csproj
```

Build và chạy ứng dụng `NHOM09`.

## Tài khoản demo

| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `CQ09` | `ATBM123` | Quản trị dự án (Project DBA / Schema Owner) |
| `SYS` | theo Oracle local | Quản trị hệ thống Oracle (System DBA - chỉ dùng setup) |
| `NV001` | `ATBM123` | Điều phối viên |
| `BS001` | `ATBM123` | Bác sĩ/Y sĩ |
| `KTV01` | `ATBM123` | Kỹ thuật viên |
| `BN000001` | `ATBM123` | Bệnh nhân |
| `u1` - `u8` | `ATBM123` | OLS demo |

## Hướng dẫn chi tiết

Đọc file:

```text
HUONG_DAN_SU_DUNG.md
```

Phần backup/recovery có hướng dẫn riêng:

```text
script SQL/Backup_recovery/README.md
```

## Lưu ý

Các quyền rộng như `DBA`, `SELECT ANY DICTIONARY`, `GRANT ANY PRIVILEGE` được dùng cho môi trường lab/demo đồ án. Không áp dụng nguyên trạng cho môi trường production.
