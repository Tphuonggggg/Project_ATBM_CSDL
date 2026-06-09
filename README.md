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

Chạy các script Oracle theo thứ tự:

```sql
@"script SQL/StoredProcedures.sql"
@"script SQL/schema_data.sql"
@"script SQL/role.sql"
@"script SQL/VPD.sql"
@"script SQL/OLS_setup.sql"
@"script SQL/03_audit_setup.sql"
```

Sau đó mở project:

```text
WindowsFormsApp1/WindowsFormsApp1.csproj
```

Build và chạy ứng dụng `NHOM09`.

## Tài khoản demo

| Tài khoản | Mật khẩu | Vai trò |
|---|---|---|
| `SYS` | theo Oracle local | DBA/setup |
| `CQ09` | `ATBM123` | Schema quản trị demo |
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
