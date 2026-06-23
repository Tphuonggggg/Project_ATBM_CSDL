# Project ATBM CSDL - Phân hệ 2

Repo này chứa mã nguồn và script Oracle cho phân hệ 2 của đồ án An toàn bảo mật hệ thống thông tin: ứng dụng quản lý dữ liệu y tế có phân quyền, VPD, OLS, audit và backup/recovery.

## Thành phần chính

- `WindowsFormsApp1/`: ứng dụng WinForms .NET Framework.
- `script SQL/`: script tạo schema, user/role, VPD, OLS, audit.
- `script SQL/04_backup_recovery/`: script backup, restore, flashback và demo sự cố.
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

## Thứ tự setup nhanh

Bạn có thể chạy toàn bộ quy trình thiết lập môi trường bằng cách đăng nhập bằng tài khoản `SYS AS SYSDBA` và chạy file tổng hợp:

```sql
@"script SQL/00_run_all.sql"
```

> [!NOTE]
> **Phân định vai trò:** `SYS AS SYSDBA` chỉ dùng cho thiết lập hệ thống ban đầu (chạy `00_run_all.sql`). Khi hệ thống đã dựng xong, toàn bộ cấu trúc CSDL y tế, bảng biểu, VPD, RBAC đều được tạo và sở hữu bởi **`CQ09`**. Tài khoản `SYS` sẽ không can thiệp sâu vào các công việc quản lý nghiệp vụ và vận hành dự án.

Hoặc chạy thủ công các script Oracle theo đúng thứ tự đánh số sau (chú ý tài khoản tương ứng):

```sql
-- Đăng nhập bằng SYS AS SYSDBA
@"script SQL/01_StoredProcedures.sql"

-- Đăng nhập bằng CQ09 (Quản trị viên dự án)
@"script SQL/02_schema_data.sql"
@"script SQL/03_role.sql"
@"script SQL/04_RBAC.sql"
@"script SQL/05_VPD.sql"

-- Đăng nhập bằng SYS AS SYSDBA (Cấu hình OLS & Audit cấp hệ thống)
@"script SQL/06_OLS_setup.sql"
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
script SQL/04_backup_recovery/README.md
```

## Lưu ý

Các quyền rộng như `DBA`, `SELECT ANY DICTIONARY`, `GRANT ANY PRIVILEGE` được dùng cho môi trường lab/demo đồ án. Không áp dụng nguyên trạng cho môi trường production.
