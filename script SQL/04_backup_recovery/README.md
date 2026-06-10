# Yêu cầu 4 - Backup và Recovery

Thư mục này chứa kịch bản backup, tạo sự cố, đọc audit log và phục hồi dữ liệu cho schema `CQ09`.

## Mục tiêu demo

- Backup schema `CQ09` bằng Oracle Data Pump.
- Tạo một sự cố sửa sai dữ liệu đơn thuốc.
- Dùng audit/FGA để xác định user, SQL và thời điểm xảy ra sự cố.
- Dùng Flashback Query để khôi phục dữ liệu về trạng thái trước sự cố.
- Cung cấp thêm lệnh RMAN tham khảo cho backup/recovery cấp database.

## File trong thư mục

| File | Mục đích |
|---|---|
| `00_prepare_backup_privileges.sql` | Tạo directory backup và cấp quyền cần thiết cho `CQ09` |
| `01_expdp_backup.bat` | Export schema `CQ09` bằng Data Pump |
| `02_impdp_restore.bat` | Import lại schema/table từ dump |
| `03_demo_su_co.sql` | Tạo sự cố demo: bác sĩ `BS001` sửa sai liều dùng đơn thuốc |
| `04_check_audit_log.sql` | Đọc audit/FGA log để tìm thời điểm và SQL gây lỗi |
| `05_flashback_restore.sql` | Khôi phục dữ liệu đơn thuốc bằng Flashback Query |
| `06_rman_commands.txt` | Lệnh RMAN tham khảo |
| `README.md` | Hướng dẫn này |

## Điều kiện trước khi chạy

Đã chạy các script chính ở thư mục `script SQL/`:

```sql
@"script SQL/StoredProcedures.sql"
@"script SQL/schema_data.sql"
@"script SQL/role.sql"
@"script SQL/VPD.sql"
@"script SQL/03_audit_setup.sql"
```

Audit nên được bật:

```sql
SHOW PARAMETER audit_trail;
```

Nếu kết quả là `NONE`, chạy bằng `SYS AS SYSDBA`:

```sql
ALTER SYSTEM SET audit_trail = DB, EXTENDED SCOPE = SPFILE;
```

Sau đó restart Oracle và chạy lại `03_audit_setup.sql`.

## Thứ tự demo đề xuất

### Bước 1 - Chuẩn bị quyền backup

Đăng nhập `SYS AS SYSDBA` và chạy:

```sql
@"script SQL/04_backup_recovery/00_prepare_backup_privileges.sql"
```

Script này chuẩn bị directory Data Pump và quyền cần thiết để export/import schema `CQ09`.

### Bước 2 - Backup schema CQ09

Chạy file batch:

```bat
"script SQL\04_backup_recovery\01_expdp_backup.bat"
```

Kết quả mong đợi:

- Có file dump `.dmp`.
- Có file log `.log`.
- Log export không có lỗi nghiêm trọng.

### Bước 3 - Tạo sự cố demo

Đảm bảo đã chạy audit setup, sau đó chạy:

```sql
@"script SQL/04_backup_recovery/03_demo_su_co.sql"
```

Sự cố demo: user/bác sĩ cập nhật sai cột `LIEUDUNG` trong bảng `CQ09.DONTHUOC`.

### Bước 4 - Kiểm tra audit log

Chạy:

```sql
@"script SQL/04_backup_recovery/04_check_audit_log.sql"
```

Ghi lại:

- User thực hiện.
- Thời điểm audit.
- SQL text.
- Dòng dữ liệu bị ảnh hưởng.

Thông tin quan trọng nhất là thời điểm ngay trước khi sự cố xảy ra.

### Bước 5 - Khôi phục bằng Flashback Query

Mở file:

```text
05_flashback_restore.sql
```

Sửa biến thời điểm restore theo kết quả audit, ví dụ:

```sql
DEFINE RESTORE_TS = "2026-06-09 14:30:00"
```

Sau đó chạy:

```sql
@"script SQL/04_backup_recovery/05_flashback_restore.sql"
```

Kiểm tra lại dữ liệu `CQ09.DONTHUOC` để xác nhận liều dùng đã được phục hồi.

### Bước 6 - Import từ dump khi cần

Nếu cần phục hồi từ file backup Data Pump:

```bat
"script SQL\04_backup_recovery\02_impdp_restore.bat"
```

Tùy tình huống demo, có thể import toàn schema hoặc chỉnh file batch để import một bảng cụ thể.

## Lệnh kiểm tra nhanh sau phục hồi

```sql
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = 'HSBA2024001'
ORDER BY NGAYDT, TENTHUOC;
```

Đọc log audit:

```sql
SELECT db_user,
       object_schema,
       object_name,
       policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
ORDER BY timestamp DESC;
```

## So sánh phương pháp

| Phương pháp | Ưu điểm | Hạn chế |
|---|---|---|
| Data Pump `expdp/impdp` | Dễ demo, phù hợp backup schema/table, có dump rõ ràng | Không phải point-in-time recovery chính xác nếu không có dump đúng thời điểm |
| Flashback Query | Phục hồi nhanh một số dòng về thời điểm trước sự cố | Phụ thuộc undo retention, không thay thế backup thật |
| RMAN | Chuẩn Oracle cho backup/recovery toàn database | Cấu hình phức tạp hơn, cần quản lý archive log nếu muốn point-in-time recovery |

## Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| `ORA-39087 directory name is invalid` | Directory Data Pump chưa tạo hoặc sai tên | Chạy lại `00_prepare_backup_privileges.sql` |
| `ORA-01031 insufficient privileges` | User thiếu quyền export/import hoặc flashback | Chạy bằng SYS/DBA hoặc cấp thêm quyền |
| Không có audit log | Chưa bật `audit_trail` hoặc chưa chạy audit setup | Bật audit, restart Oracle, chạy lại `03_audit_setup.sql` |
| Flashback không thấy dữ liệu cũ | Undo retention không đủ | Dùng dump backup hoặc giảm khoảng thời gian giữa sự cố và restore |
| Import đè dữ liệu lỗi | Chế độ import chưa đúng | Kiểm tra tham số `TABLE_EXISTS_ACTION` trong file `.bat` |

## Kết luận demo

Kịch bản nộp nên trình bày theo chuỗi:

1. Có backup Data Pump trước sự cố.
2. Sự cố sửa sai dữ liệu được audit lại.
3. Audit log giúp xác định thời điểm và câu SQL gây lỗi.
4. Flashback Query khôi phục nhanh dòng bị sai.
5. Data Pump vẫn là phương án phục hồi dự phòng khi cần restore từ dump.
